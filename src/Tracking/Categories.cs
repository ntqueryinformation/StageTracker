using System.Collections.Generic;
using RoR2;
using StageTracker.Util;

namespace StageTracker.Tracking
{
    /// <summary>Every loot category the panel can show. Each has a config toggle (ModConfig).</summary>
    public enum Cat
    {
        // Chests & terminals
        Multishop,
        EquipmentMultishop,
        CrashedMultishop,
        ChestSmall,
        ChestLarge,
        ChestLegendary,
        ChestDamage,
        ChestHealing,
        ChestUtility,
        CloakedChest,
        AdaptiveChest,
        SecurityChest,
        ScavengerPack,
        ShippingRequests,
        EquipmentBarrel,
        RustyLockbox,
        EncrustedCache,

        // Shrines
        ShrineChance,
        ShrineMountain,
        ShrineBlood,
        ShrineOrder,
        ShrineCombat,
        ShrineCombatCollective,
        ShrineWoods,
        ShrineHalcyon,
        ShrineShaping,
        ShrineRebirth,
        ShrineCompulsion,
        ShrineHallowed,
        ShrineDescent,
        AltarOfGold,
        NewtAltar,
        CleansingPool,

        // Machines
        Scrapper,
        Printers,
        Cauldrons,

        // Void & lunar
        VoidChests,
        LunarPod,
        LunarSeer,

        // Seekers of the Storm / Alloyed Collective
        Geodes,
        AccessNodes,
        TempItemDistributors,
        WanderingChef,

        // Hallowed Concepts
        ConceptReliquaries,
        GhostlyChests,
        Gongs,
        AnimaPortals,

        // Anything not matched above is grouped by its display name so nothing is ever invisible.
        Uncategorized,
    }

    public enum DroneCat
    {
        GunnerTurret,
        TripleShop,
        DroneScrapper,
        DroneCombiner,
        BrokenDrones,
    }

    public sealed class CategoryDef
    {
        public Cat Id;
        public string Key;
        public string Label;
        public string Token;   // optional language token; label is localized when present
        public string Section; // blank line is inserted when the section changes
        public ColorCatalog.ColorIndex? ColorIdx;
        public string Hex;     // literal color when no ColorIndex fits
    }

    public sealed class DroneCategoryDef
    {
        public DroneCat Id;
        public string Key;
        public string Label;
    }

    public static class Categories
    {
        // Order here is the order lines appear in the loot panel.
        public static readonly CategoryDef[] Loot =
        {
            new CategoryDef { Id = Cat.Multishop, Key = "MultishopTerminals", Label = "Multishop Terminal", Token = "MULTISHOP_TERMINAL_NAME", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Utility },
            new CategoryDef { Id = Cat.EquipmentMultishop, Key = "EquipmentMultishops", Label = "Equipment Multishop", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Equipment },
            new CategoryDef { Id = Cat.CrashedMultishop, Key = "CrashedMultishops", Label = "Crashed Multishop", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Money },
            new CategoryDef { Id = Cat.ChestSmall, Key = "ChestSmall", Label = "Chest", Section = "Chests & Terminals", Hex = Palette.ChestGold },
            new CategoryDef { Id = Cat.ChestLarge, Key = "ChestLarge", Label = "Large Chest", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Tier2Item },
            new CategoryDef { Id = Cat.ChestLegendary, Key = "ChestLegendary", Label = "Legendary Chest", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.BossItem },
            new CategoryDef { Id = Cat.ChestDamage, Key = "ChestDamage", Label = "Damage Chest", Section = "Chests & Terminals", Hex = Palette.ChestGold },
            new CategoryDef { Id = Cat.ChestHealing, Key = "ChestHealing", Label = "Healing Chest", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Healing },
            new CategoryDef { Id = Cat.ChestUtility, Key = "ChestUtility", Label = "Utility Chest", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Utility },
            new CategoryDef { Id = Cat.CloakedChest, Key = "CloakedChests", Label = "Cloaked Chest", Token = "CHEST1_STEALTHED_NAME", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Interactable },
            new CategoryDef { Id = Cat.AdaptiveChest, Key = "AdaptiveChests", Label = "Adaptive Chest", Token = "CASINOCHEST_NAME", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.VoidItem },
            new CategoryDef { Id = Cat.SecurityChest, Key = "SecurityChests", Label = "Timed Security Chest", Section = "Chests & Terminals", Hex = Palette.ChestGold },
            new CategoryDef { Id = Cat.ScavengerPack, Key = "ScavengerPacks", Label = "Scavenger Backpack", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.BossItem },
            new CategoryDef { Id = Cat.ShippingRequests, Key = "ShippingRequests", Label = "Shipping Request", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Interactable },
            new CategoryDef { Id = Cat.EquipmentBarrel, Key = "EquipmentBarrels", Label = "Equipment Barrel", Token = "EQUIPMENTBARREL_NAME", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.Equipment },
            new CategoryDef { Id = Cat.RustyLockbox, Key = "RustyLockboxes", Label = "Rusty Lockbox", Token = "LOCKBOX_NAME", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.BossItem },
            new CategoryDef { Id = Cat.EncrustedCache, Key = "EncrustedCaches", Label = "Encrusted Cache", Token = "VOIDLOCKBOX_NAME", Section = "Chests & Terminals", ColorIdx = ColorCatalog.ColorIndex.VoidItem },

            new CategoryDef { Id = Cat.ShrineChance, Key = "ShrineOfChance", Label = "Shrine of Chance", Token = "SHRINE_CHANCE_NAME", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineMountain, Key = "ShrineOfTheMountain", Label = "Shrine of the Mountain", Token = "SHRINE_BOSS_NAME", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineBlood, Key = "ShrineOfBlood", Label = "Shrine of Blood", Token = "SHRINE_BLOOD_NAME", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineOrder, Key = "ShrineOfOrder", Label = "Shrine of Order", Token = "SHRINE_RESTACK_OPTION_NAME", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineCombat, Key = "ShrineOfCombat", Label = "Shrine of Combat", Token = "SHRINE_COMBAT_NAME", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineCombatCollective, Key = "CollectiveShrineOfCombat", Label = "Collective Shrine of Combat", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineWoods, Key = "ShrineOfTheWoods", Label = "Shrine of the Woods", Token = "SHRINE_WOODS_NAME", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineHalcyon, Key = "HalcyonShrines", Label = "Halcyon Shrine", Section = "Shrines", Hex = Palette.GoldPortal },
            new CategoryDef { Id = Cat.ShrineShaping, Key = "ShrineOfShaping", Label = "Shrine of Shaping", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineRebirth, Key = "ShrineOfRebirth", Label = "Shrine of Rebirth", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineCompulsion, Key = "ShrineOfCompulsion", Label = "Shrine of Compulsion", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.ShrineHallowed, Key = "HallowedShrines", Label = "Hallowed Shrine", Section = "Shrines", ColorIdx = ColorCatalog.ColorIndex.HexedItem },
            new CategoryDef { Id = Cat.ShrineDescent, Key = "ShrineOfDescent", Label = "Shrine of Descent", Section = "Shrines", Hex = Palette.ShrineYellow },
            new CategoryDef { Id = Cat.AltarOfGold, Key = "AltarsOfGold", Label = "Altar of Gold", Section = "Shrines", Hex = Palette.GoldPortal },
            new CategoryDef { Id = Cat.NewtAltar, Key = "NewtAltars", Label = "Newt Altar", Section = "Shrines", ColorIdx = ColorCatalog.ColorIndex.LunarItem },
            new CategoryDef { Id = Cat.CleansingPool, Key = "CleansingPools", Label = "Cleansing Pool", Token = "SHRINE_CLEANSE_NAME", Section = "Shrines", ColorIdx = ColorCatalog.ColorIndex.LunarItem },

            new CategoryDef { Id = Cat.Scrapper, Key = "Scrappers", Label = "Scrapper", Section = "Machines", Hex = Palette.Gray },
            new CategoryDef { Id = Cat.Printers, Key = "Printers", Label = "3D Printer", Section = "Machines", Hex = Palette.Gray },
            new CategoryDef { Id = Cat.Cauldrons, Key = "Cauldrons", Label = "Cauldron", Section = "Machines", Hex = Palette.Gray },

            new CategoryDef { Id = Cat.VoidChests, Key = "VoidChests", Label = "Void Chest", Token = "VOID_CHEST_NAME", Section = "Void & Lunar", ColorIdx = ColorCatalog.ColorIndex.VoidItem },
            new CategoryDef { Id = Cat.LunarPod, Key = "LunarPods", Label = "Lunar Pod", Token = "LUNAR_CHEST_NAME", Section = "Void & Lunar", ColorIdx = ColorCatalog.ColorIndex.LunarItem },
            new CategoryDef { Id = Cat.LunarSeer, Key = "LunarSeers", Label = "Lunar Seer", Section = "Void & Lunar", ColorIdx = ColorCatalog.ColorIndex.LunarItem },

            new CategoryDef { Id = Cat.Geodes, Key = "AurelioniteGeodes", Label = "Aurelionite Geode", Section = "Storm & Collective", Hex = Palette.GoldPortal },
            new CategoryDef { Id = Cat.AccessNodes, Key = "AccessNodes", Label = "Access Node", Section = "Storm & Collective", Hex = Palette.AlloyedTeal },
            new CategoryDef { Id = Cat.TempItemDistributors, Key = "TempItemDistributors", Label = "Temporary Item Distributor", Section = "Storm & Collective", Hex = Palette.AlloyedTeal },
            new CategoryDef { Id = Cat.WanderingChef, Key = "WanderingChef", Label = "Wandering CHEF", Section = "Storm & Collective", Hex = Palette.AlloyedTeal },

            new CategoryDef { Id = Cat.ConceptReliquaries, Key = "ConceptReliquaries", Label = "Concept Reliquary", Section = "Hallowed", ColorIdx = ColorCatalog.ColorIndex.HexedItem },
            new CategoryDef { Id = Cat.GhostlyChests, Key = "GhostlyChests", Label = "Ghostly Chest", Section = "Hallowed", ColorIdx = ColorCatalog.ColorIndex.HexedItem },
            new CategoryDef { Id = Cat.Gongs, Key = "Gongs", Label = "Gong", Section = "Hallowed", ColorIdx = ColorCatalog.ColorIndex.Trial },
            new CategoryDef { Id = Cat.AnimaPortals, Key = "AnimaPortals", Label = "Anima Portal", Section = "Hallowed", ColorIdx = ColorCatalog.ColorIndex.Artifact },

            new CategoryDef { Id = Cat.Uncategorized, Key = "Uncategorized", Label = "Other", Section = "Other", Hex = Palette.Gray },
        };

        public static readonly DroneCategoryDef[] DroneTypes =
        {
            new DroneCategoryDef { Id = DroneCat.GunnerTurret, Key = "GunnerTurret", Label = "Gunner Turret" },
            new DroneCategoryDef { Id = DroneCat.TripleShop, Key = "TripleDroneShop", Label = "Triple Drone Shop" },
            new DroneCategoryDef { Id = DroneCat.DroneScrapper, Key = "DroneScrapper", Label = "Drone Scrapper" },
            new DroneCategoryDef { Id = DroneCat.DroneCombiner, Key = "DroneCombiner", Label = "Drone Combiner Station" },
            new DroneCategoryDef { Id = DroneCat.BrokenDrones, Key = "BrokenDronesByType", Label = "Broken Drones" },
        };

        public static CategoryDef Get(Cat id)
        {
            for (int i = 0; i < Loot.Length; i++)
            {
                if (Loot[i].Id == id)
                {
                    return Loot[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Fallback matching by resolved English display name, for interactables whose tokens are
        /// unknown and that have no dedicated component (verified against the English client).
        /// </summary>
        public static readonly Dictionary<string, Cat> DisplayNameMap = new Dictionary<string, Cat>
        {
            ["Shipping Request Form"] = Cat.ShippingRequests,
            ["Aurelionite Fragment"] = Cat.Geodes,
            ["Aurelionite Geode"] = Cat.Geodes,
        };
    }
}
