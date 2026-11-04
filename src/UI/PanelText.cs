using System.Text;
using RoR2;
using StageTracker.Config;
using StageTracker.Tracking;
using StageTracker.Util;

namespace StageTracker.UI
{
    /// <summary>Builds the rich-text body of the loot and drone panels.</summary>
    public static class PanelText
    {
        private const string Stack = "<style=cStack>";
        private const string Sub = "<style=cSub>";
        private const string Close = "</style>";
        private const string Separator = Stack + " · " + Close;

        public static string BuildLoot(LootData data, ModConfig config)
        {
            var sb = new StringBuilder(1024);
            string lastSection = null;

            for (int i = 0; i < Categories.Loot.Length; i++)
            {
                var def = Categories.Loot[i];

                if (def.Id == Cat.Uncategorized)
                {
                    if (config.TrackUncategorized.Value)
                    {
                        for (int u = 0; u < data.Unknown.Count; u++)
                        {
                            var unknown = data.Unknown[u];
                            if (unknown.total <= 0)
                            {
                                continue;
                            }

                            sb.Append('\n');
                            sb.Append(FormatLine(unknown.label, Palette.Gray, unknown.available, unknown.total, config.ShowUsedOverTotal.Value)).Append('\n');
                        }
                    }
                    continue;
                }

                if (!config.Enabled(def.Id) || !data.Counts.TryGetValue(def.Id, out var count) || count.total <= 0)
                {
                    continue;
                }

                if (def.Section != lastSection)
                {
                    if (lastSection != null)
                    {
                        sb.Append('\n');
                    }
                    lastSection = def.Section;
                }

                string label = Lang.TryGet(def.Token) ?? def.Label;
                sb.Append(FormatLine(label, def.ColorIdx != null ? Palette.Hex(def.ColorIdx.Value) : def.Hex, count.available, count.total, config.ShowUsedOverTotal.Value)).Append('\n');

                if (def.Id == Cat.ShrineChance && data.ChancePurchasesLeft > 0)
                {
                    sb.Length -= 1; // replace the newline with the purchase hint on the same line
                    sb.Append(' ').Append(Palette.Colored($"({data.ChancePurchasesLeft})", Palette.ShrineYellow)).Append('\n');
                }
            }

            // Machines with per-tier buckets.
            AppendTierLine(sb, data.PrinterAvailable, data.PrinterTotal, Lang.TryGet("DUPLICATOR_NAME") ?? "3D Printer", config.Enabled(Cat.Printers), config.ShowUsedOverTotal.Value);
            AppendTierLine(sb, data.CauldronAvailable, data.CauldronTotal, "Cauldron", config.Enabled(Cat.Cauldrons), config.ShowUsedOverTotal.Value);

            if (config.ShowEnemies.Value && (data.MonsterEnemies > 0 || data.VoidEnemies > 0))
            {
                sb.Append('\n');
                sb.Append(Stack + "> " + Close).Append(Palette.Colored("Enemies", Palette.EnemyRed)).Append(Stack + ": " + Close);
                bool any = false;
                if (data.MonsterEnemies > 0)
                {
                    sb.Append(Palette.Colored(data.MonsterEnemies.ToString(), Palette.EnemyRed));
                    any = true;
                }
                if (data.VoidEnemies > 0)
                {
                    if (any)
                    {
                        sb.Append(Separator);
                    }
                    sb.Append(Palette.Colored(data.VoidEnemies.ToString(), Palette.Hex(ColorCatalog.ColorIndex.VoidItem)));
                }
                sb.Append('\n');
            }

            if (config.ShowInvitations.Value && data.Invitations > 0)
            {
                sb.Append(Stack + "> " + Close)
                    .Append(Palette.Colored("Invitations", Palette.ShrineYellow))
                    .Append(Stack + ": " + Close)
                    .Append(Palette.Colored(data.Invitations.ToString(), Palette.ShrineYellow))
                    .Append('\n');
            }

            if (config.ShowStageName.Value && !string.IsNullOrEmpty(data.StageName))
            {
                sb.Append('\n');
                sb.Append(Stack + "-- ").Append(data.StageName).Append(" --").Append(Close);
            }

            return sb.ToString();
        }

        public static string BuildDrones(DroneData data, ModConfig config)
        {
            var sb = new StringBuilder(512);

            if (config.Enabled(DroneCat.GunnerTurret) && data.TurretTotal > 0)
            {
                sb.Append(FormatLine("Gunner Turret", Palette.Hex(ColorCatalog.ColorIndex.Tier1Item), data.TurretAvailable, data.TurretTotal, config.ShowUsedOverTotal.Value)).Append('\n');
            }

            if (config.Enabled(DroneCat.BrokenDrones))
            {
                for (int i = 0; i < data.Broken.Count; i++)
                {
                    var group = data.Broken[i];
                    if (group.Total <= 0)
                    {
                        continue;
                    }

                    sb.Append(FormatLine(group.Label, group.ColorHex, group.Available, group.Total, config.ShowUsedOverTotal.Value)).Append('\n');
                }
            }

            if (config.Enabled(DroneCat.TripleShop) && data.TripleShopTotal > 0)
            {
                sb.Append('\n');
                sb.Append(FormatLine("Triple Drone Shop", Palette.Hex(ColorCatalog.ColorIndex.Utility), data.TripleShopAvailable, data.TripleShopTotal, config.ShowUsedOverTotal.Value)).Append('\n');
            }

            if (config.Enabled(DroneCat.DroneScrapper) || config.Enabled(DroneCat.DroneCombiner))
            {
                sb.Append('\n');
                if (config.Enabled(DroneCat.DroneScrapper))
                {
                    AppendPresence(sb, "Drone Scrapper", data.DroneScrapperPresent);
                }
                if (config.Enabled(DroneCat.DroneCombiner))
                {
                    AppendPresence(sb, "Drone Combiner Station", data.DroneCombinerPresent);
                }
            }

            return sb.ToString();
        }

        private static void AppendTierLine(StringBuilder sb, int[] available, int[] total, string label, bool enabled, bool alwaysUsedOverTotal)
        {
            if (!enabled)
            {
                return;
            }

            bool any = false;
            for (int i = 0; i < total.Length; i++)
            {
                if (total[i] > 0)
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                return;
            }

            sb.Append('\n');
            sb.Append(Stack + "> " + Close).Append(Palette.Colored(label, Palette.Gray)).Append(Stack + ": " + Close);

            bool first = true;
            for (int i = 0; i < total.Length; i++)
            {
                if (total[i] <= 0)
                {
                    continue;
                }

                if (!first)
                {
                    sb.Append(Separator);
                }
                first = false;

                string hex = Palette.BucketHex(i);
                sb.Append(available[i] == total[i] && !alwaysUsedOverTotal
                    ? Palette.Colored(total[i].ToString(), hex)
                    : Palette.Colored($"{available[i]}/{total[i]}", hex));
            }

            sb.Append('\n');
        }

        private static void AppendPresence(StringBuilder sb, string label, bool present)
        {
            sb.Append(Stack + "> " + Close)
                .Append(Palette.Colored(label, Palette.Gray))
                .Append(Stack + ": " + Close)
                .Append(present
                    ? Palette.Colored("@", Palette.Hex(ColorCatalog.ColorIndex.Healing))
                    : Sub + "×" + Close)
                .Append('\n');
        }

        private static string FormatLine(string label, string colorHex, int available, int total, bool alwaysUsedOverTotal)
        {
            var sb = new StringBuilder(64);
            sb.Append(Stack + "> " + Close)
                .Append(Palette.Colored(label, colorHex))
                .Append(Stack + ": " + Close);

            if (available == total && !alwaysUsedOverTotal)
            {
                sb.Append(total);
            }
            else if (available == 0)
            {
                sb.Append(Sub + "0" + Close).Append(Stack + "/" + total + Close);
            }
            else
            {
                sb.Append(available).Append(Stack + "/" + total + Close);
            }

            return sb.ToString();
        }
    }
}
