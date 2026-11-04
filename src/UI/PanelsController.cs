using RoR2;
using RoR2.UI;
using StageTracker.Config;
using StageTracker.Tracking;
using UnityEngine;

namespace StageTracker.UI
{
    /// <summary>
    /// Lives on the HUD, clones the two panels once the vanilla objective panel exists, and drives
    /// visibility (HoldTab mirrors the scoreboard; AlwaysVisible keeps them docked on the right)
    /// plus the throttled stage rescans.
    /// </summary>
    public sealed class PanelsController : MonoBehaviour
    {
        private const float RescanInterval = 1f / 30f;
        private const int CreateAttempts = 240; // ~4s at 60fps, retried every 30 frames

        private HUD _hud;
        private HudPanel _loot;
        private HudPanel _drone;

        private LootData _lootData;
        private DroneData _droneData;
        private float _lastScan;
        private int _framesSinceStart;

        private void Awake()
        {
            _hud = GetComponent<HUD>();
        }

        private void Update()
        {
            if (_loot == null || _drone == null)
            {
                _framesSinceStart++;
                if (_framesSinceStart % 30 == 0 && _framesSinceStart <= CreateAttempts * 30)
                {
                    TryCreatePanels();
                }

                if (_loot == null || _drone == null)
                {
                    return;
                }
            }

            var config = Plugin.Config;
            if (config == null)
            {
                return;
            }

            bool visible = config.PanelMode.Value == DisplayMode.AlwaysVisible
                || (_hud && _hud.scoreboardPanel && _hud.scoreboardPanel.activeSelf);

            bool lootOn = visible && config.ShowLootPanel.Value;
            bool droneOn = visible && config.ShowDronePanel.Value;

            _loot.Root.SetActive(lootOn);
            _drone.Root.SetActive(droneOn);

            if (!visible || Time.unscaledTime - _lastScan < RescanInterval)
            {
                return;
            }

            _lastScan = Time.unscaledTime;

            _lootData = LootScan.Scan();
            _droneData = DroneScan.Scan();

            _loot.Body.text = PanelText.BuildLoot(_lootData, config);
            _drone.Body.text = PanelText.BuildDrones(_droneData, config);
        }

        private void TryCreatePanels()
        {
            var template = _hud ? _hud.GetComponentInChildren<ObjectivePanelController>(true) : null;
            if (!template)
            {
                return; // main menu HUDs never have one; keep retrying while this HUD lives
            }

            _loot = HudPanel.Clone(template, "StageTrackerLootPanel", "Loot:");
            _drone = HudPanel.Clone(template, "StageTrackerDronePanel", "Drones:");

            if (_loot == null || _drone == null)
            {
                enabled = false;
                return;
            }

            _loot.Root.SetActive(false);
            _drone.Root.SetActive(false);
        }
    }
}
