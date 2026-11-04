using RoR2;
using UnityEngine;

namespace StageTracker.Util
{
    /// <summary>
    /// Color helpers. Whenever possible a category resolves through <see cref="ColorCatalog"/> so the
    /// panel matches the game's palette exactly and follows any palette patches automatically.
    /// Literal hexes are only used for colors that have no ColorIndex (chest gold, shrine yellow, ...).
    /// </summary>
    public static class Palette
    {
        public const string ChestGold = "E5C962";   // damage/gold used by the game for chest-style text
        public const string ShrineYellow = "EFEB1A";
        public const string GoldPortal = "FFFCC4";
        public const string AlloyedTeal = "1CB8DB"; // Alloyed Collective DLC accent
        public const string Gray = "8B8B8B";
        public const string EnemyRed = "E58262";

        public static string Hex(ColorCatalog.ColorIndex index)
        {
            return ColorUtility.ToHtmlStringRGB(ColorCatalog.GetColor(index));
        }

        public static string Hex(ItemTier tier)
        {
            ColorCatalog.ColorIndex index;
            switch (tier)
            {
                case ItemTier.Tier1: index = ColorCatalog.ColorIndex.Tier1Item; break;
                case ItemTier.Tier2: index = ColorCatalog.ColorIndex.Tier2Item; break;
                case ItemTier.Tier3: index = ColorCatalog.ColorIndex.Tier3Item; break;
                case ItemTier.Lunar: index = ColorCatalog.ColorIndex.LunarItem; break;
                case ItemTier.Boss: index = ColorCatalog.ColorIndex.BossItem; break;
                case ItemTier.VoidTier1:
                case ItemTier.VoidTier2:
                case ItemTier.VoidTier3:
                case ItemTier.VoidBoss: index = ColorCatalog.ColorIndex.VoidItem; break;
                case ItemTier.FoodTier: index = ColorCatalog.ColorIndex.Tier2Item; break;
                case ItemTier.Cursed: index = ColorCatalog.ColorIndex.CursedItem; break;
                case ItemTier.Hex: index = ColorCatalog.ColorIndex.HexedItem; break;
                default: index = ColorCatalog.ColorIndex.Tier1Item; break;
            }
            return Hex(index);
        }

        public static string Colored(string text, string hex)
        {
            return $"<color=#{hex}>{text}</color>";
        }

        /// <summary>Machines (printers, cauldrons) bucket by purchase cost tier: white/green/red/boss/other.</summary>
        public static int CostBucket(CostTypeIndex costType, string gameObjectName)
        {
            switch (costType)
            {
                case CostTypeIndex.WhiteItem: return 0;
                case CostTypeIndex.GreenItem: return 1;
                case CostTypeIndex.RedItem: return 2;
                case CostTypeIndex.BossItem: return 3;
                default: return gameObjectName != null && gameObjectName.Contains("Void") ? 4 : 0;
            }
        }

        public static string BucketHex(int bucket)
        {
            switch (bucket)
            {
                case 0: return Hex(ColorCatalog.ColorIndex.Tier1Item);
                case 1: return Hex(ColorCatalog.ColorIndex.Tier2Item);
                case 2: return Hex(ColorCatalog.ColorIndex.Tier3Item);
                case 3: return Hex(ColorCatalog.ColorIndex.BossItem);
                default: return Hex(ColorCatalog.ColorIndex.VoidItem);
            }
        }
    }

    /// <summary>Language token resolution that tolerates missing entries.</summary>
    public static class Lang
    {
        /// <summary>Returns the localized string, or null when the token is missing (GetString echoes the token back).</summary>
        public static string TryGet(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            string value = Language.GetString(token);
            if (string.IsNullOrEmpty(value) || value == token)
            {
                return null;
            }

            return value;
        }
    }
}
