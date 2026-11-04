using System.Collections.Generic;
using RoR2;
using StageTracker.Util;
using UnityEngine;

namespace StageTracker.Tracking
{
    public sealed class DroneData
    {
        public int TurretAvailable;
        public int TurretTotal;

        public int TripleShopAvailable;
        public int TripleShopTotal;

        public bool DroneScrapperPresent;
        public bool DroneCombinerPresent;

        public readonly List<BrokenGroup> Broken = new List<BrokenGroup>();

        public readonly struct BrokenGroup
        {
            public BrokenGroup(int droneIndex, string label, string colorHex, int available, int total)
            {
                DroneIndex = droneIndex;
                Label = label;
                ColorHex = colorHex;
                Available = available;
                Total = total;
            }

            public int DroneIndex { get; }
            public string Label { get; }
            public string ColorHex { get; }
            public int Available { get; }
            public int Total { get; }
        }
    }

    public static class DroneScan
    {
        // Broken (repairable) drones are plain interactable prefabs, not character bodies. Each
        // DroneDef knows the spawn card of its broken variant, so the prefab name identifies the
        // drone type — this also covers combiner-upgraded variants registered in the catalog.
        private static readonly Dictionary<string, DroneDef> BrokenPrefabMap = new Dictionary<string, DroneDef>();
        private static bool BrokenPrefabMapBuilt;

        // Unmapped drone interactables already logged this session (keeps the log tidy).
        private static readonly HashSet<string> _loggedFallbacks = new HashSet<string>();

        private static void EnsureBrokenPrefabMap()
        {
            if (BrokenPrefabMapBuilt)
            {
                return;
            }

            BrokenPrefabMapBuilt = true;
            foreach (var def in DroneCatalog.allDroneDefs)
            {
                if (!def)
                {
                    continue;
                }

                var card = def.droneBrokenSpawnCard;
                var prefab = card ? card.prefab : null;
                if (!prefab)
                {
                    continue;
                }

                string prefabName = StripClone(prefab.name);
                if (!BrokenPrefabMap.ContainsKey(prefabName))
                {
                    BrokenPrefabMap[prefabName] = def;
                }
            }
        }

        private static string StripClone(string name)
        {
            return name != null && name.EndsWith("(Clone)") ? name.Substring(0, name.Length - 7) : name;
        }

        public static DroneData Scan()
        {
            EnsureBrokenPrefabMap();

            var data = new DroneData();
            var brokenMap = new SortedDictionary<int, (string label, string colorHex, int available, int total)>();
            var fallbackMap = new SortedDictionary<string, (int available, int total)>();

            var purchases = InstanceTracker.GetInstancesList<PurchaseInteraction>();
            for (int i = 0; i < purchases.Count; i++)
            {
                var purchase = purchases[i];
                if (!purchase)
                {
                    continue;
                }

                var go = purchase.gameObject;
                string token = purchase.displayNameToken;

                if (token != null && token.StartsWith("TURRET1"))
                {
                    data.TurretTotal++;
                    if (purchase.available)
                    {
                        data.TurretAvailable++;
                    }
                    continue;
                }

                if (go.GetComponentInChildren<DroneVendorTerminalBehavior>(true) || go.GetComponentInChildren<DroneVendorMultiShopController>(true) || token == "DRONE_VENDOR_TERMINAL_NAME")
                {
                    data.TripleShopTotal++;
                    if (purchase.available)
                    {
                        data.TripleShopAvailable++;
                    }
                    continue;
                }

                if (go.GetComponentInChildren<DroneScrapperController>(true) || token == "DRONE_SCRAPPER_NAME")
                {
                    data.DroneScrapperPresent = true;
                    continue;
                }

                if (go.GetComponentInChildren<DroneCombinerController>(true))
                {
                    data.DroneCombinerPresent = true;
                    continue;
                }

                // Broken drone? Match the prefab against the catalog's broken spawn cards.
                DroneDef def = BrokenPrefabMap.TryGetValue(StripClone(go.name), out var mapped) ? mapped : null;
                if (def)
                {
                    brokenMap.TryGetValue((int)def.droneIndex, out var bucket);
                    bucket.total++;
                    if (purchase.available)
                    {
                        bucket.available++;
                    }

                    if (bucket.label == null)
                    {
                        bucket.label = Lang.TryGet(def.nameToken) ?? def.name;
                        bucket.colorHex = def.colorOverride != ColorCatalog.ColorIndex.None
                            ? Palette.Hex(def.colorOverride)
                            : Palette.Hex(def.tier);
                    }

                    brokenMap[(int)def.droneIndex] = bucket;
                    continue;
                }

                // Drone-flavored interactable the catalog doesn't know: group by display name so it
                // still shows up, and log it once so unmapped drones are easy to add later.
                bool droneFlavored = purchase.isDrone || (token != null && token.Contains("DRONE"));
                if (droneFlavored)
                {
                    string displayName = purchase.GetDisplayName();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = go.name;
                    }

                    if (_loggedFallbacks.Add(displayName))
                    {
                        Plugin.Log.LogInfo($"Unmapped drone interactable '{displayName}' (token='{token}', go='{go.name}') — shown gray in the drone panel.");
                    }

                    fallbackMap.TryGetValue(displayName, out var fallback);
                    fallback.total++;
                    if (purchase.available)
                    {
                        fallback.available++;
                    }
                    fallbackMap[displayName] = fallback;
                }
            }

            foreach (var pair in brokenMap)
            {
                data.Broken.Add(new DroneData.BrokenGroup(pair.Key, pair.Value.label, pair.Value.colorHex ?? Palette.Gray, pair.Value.available, pair.Value.total));
            }

            foreach (var pair in fallbackMap)
            {
                data.Broken.Add(new DroneData.BrokenGroup(int.MaxValue, pair.Key, Palette.Gray, pair.Value.available, pair.Value.total));
            }

            return data;
        }
    }
}
