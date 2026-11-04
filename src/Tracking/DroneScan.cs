using System.Collections.Generic;
using System.Text;
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
        public static DroneData Scan()
        {
            var data = new DroneData();
            var brokenMap = new SortedDictionary<int, (string label, string colorHex, int available, int total)>();
            var fallbackMap = new SortedDictionary<string, (int available, int total)>();

            // Drone-flavored interactables.
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

                if (go.GetComponent<DroneVendorTerminalBehavior>() || go.GetComponent<DroneVendorMultiShopController>() || token == "DRONE_VENDOR_TERMINAL_NAME")
                {
                    data.TripleShopTotal++;
                    if (purchase.available)
                    {
                        data.TripleShopAvailable++;
                    }
                    continue;
                }

                if (go.GetComponent<DroneScrapperController>() || token == "DRONE_SCRAPPER_NAME")
                {
                    data.DroneScrapperPresent = true;
                    continue;
                }

                if (go.GetComponent<DroneCombinerController>())
                {
                    data.DroneCombinerPresent = true;
                }
            }

            if (InstanceTracker.Any<DroneCombinerController>())
            {
                data.DroneCombinerPresent = true;
            }

            // Broken (repairable) drones live in the scene as dormant character bodies with a
            // PurchaseInteraction on them; DroneCatalog maps the body to its type and tier — this
            // also catches combiner-upgraded drones, which token matching could never do.
            var bodies = CharacterBody.readOnlyInstancesList;
            for (int i = 0; i < bodies.Count; i++)
            {
                var body = bodies[i];
                if (!body)
                {
                    continue;
                }

                var purchase = body.GetComponentInChildren<PurchaseInteraction>();
                if (!purchase)
                {
                    continue; // deployed drones (owned by masters) have no purchase interaction
                }

                string token = purchase.displayNameToken;
                if (token != null && token.StartsWith("TURRET1"))
                {
                    continue; // already counted above
                }

                DroneIndex droneIndex = DroneCatalog.GetDroneIndexFromBodyIndex(body.bodyIndex);
                if (droneIndex != DroneIndex.None)
                {
                    brokenMap.TryGetValue((int)droneIndex, out var bucket);
                    bucket.total++;
                    if (purchase.available)
                    {
                        bucket.available++;
                    }

                    if (bucket.label == null)
                    {
                        var def = DroneCatalog.GetDroneDef(droneIndex);
                        if (def)
                        {
                            bucket.label = Lang.TryGet(def.nameToken) ?? def.name;
                            bucket.colorHex = def.colorOverride != ColorCatalog.ColorIndex.None
                                ? Palette.Hex(def.colorOverride)
                                : Palette.Hex(def.tier);
                        }
                    }

                    brokenMap[(int)droneIndex] = bucket;
                    continue;
                }

                // Not in the catalog: group by body display name so it still shows up. Only accept
                // drone-like bodies, so unrelated purchase interactions can't leak into this panel.
                string name = body.GetDisplayName();
                if (string.IsNullOrEmpty(name))
                {
                    name = body.gameObject.name;
                }

                if (!name.Contains("Drone") && !name.Contains("drone") && !name.Contains("Turret"))
                {
                    continue;
                }

                fallbackMap.TryGetValue(name, out var fallback);
                fallback.total++;
                if (purchase.available)
                {
                    fallback.available++;
                }
                fallbackMap[name] = fallback;
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
