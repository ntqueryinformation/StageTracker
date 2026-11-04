using BepInEx;
using RoR2;
using RoR2.UI;
using StageTracker.Config;
using StageTracker.UI;
using UnityEngine;

namespace StageTracker
{
    /// <summary>
    /// Entry point. Hooks HUD creation and attaches the panel controller to every run HUD.
    /// No Harmony/MMHOOK patches anywhere in this mod — everything is polling + vanilla events,
    /// so game updates (new DLCs) can't break the hooks.
    /// </summary>
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Author = "Kyle";
        public const string Name = "StageTracker";
        public const string GUID = Author + "." + Name;
        public const string Version = "1.0.0";

        internal static BepInEx.Logging.ManualLogSource Log;

        internal static new ModConfig Config { get; private set; }

        private void Awake()
        {
            Log = Logger;
            Config = new ModConfig(base.Config);

            HUD.shouldHudDisplay += OnShouldHudDisplay;
            Log.LogInfo($"{Name} v{Version} loaded.");
        }

        // Settings registration is deferred to Start(): after every plugin's Awake, so Risk of
        // Options' own initialization has settled before we hand it our config entries.
        private void Start()
        {
            Config.RegisterRiskOfOptions(Name, "Loot and drone panels that track everything left on the stage — all shown at once, no teleporter gating.", GUID);
        }

        private void OnDestroy()
        {
            HUD.shouldHudDisplay -= OnShouldHudDisplay;
        }

        private static void OnShouldHudDisplay(HUD hud, ref bool shouldDisplay)
        {
            if (!hud || hud.gameObject.GetComponent<PanelsController>())
            {
                return;
            }

            hud.gameObject.AddComponent<PanelsController>();
        }
    }
}
