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

            // Remove the original objective entries (including the template label) afterwards, keeping
            // only the two fresh copies.
            foreach (Transform child in container)
            {
                if (child != header.transform && child != body.transform)
                {
                    Object.DestroyImmediate(child.gameObject);
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

            return new HudPanel { Root = clone, Label = header, Body = body };
        }
    }
}
