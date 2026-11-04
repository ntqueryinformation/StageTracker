using System.Collections.Generic;
using System.Reflection;
using RoR2;
using StageTracker.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace StageTracker.Tracking
{
    /// <summary>Snapshot of everything trackable on the current stage.</summary>
    public sealed class LootData
    {
        public readonly Dictionary<Cat, (int available, int total)> Counts = new Dictionary<Cat, (int, int)>();

        /// <summary>Interactables without a known category, grouped by display name (future-proofing).</summary>
        public readonly List<(string label, int available, int total)> Unknown = new List<(string, int, int)>();

        /// <summary>Printer counts per cost tier: white, green, red, boss, void.</summary>
        public readonly int[] PrinterAvailable = new int[5];
        public readonly int[] PrinterTotal = new int[5];

        /// <summary>Bazaar cauldron counts per cost tier: white, green, red, boss, void.</summary>
        public readonly int[] CauldronAvailable = new int[5];
        public readonly int[] CauldronTotal = new int[5];

        public int MonsterEnemies;
        public int VoidEnemies;
        public int Invitations;

        /// <summary>Chance-shrine purchases left, host only (purchase counts are not networked).</summary>
        public int ChancePurchasesLeft;
        public bool IsHost;

        public string StageName;
    }

    public static class LootScan
    {
        private static readonly Dictionary<string, Cat> TokenMap = new Dictionary<string, Cat>
        {
            ["CHEST1_STEALTHED_NAME"] = Cat.CloakedChest,
            ["CASINOCHEST_NAME"] = Cat.AdaptiveChest,
            ["EQUIPMENTBARREL_NAME"] = Cat.EquipmentBarrel,
            ["LOCKBOX_NAME"] = Cat.RustyLockbox,
            ["VOIDLOCKBOX_NAME"] = Cat.EncrustedCache,
            ["SHRINE_CHANCE_NAME"] = Cat.ShrineChance,
            ["SHRINE_CLEANSE_NAME"] = Cat.CleansingPool,
            ["LUNAR_CHEST_NAME"] = Cat.LunarPod,
            ["LUNARSEER_NAME"] = Cat.LunarSeer,
            ["VOID_CHEST_NAME"] = Cat.VoidChests,
            ["VOID_TRIPLE_NAME"] = Cat.VoidChests,
        };

        // Private counters only exist server-side; read through reflection so a future rename fails soft.
        private static readonly FieldInfo ChanceSuccessField =
            typeof(ShrineChanceBehavior).GetField("successfulPurchaseCount", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo BossPurchaseField =
            typeof(ShrineBossBehavior).GetField("purchaseCount", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>Drone-panel territory: flagged drones plus gunner turrets (which are not flagged isDrone).</summary>
        private static bool BelongsToDronePanel(PurchaseInteraction purchase)
        {
            if (purchase.isDrone)
            {
                return true;
            }

            string token = purchase.displayNameToken;
            return token != null && (token.StartsWith("TURRET1") || token == "DRONE_SCRAPPER_NAME");
        }

        public static LootData Scan()
        {
            var data = new LootData();
            data.IsHost = NetworkServer.active;

            var unknownMap = new Dictionary<string, (int available, int total)>();
            var purchases = InstanceTracker.GetInstancesList<PurchaseInteraction>();

            for (int i = 0; i < purchases.Count; i++)
            {
                var purchase = purchases[i];
                if (!purchase || BelongsToDronePanel(purchase))
                {
                    continue; // drones (and turrets) are the drone panel's job
                }

                var category = Classify(purchase);
                if (category == null)
                {
                    string displayName = purchase.GetDisplayName();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = purchase.gameObject.name;
                    }

                    if (_loggedUnknowns.Add(displayName))
                    {
                        Plugin.Log.LogInfo($"Uncategorized interactable '{displayName}' (token='{purchase.displayNameToken}', cost={purchase.costType}, go='{purchase.gameObject.name}') — it will show gray under Other until it gets its own category.");
                    }

                    unknownMap.TryGetValue(displayName, out var bucket);
                    bucket.total++;
                    if (purchase.available)
                    {
                        bucket.available++;
                    }
                    unknownMap[displayName] = bucket;
                    continue;
                }

                if (category == Cat.Printers || category == Cat.Cauldrons)
                {
                    int bucket = Palette.CostBucket(purchase.costType, purchase.gameObject.name);
                    var totals = category == Cat.Printers ? data.PrinterTotal : data.CauldronTotal;
                    var availables = category == Cat.Printers ? data.PrinterAvailable : data.CauldronAvailable;
                    totals[bucket]++;
                    if (purchase.available)
                    {
                        availables[bucket]++;
                    }
                    continue;
                }

                data.Counts.TryGetValue(category.Value, out var entry);
                entry.total++;
                if (purchase.available)
                {
                    entry.available++;
                }
                data.Counts[category.Value] = entry;
            }

            foreach (var pair in unknownMap)
            {
                data.Unknown.Add((pair.Key, pair.Value.available, pair.Value.total));
            }
            data.Unknown.Sort((a, b) => string.CompareOrdinal(a.label, b.label));

            // Chance shrine purchases: server only, the SyncVar counter isn't replicated.
            if (data.IsHost && ChanceSuccessField != null)
            {
                var shrines = InstanceTracker.GetInstancesList<ShrineChanceBehavior>();
                for (int i = 0; i < shrines.Count; i++)
                {
                    var shrine = shrines[i];
                    if (!shrine)
                    {
                        continue;
                    }

                    int successful = (int)ChanceSuccessField.GetValue(shrine);
                    int left = shrine.maxPurchaseCount - successful;
                    if (left > 0)
                    {
                        data.ChancePurchasesLeft += left;
                    }
                }
            }

            // Mountain shrine invitations still to claim.
            var bosses = InstanceTracker.GetInstancesList<ShrineBossBehavior>();
            for (int i = 0; i < bosses.Count; i++)
            {
                var shrine = bosses[i];
                if (!shrine)
                {
                    continue;
                }

                int used = BossPurchaseField != null ? (int)BossPurchaseField.GetValue(shrine) : 0;
                int left = shrine.maxPurchaseCount - used;
                if (left > 0)
                {
                    data.Invitations += left;
                }
            }

            data.MonsterEnemies = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;
            data.VoidEnemies = TeamComponent.GetTeamMembers(TeamIndex.Void).Count;

            var sceneInfo = SceneInfo.instance;
            if (sceneInfo && sceneInfo.sceneDef)
            {
                data.StageName = Language.GetString(sceneInfo.sceneDef.nameToken);
            }

            return data;
        }

        // Interactables logged as uncategorized at least once this session (keeps the log tidy).
        private static readonly HashSet<string> _loggedUnknowns = new HashSet<string>();

        private static Cat? Classify(PurchaseInteraction purchase)
        {
            // Token-based classification first — cheap and stable across updates.
            if (!string.IsNullOrEmpty(purchase.displayNameToken))
            {
                if (TokenMap.TryGetValue(purchase.displayNameToken, out var byToken))
                {
                    return byToken;
                }

                if (purchase.displayNameToken.StartsWith("DUPLICATOR"))
                {
                    return Cat.Printers;
                }

                if (purchase.displayNameToken == "BAZAAR_CAULDRON_NAME")
                {
                    return Cat.Cauldrons;
                }

                if (purchase.displayNameToken.StartsWith("SHRINE_BOSS"))
                {
                    return Cat.ShrineMountain;
                }
            }

            if (purchase.isGoldShrine)
            {
                return Cat.AltarOfGold;
            }

            // Component-based classification — catches newer content regardless of its token string.
            // GetComponentInChildren: some prefabs keep the behaviour on the root while the
            // PurchaseInteraction sits on a child (multishop terminals and the Newt Altar both do).
            var go = purchase.gameObject;
            if (go.GetComponentInChildren<ShrineCompulsionBehaviour>(true)) return Cat.ShrineCompulsion;
            if (go.GetComponentInChildren<HallowingShrineSplitBase>(true)) return Cat.ShrineHallowed;
            if (go.GetComponentInChildren<ShrineRebirthController>(true)) return Cat.ShrineRebirth;
            if (go.GetComponentInChildren<HalcyoniteShrineInteractable>(true)) return Cat.ShrineHalcyon;
            if (go.GetComponentInChildren<ShrineCombatTroopBehavior>(true)) return Cat.ShrineCombatCollective;
            if (go.GetComponentInChildren<ShrineCombatBehavior>(true)) return Cat.ShrineCombat;
            if (go.GetComponentInChildren<ShrineDeathBehavior>(true)) return Cat.ShrineCombat;
            if (go.GetComponentInChildren<ShrineHealingBehavior>(true)) return Cat.ShrineWoods;
            if (go.GetComponentInChildren<ShrineRestackBehavior>(true)) return Cat.ShrineOrder;
            if (go.GetComponentInChildren<ShrineBloodBehavior>(true)) return Cat.ShrineBlood;
            if (go.GetComponentInChildren<ShrineChanceBehavior>(true)) return Cat.ShrineChance;
            if (go.GetComponentInChildren<ShrineBossBehavior>(true)) return Cat.ShrineMountain;
            if (go.GetComponentInChildren<ShrineCleanseBehavior>(true)) return Cat.CleansingPool;
            if (go.GetComponentInChildren<SolusVendorShrineController>(true)) return Cat.TempItemDistributors;
            if (go.GetComponentInChildren<ScrapperController>(true)) return Cat.Scrapper;
            if (go.GetComponentInChildren<BarrelInteraction>(true)) return Cat.EquipmentBarrel;
            if (go.GetComponentInChildren<PortalStatueBehavior>(true)) return Cat.NewtAltar; // the Newt Altar is the blue portal statue
            if (go.GetComponentInChildren<DroneVendorTerminalBehavior>(true)) return null; // drone panel
            if (go.GetComponentInChildren<DroneVendorMultiShopController>(true)) return null; // drone panel
            if (go.GetComponentInChildren<DroneCombinerController>(true)) return null; // drone panel
            if (go.GetComponentInChildren<DroneScrapperController>(true)) return null; // drone panel

            if (go.GetComponentInChildren<MultiShopController>(true))
            {
                if (go.name.Contains("Equipment"))
                {
                    return Cat.EquipmentMultishop;
                }

                return Cat.Multishop;
            }

            if (go.name.Contains("SecurityChest") || go.name.Contains("TimedChest"))
            {
                return Cat.SecurityChest;
            }

            if (go.name.Contains("ScavengerBackpack"))
            {
                return Cat.ScavengerPack;
            }

            // Chests are split by their ChestBehavior data: guaranteed tier chances separate
            // small/large/legendary, and requiredItemTag identifies the category (damage/healing/
            // utility) chests. The crashed multishop "capsules" use the dedicated free-chest table.
            var chest = go.GetComponentInChildren<ChestBehavior>(true);
            if (chest)
            {
                if (chest.dropTable is FreeChestDropTable)
                {
                    return Cat.CrashedMultishop;
                }

                switch (chest.requiredItemTag)
                {
                    case ItemTag.Damage: return Cat.ChestDamage;
                    case ItemTag.Healing: return Cat.ChestHealing;
                    case ItemTag.Utility: return Cat.ChestUtility;
                }

                if (chest.tier3Chance >= 0.99f)
                {
                    return Cat.ChestLegendary;
                }

                if (chest.tier2Chance >= 0.99f)
                {
                    return Cat.ChestLarge;
                }

                return Cat.ChestSmall;
            }

            // Last resort: match by the resolved English display name.
            var displayName = purchase.GetDisplayName();
            if (!string.IsNullOrEmpty(displayName) && Categories.DisplayNameMap.TryGetValue(displayName, out var byName))
            {
                return byName;
            }

            return null;
        }
    }
}
