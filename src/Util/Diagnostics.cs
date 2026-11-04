using System.Text;
using RoR2;
using UnityEngine;

namespace StageTracker.Util
{
    /// <summary>
    /// Debug helper used while testing against new DLC content: dumps every interactable on the
    /// current stage (token, resolved display name, cost, components) plus every drone body, so
    /// unknown content can be added to the category table with exact identifiers.
    /// </summary>
    public static class Diagnostics
    {
        [ConCommand(commandName = "stage_tracker_scan", flags = ConVarFlags.None, helpText = "StageTracker: dump all trackable interactables and drone bodies on this stage to the console/log.")]
        public static void ScanStage(ConCommandArgs args)
        {
            var sb = new StringBuilder(4096);
            sb.AppendLine("=== StageTracker stage scan ===");

            var purchases = InstanceTracker.GetInstancesList<PurchaseInteraction>();
            sb.AppendLine($"PurchaseInteractions: {purchases.Count}");
            for (int i = 0; i < purchases.Count; i++)
            {
                var purchase = purchases[i];
                if (!purchase)
                {
                    continue;
                }

                var components = purchase.GetComponents<MonoBehaviour>();
                var names = new StringBuilder();
                for (int c = 0; c < components.Length; c++)
                {
                    var component = components[c];
                    if (!component)
                    {
                        continue;
                    }

                    var type = component.GetType();
                    if (type.Namespace == null || (!type.Namespace.StartsWith("RoR2") && !type.Namespace.StartsWith("EntityStates")))
                    {
                        continue;
                    }

                    if (names.Length > 0)
                    {
                        names.Append(", ");
                    }
                    names.Append(type.FullName);
                }

                sb.AppendLine($"  [{purchase.gameObject.name}] token='{purchase.displayNameToken}' name='{purchase.GetDisplayName()}' cost={purchase.cost}({purchase.costType}) available={purchase.available} isShrine={purchase.isShrine} isDrone={purchase.isDrone} goldShrine={purchase.isGoldShrine} components=[{names}]");

                var chest = purchase.GetComponentInChildren<ChestBehavior>(true);
                if (chest)
                {
                    sb.AppendLine($"      chest: dropTable={(chest.dropTable ? chest.dropTable.GetType().Name + "/" + chest.dropTable.name : "null")} tag={chest.requiredItemTag} tiers={chest.tier1Chance:P0}/{chest.tier2Chance:P0}/{chest.tier3Chance:P0} lunar={chest.lunarChance:P0} drops={chest.minDropCount}-{chest.maxDropCount}");
                }
            }

            var bodies = CharacterBody.readOnlyInstancesList;
            sb.AppendLine($"CharacterBodies with purchase interactions: (scanning {bodies.Count})");
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
                    continue;
                }

                DroneIndex droneIndex = DroneCatalog.GetDroneIndexFromBodyIndex(body.bodyIndex);
                sb.AppendLine($"  [{body.gameObject.name}] body='{body.GetDisplayName()}' droneIndex={droneIndex} token='{purchase.displayNameToken}' available={purchase.available}");
            }

            sb.AppendLine("=== end scan ===");
            Debug.Log(sb.ToString());
        }
    }
}
