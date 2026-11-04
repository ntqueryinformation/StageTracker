using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StageTracker.UI
{
    /// <summary>
    /// A panel cloned from the vanilla objective panel, so it inherits the game's skin, scaling and
    /// position in the upper-right HUD cluster. Contains a header label and one rich-text body.
    /// </summary>
    public sealed class HudPanel
    {
        public GameObject Root;
        public HGTextMeshProUGUI Label;
        public HGTextMeshProUGUI Body;

        public static HudPanel Clone(ObjectivePanelController template, string panelName, string headerText)
        {
            var clone = Object.Instantiate(template.gameObject, template.transform.parent);
            clone.name = panelName;

            Object.DestroyImmediate(clone.GetComponent<ObjectivePanelController>());
            var targetSetter = clone.GetComponent<RoR2.UI.HudObjectiveTargetSetter>();
            if (targetSetter)
            {
                Object.DestroyImmediate(targetSetter);
            }

            var templateLabel = clone.GetComponentInChildren<HGTextMeshProUGUI>(true);
            if (!templateLabel)
            {
                Object.DestroyImmediate(clone);
                return null;
            }

            var container = templateLabel.transform.parent;
            if (!container)
            {
                Object.DestroyImmediate(clone);
                return null;
            }

            var header = Object.Instantiate(templateLabel.gameObject, container).GetComponent<HGTextMeshProUGUI>();
            var body = Object.Instantiate(templateLabel.gameObject, container).GetComponent<HGTextMeshProUGUI>();

            // Remove the original objective entries (including the template label), keeping only the
            // two fresh copies. Destroying while iterating a Transform skips children, so keep
            // sweeping until nothing but ours is left — leftovers here are the blank space bug.
            bool removedExtra = true;
            while (removedExtra)
            {
                removedExtra = false;
                Transform toRemove = null;
                foreach (Transform child in container)
                {
                    if (child != header.transform && child != body.transform)
                    {
                        toRemove = child;
                        removedExtra = true;
                        break;
                    }
                }

                if (toRemove)
                {
                    Object.DestroyImmediate(toRemove.gameObject);
                }
            }

            foreach (var text in new[] { header, body })
            {
                var languageController = text.GetComponent<LanguageTextMeshController>();
                if (languageController)
                {
                    Object.DestroyImmediate(languageController); // would overwrite our custom text
                }

                var layoutElement = text.GetComponent<LayoutElement>();
                if (layoutElement)
                {
                    Object.DestroyImmediate(layoutElement); // let the VerticalLayoutGroup size it
                }

                text.fontSize = 12f;
                text.fontSizeMax = 12f;
                text.fontSizeMin = 6f;
                text.alignment = TextAlignmentOptions.TopLeft;
            }

            header.text = headerText;
            body.text = string.Empty;

            LogHierarchy(clone.transform, 0);

            return new HudPanel { Root = clone, Label = header, Body = body };
        }

        private static void LogHierarchy(Transform node, int depth)
        {
            var rect = node as RectTransform;
            string size = rect != null ? $" size={rect.rect.width:F0}x{rect.rect.height:F0}" : string.Empty;
            Plugin.Log.LogInfo($"{new string(' ', depth * 2)}{node.name}{size}");

            foreach (Transform child in node)
            {
                LogHierarchy(child, depth + 1);
            }
        }
    }
}
