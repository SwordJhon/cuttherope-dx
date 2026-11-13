using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>Quad holding Paddington's suitcase on the Christmas support sheet.</summary>
        private const int PaddingtonSupportQuad = 1;

        /// <summary>Number of platforms on the Experiments support sheet, one per Experiments pack.</summary>
        private const int ExperimentsSupportCount = 8;

        /// <summary>Number of platforms on the extra support sheet.</summary>
        private const int ExtrasSupportCount = 3;

        /// <summary>
        /// Loads Om Nom from XML node data
        /// Sets up Om Nom animations, blink animation, and greeting if needed
        /// </summary>
        /// <param name="xmlNode">The XML node describing Om Nom.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadTarget(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int pack = Application.SharedRootController().Pack;

            int targetType = ParseIntOrZero(xmlNode.Attribute("targetType")?.Value ?? string.Empty);

            bool isClassicSkin = OmNomSkinRegistry.IsClassicSkin(
                OmNomSkinRegistry.ResolveTargetSkinIndex(
                    targetType,
                    OmNomSkinRegistry.GetSelectedSkinIndex(),
                    OmNomSkinRegistry.TotalSkinCount));
            string supportThemeOverride = xmlNode.Attribute("supportTheme")?.Value ?? string.Empty;
            int supportQuadOverride = ParseIntOrZero(xmlNode.Attribute("supportQuad")?.Value ?? string.Empty);
            bool isPaddington = SpecialEvents.IsJanuary && isClassicSkin;

            bool isPrimaryTarget = targets.Count == 0;
            bool paddingtonGreetingPending =
                isPaddington && isPrimaryTarget && !nightLevel && RootController.IsShowGreeting();

            (string supportResource, int requestedQuad) = ResolveSupport(xmlNode, pack, isPaddington, MenuTheme.IsExperiments);

            // Clamp quad index to valid range; fall back to first quad for invalid values.
            Texture2D supportTexture = Application.GetTexture(supportResource);
            int quadIndex = (requestedQuad >= 0 && requestedQuad < supportTexture.quadRects.Length) ? requestedQuad : 0;

            support = Image.FromResource(supportResource, quadIndex);
            support.DoRestoreCutTransparency();
            support.anchor = 18;

            ITargetAnimationBackend animation = TargetAnimationBackendFactory.CreateForTarget(
                targetType, nightLevel, SpecialEvents.IsXmas, isPaddington, paddingtonGreetingPending, MenuTheme.IsExperiments);
            GameObject targetObj = animation.TargetObject;
            targetBaseScaleX = animation.GetTargetBaseScaleX();
            targetBaseScaleY = animation.GetTargetBaseScaleY();
            targetObj.scaleX = targetBaseScaleX;
            targetObj.scaleY = targetBaseScaleY;

            string xAttribute = xmlNode.Attribute("x")?.Value ?? string.Empty;
            int sourceX = ParseCoordinateIntOrZero(xAttribute);
            float transformedX = (sourceX * scale) + offsetX + mapOffsetX;
            targetObj.x = support.x = transformedX;

            string yAttribute = xmlNode.Attribute("y")?.Value ?? string.Empty;
            int sourceY = ParseCoordinateIntOrZero(yAttribute);
            float transformedY = (sourceY * scale) + offsetY + mapOffsetY;
            targetObj.y = support.y = transformedY;

            float rotationAttribute = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value);
            targetObj.rotation = support.rotation = rotationAttribute;

            // Mouth hitbox, center-relative so skins of any size keep the same mouth line.
            // Desktop: derived from classic char_animations (640x640): bb = (264, 350, 108, 2).
            // Mobile: WP7 bb (90, 110, 25, 1) scaled x3 onto the same 640x640 sheet = (270, 330, 75, 3).
            targetObj.bb = ActivePhysicsConstants.UseMobilePhysicsModel
                ? MakeRectangle((targetObj.width >> 1) - 50f, (targetObj.height >> 1) + 10f, 75f, 3f)
                : MakeRectangle((targetObj.width >> 1) - 56f, (targetObj.height >> 1) + 30f, 108f, 2f);

            animation.Initialize(this);
            targetObj.ParseMover(xmlNode);

            // Register this Om Nom as an independent target. targets[0] stays the primary.
            targets.Add(new TargetContext(BLINK_SKIP, RND_RANGE(5, 20))
            {
                animation = animation,
                targetObject = targetObj,
                support = support,
                supportOffset = Vect(support.x - targetObj.x, support.y - targetObj.y),
                supportRotationOffset = support.rotation - targetObj.rotation,
                baseScaleX = targetBaseScaleX,
                baseScaleY = targetBaseScaleY,
            });

            // Show greeting if needed (skip for night levels).
            // Skins with startWithGreeting already play greeting on init, so skip the delayed call.
            if (RootController.IsShowGreeting())
            {
                if (!nightLevel && !animation.StartsWithGreeting)
                {
                    dd.CallObjectSelectorParamafterDelay(new DelayedDispatcher.DispatchFunc(Selector_showGreeting), null, 1.3f);
                }

                RootController.SetShowGreeting(false);
            }

            support = targets[0].support;
            targetBaseScaleX = targets[0].baseScaleX;
            targetBaseScaleY = targets[0].baseScaleY;
        }

        /// <summary>
        /// Picks the platform Om Nom sits on. Paddington seats him on the bear's suitcase. Otherwise
        /// the pack's platform set decides: the Experiments set takes the platform the pack's entry
        /// names or, when it names none that exists, the next in turn by the pack's position.
        /// </summary>
        /// <param name="xmlNode">The XML node describing Om Nom.</param>
        /// <param name="pack">Pack being played.</param>
        /// <param name="isPaddington">Whether the Paddington greeting is in play.</param>
        /// <param name="isExperiments">Whether the Experiments menus are active.</param>
        /// <returns>The support sheet and the quad to draw from it.</returns>
        internal static (string Resource, int Quad) ResolveSupport(XElement xmlNode, int pack, bool isPaddington, bool isExperiments)
        {
            string overrideTheme = xmlNode.Attribute("supportTheme")?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(overrideTheme))
            {
                int quad = ParseIntOrZero(xmlNode.Attribute("supportQuad")?.Value ?? string.Empty);
                return overrideTheme switch
                {
                    "xmas" => (Resources.Img.CharSupportsXmas, quad),
                    "experiments" => (Resources.Img.CharSupportExperiments, quad is >= 0 and < ExperimentsSupportCount ? quad : pack % ExperimentsSupportCount),
                    "extras" => (Resources.Img.CharSupportsExtras, quad is >= 0 and < ExtrasSupportCount ? quad : pack % ExtrasSupportCount),
                    _ => (Resources.Img.CharSupports, quad),
                };
            }
            if (isPaddington)
            {
                return (Resources.Img.CharSupportsXmas, PaddingtonSupportQuad);
            }
            SittingPlatformTheme theme = ResolveSupportTheme(PackConfig.GetSittingPlatformTheme(pack), isExperiments);
            if (theme == SittingPlatformTheme.Experiments)
            {
                int quad = PackConfig.GetExpSittingPlatform(pack);
                return (Resources.Img.CharSupportExperiments,
                    quad is >= 0 and < ExperimentsSupportCount ? quad : pack % ExperimentsSupportCount);
            }
            if (theme == SittingPlatformTheme.Extras)
            {
                int quad = PackConfig.GetExtraSittingPlatform(pack);
                return (Resources.Img.CharSupportsExtras,
                    quad is >= 0 and < ExtrasSupportCount ? quad : pack % ExtrasSupportCount);
            }
            return (Resources.Img.CharSupports, PackConfig.GetSittingPlatform(pack));
        }

        /// <summary>
        /// Picks the platform set: the one the pack asks for, else the one matching the menus.
        /// </summary>
        /// <param name="configured">Platform set from the pack's entry, or <see langword="null"/> when unset.</param>
        /// <param name="isExperiments">Whether the Experiments menus are active.</param>
        /// <returns>The platform set to draw from.</returns>
        internal static SittingPlatformTheme ResolveSupportTheme(SittingPlatformTheme? configured, bool isExperiments)
        {
            return configured ?? (isExperiments ? SittingPlatformTheme.Experiments : SittingPlatformTheme.Original);
        }
    }
}
