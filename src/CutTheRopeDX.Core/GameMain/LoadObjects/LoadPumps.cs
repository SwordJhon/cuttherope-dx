using System.Xml.Linq;

using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a pump object from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the pump.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadPump(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            Pump pump = Image.InitializeFromResource(new Pump(), Resources.Img.ObjPump);
            pump.DoRestoreCutTransparency();
            _ = pump.AddAnimationWithDelayLoopedCountSequence(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 1, [2, 3, 0]);
            pump.bb = GetPumpBoundingBox();
            pump.initial_x = pump.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            pump.initial_y = pump.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            pump.initial_rotation = 0f;
            pump.initial_rotatedCircle = null;
            pump.bindToDisc = GetBoolAttribute(xmlNode, "bindToDisc", true);
            pump.rotateInDisc = GetBoolAttribute(xmlNode, "rotateInDisc", true);
            pump.anchor = 18;
            pump.ParseMover(xmlNode);
            pump.rotation += DEG_90;
            if (pump.mover != null)
            {
                pump.mover.angle_ += DEG_90;
                pump.mover.angle_initial = pump.mover.angle_;
            }
            pump.UpdateRotation();
            pumps.Add(pump);
        }
    }
}
