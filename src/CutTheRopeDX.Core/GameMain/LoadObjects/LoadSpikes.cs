using System.Xml.Linq;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a spike object from XML node data
        /// Supports regular spikes (spike1-4), electro spikes and origins spikes
        /// </summary>
        /// <param name="xmlNode">The XML node describing the spikes.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadSpike(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float py = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            int w = ParseIntOrZero(xmlNode.Attribute("size")?.Value);
            float an = ParseIntOrZero(xmlNode.Attribute("angle")?.Value);
            string toggledAttribute = xmlNode.Attribute("toggled")?.Value ?? string.Empty;
            int toggledState = -1;
            _ = bool.TryParse(xmlNode.Attribute($"reversed")?.Value, out bool reversed);
            bool isElectro = GetBoolAttribute(xmlNode, "electro", defaultValue: xmlNode.Name.LocalName == "electro");
            int spikesAmount = ParseIntOrZero(xmlNode.Attribute("spikesAmount")?.Value);
            bool isOrigins = spikesAmount > 0 || xmlNode.Name.LocalName is "spikeOrigins" or "spikeo";
            if (toggledAttribute.Length > 0)
            {
                toggledState = toggledAttribute == "false" ? -1 : ParseIntOrZero(toggledAttribute);
            }
            Spikes spikes = new Spikes().InitWithPosXYWidthAndAngleToggled(px, py, w, an, toggledState, reversed, spikesAmount, isElectro, isOrigins);
            spikes.ParseMover(xmlNode);
            if (toggledState != 0)
            {
                spikes.delegateRotateAllSpikesWithID = new Spikes.rotateAllSpikesWithID(RotateAllSpikesWithID);
            }
            if (isElectro)
            {
                spikes.initialDelay = ParseFloatOrZero(xmlNode.Attribute("initialDelay")?.Value);
                spikes.onTime = ParseFloatOrZero(xmlNode.Attribute("onTime")?.Value);
                spikes.offTime = ParseFloatOrZero(xmlNode.Attribute("offTime")?.Value);
                spikes.electroTimer = 0f;
                spikes.TurnElectroOff();
                spikes.electroTimer += spikes.initialDelay;
                spikes.UpdateRotation();
            }
            this.spikes.Add(spikes);
        }
    }
}
