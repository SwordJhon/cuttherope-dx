using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a decal from XML node data and positions it in the scene.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the decal.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadDecal(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            string texture = xmlNode.Attribute("texture")?.Value;
            if (string.IsNullOrEmpty(texture))
            {
                return;
            }
            Decal decal = Image.InitializeFromResource(new Decal(), texture);
            if (!ParseDecalQuads(decal, xmlNode))
            {
                return;
            }

            float x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float angle = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value);
            decal.x = x;
            decal.y = y;
            decal.rotation = angle;

            float scaleX = ParseFloatOrDefault(xmlNode.Attribute("scaleX")?.Value, 1f);
            float scaleY = ParseFloatOrDefault(xmlNode.Attribute("scaleY")?.Value, 1f);
            if (GetBoolAttribute(xmlNode, "retinaScale", false))
            {
                scaleX *= FlashXmlScale.PcOutputScale;
                scaleY *= FlashXmlScale.PcOutputScale;
            }
            if (GetBoolAttribute(xmlNode, "flipX", false))
            {
                scaleX *= -1;
            }
            if (GetBoolAttribute(xmlNode, "flipY", false))
            {
                scaleY *= -1;
            }
            decal.scaleX = scaleX;
            decal.scaleY = scaleY;

            decal.TryApplyTint(ParseColor(xmlNode.Attribute("color")?.Value));
            decal.ParseMover(xmlNode);
            decal.Update(0f);

            bool isFront = GetBoolAttribute(xmlNode, "frontLayer", false);
            if (isFront)
            {
                frontDecals.Add(decal);
            }
            else
            {
                backDecals.Add(decal);
            }
        }

        private static bool ParseDecalQuads(Decal decal, XElement xmlNode)
        {
            string raw = xmlNode.Attribute("quad")?.Value;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return true;
            }

            List<int> quads = [];
            foreach (string part in raw.Split(','))
            {
                if (!int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int quad))
                {
                    return false;
                }
                quads.Add(quad);
            }

            if (quads.Count == 1)
            {
                return decal.ShowSource(quads[0]);
            }

            float delay = ParseFloatOrDefault(xmlNode.Attribute("speed")?.Value, 1 / 20f);
            Timeline.LoopType loopType = xmlNode.Attribute("loopType")?.Value switch
            {
                "noloop" => Timeline.LoopType.TIMELINE_NO_LOOP,
                "pingpong" => Timeline.LoopType.TIMELINE_PING_PONG,
                "replay" => Timeline.LoopType.TIMELINE_REPLAY,
                _ => Timeline.LoopType.TIMELINE_REPLAY,
            };
            return decal.Animate(quads, delay, loopType);
        }
    }
}
