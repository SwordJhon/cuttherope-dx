using System.Xml.Linq;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a bouncer object from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the bouncer.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadBouncer(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            int size = ParseIntOrZero(xmlNode.Attribute("size")?.Value);
            float angle = ParseIntOrZero(xmlNode.Attribute("angle")?.Value);
            float k = ParseFloatOrZero(xmlNode.Attribute("moveLength")?.Value) * scale;
            _ = bool.TryParse(xmlNode.Attribute("moveVertical")?.Value, out bool v);
            float a = ParseFloatOrDefault(xmlNode.Attribute("moveAngle")?.Value, v ? DEG_90 : 0);
            float o = ParseFloatOrZero(xmlNode.Attribute("moveOffset")?.Value) * scale;
            Bouncer bouncer = new Bouncer().InitWithPosXYWidthAndAngle(x, y, size, angle);
            bouncer.bindToDisc = GetBoolAttribute(xmlNode, "bindToDisc", false);
            bouncer.ParseMover(xmlNode);
            GrabAxes axes = GrabAxisResolver.Resolve(
                new GrabAxisRequest(
                    Gun: false,
                    Wheel: false,
                    Kickable: false,
                    Radius: 0,
                    MoveLength: k,
                    HasMover: bouncer.mover != null,
                    MoveAngle: a,
                    MoveOffset: o,
                    AnchorX: x,
                    AnchorY: y),
                startsKicked: false);
            bouncer.Motion = axes.Motion;
            bouncer.CreateRailVisuals();
            bouncers.Add(bouncer);
        }
    }
}
