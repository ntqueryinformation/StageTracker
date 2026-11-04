using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using StageTracker.Tracking;
using StageTracker.Util;

namespace StageTracker.Config
{
    public enum DisplayMode
    {
        HoldTab,
        AlwaysVisible,
    }

    /// <summary>
    /// All mod settings. Every entry is a normal BepInEx config entry; the whole set is registered
    /// with Risk of Options so it shows up in the game's Options menu under "Mods".
    /// </summary>
    public sealed class ModConfig
    {
        public readonly ConfigEntry<DisplayMode> PanelMode;
        public readonly ConfigEntry<bool> ShowLootPanel;
        public readonly ConfigEntry<bool> ShowDronePanel;
        public readonly ConfigEntry<bool> ShowEnemies;
        public readonly ConfigEntry<bool> ShowInvitations;
        public readonly ConfigEntry<bool> ShowStageName;
        public readonly ConfigEntry<bool> TrackUncategorized;

        public readonly IReadOnlyDictionary<Cat, ConfigEntry<bool>> LootToggles;
        public readonly IReadOnlyDictionary<DroneCat, ConfigEntry<bool>> DroneToggles;

        public bool Enabled(Cat id)
        {
            return LootToggles.TryGetValue(id, out var entry) && entry.Value;
        }

        public bool Enabled(DroneCat id)
        {
            return DroneToggles.TryGetValue(id, out var entry) && entry.Value;
        }

        public ModConfig(ConfigFile file)
        {
            PanelMode = file.Bind(
                "Display",
                "DisplayMode",
                DisplayMode.HoldTab,
                "HoldTab: panels only appear while the scoreboard (Tab) is held. AlwaysVisible: panels stay docked on the right side of the screen.");

            ShowLootPanel = file.Bind("Display", "ShowLootPanel", true, "Show the loot panel.");
            ShowDronePanel = file.Bind("Display", "ShowDronePanel", true, "Show the drone panel.");

            ShowEnemies = file.Bind("Footer", "ShowEnemyCounts", true, "Show remaining enemy counts by team.");
            ShowInvitations = file.Bind("Footer", "ShowInvitations", true, "Show how many Shrine of the Mountain invitations are still pending. Accurate on the host; approximate for clients.");
            ShowStageName = file.Bind("Footer", "ShowStageName", true, "Show the stage name at the bottom of the loot panel.");

            TrackUncategorized = file.Bind(
                "Loot",
                "TrackUncategorized",
                true,
                "Also list interactables this mod doesn't know yet (e.g. content from a future update), grouped by their in-game name.");

            var loot = new Dictionary<Cat, ConfigEntry<bool>>();
            foreach (var def in Categories.Loot)
            {
                if (def.Id == Cat.Uncategorized)
                {
                    loot[def.Id] = TrackUncategorized;
                    continue;
                }

                loot[def.Id] = file.Bind("Loot", def.Key, true, $"Track {def.Label}.");
            }
            LootToggles = loot;

            var drones = new Dictionary<DroneCat, ConfigEntry<bool>>();
            foreach (var def in Categories.DroneTypes)
            {
                drones[def.Id] = file.Bind("Drones", def.Key, true, $"Track {def.Label} in the drone panel.");
            }
            DroneToggles = drones;
        }

        /// <summary>
        /// Soft dependency: if Risk of Options isn't installed the mod still works and all settings
        /// remain available through the BepInEx config file. Registration happens in Start() — after
        /// every plugin's Awake — because registering before RiskOfOptions initializes gets the
        /// options lost.
        /// </summary>
        public void RegisterRiskOfOptions(string modName, string description, string modGuid)
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                Plugin.Log.LogInfo("Risk of Options not installed; settings stay in the BepInEx config file.");
                return;
            }

            try
            {
                RegisterInternal(modName, description, modGuid);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Risk of Options registration failed ({e.GetType().Name}: {e.Message}); settings stay in the BepInEx config file.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RegisterInternal(string modName, string description, string modGuid)
        {
            ModSettingsManager.SetModDescription(description, modGuid, modName);

            ModSettingsManager.AddOption(new ChoiceOption(PanelMode), modGuid, modName);
            ModSettingsManager.AddOption(new CheckBoxOption(ShowLootPanel), modGuid, modName);
            ModSettingsManager.AddOption(new CheckBoxOption(ShowDronePanel), modGuid, modName);
            ModSettingsManager.AddOption(new CheckBoxOption(ShowEnemies), modGuid, modName);
            ModSettingsManager.AddOption(new CheckBoxOption(ShowInvitations), modGuid, modName);
            ModSettingsManager.AddOption(new CheckBoxOption(ShowStageName), modGuid, modName);
            ModSettingsManager.AddOption(new CheckBoxOption(TrackUncategorized), modGuid, modName);

            foreach (var def in Categories.Loot)
            {
                if (def.Id == Cat.Uncategorized)
                {
                    continue;
                }

                ModSettingsManager.AddOption(new CheckBoxOption(LootToggles[def.Id]), modGuid, modName);
            }

            foreach (var def in Categories.DroneTypes)
            {
                ModSettingsManager.AddOption(new CheckBoxOption(DroneToggles[def.Id]), modGuid, modName);
            }

            // Log-only sanity check against RiskOfOptions' internal collection.
            try
            {
                var collectionField = typeof(ModSettingsManager).GetField("OptionCollection", BindingFlags.NonPublic | BindingFlags.Static);
                var collection = collectionField?.GetValue(null);
                var indexer = collection?.GetType().GetProperty("Item", BindingFlags.NonPublic | BindingFlags.Instance);
                var mine = indexer?.GetValue(collection, new object[] { modGuid });
                var categoryCount = mine?.GetType().GetProperty("CategoryCount", BindingFlags.NonPublic | BindingFlags.Instance);
                Plugin.Log.LogInfo($"RiskOfOptions self-check: categories for {modGuid} = {categoryCount?.GetValue(mine)}");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"RiskOfOptions self-check failed: {e.Message}");
            }
        }
    }
}
