using System.Collections.Generic;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>A decal object that can hold any texture inside <see cref="Resources.Img"/>.</summary>
    internal sealed class Decal : GameObject
    {
        /// <summary>Chooses what part of the texture this decal shows.</summary>
        /// <param name="quad">A quad index, or -1 to draw the entire texture.</param>
        /// <returns><see langword="false"/> when the quad does not exist in the texture.</returns>
        public bool ShowSource(int quad)
        {
            if (quad < 0 || texture.quadsCount == 0)
            {
                SetDrawFullImage();
                return true;
            }
            if (quad >= texture.quadsCount)
            {
                return false;
            }
            SetDrawQuad(quad);
            return true;
        }

        /// <summary>Plays the given quads in order.</summary>
        /// <param name="quads">Quad indices, in playback order.</param>
        /// <param name="delay">Seconds each frame is shown.</param>
        /// <param name="loopType">A <see cref="Timeline.LoopType"/> for the animation.</param>
        /// <returns><see langword="false"/> when a quad does not exist in the texture.</returns>
        public bool Animate(List<int> quads, float delay, Timeline.LoopType loopType)
        {
            foreach (int quad in quads)
            {
                if (quad < 0 || quad >= texture.quadsCount)
                {
                    return false;
                }
            }
            SetDrawQuad(quads[0]);
            AddAnimationWithIDDelayLoopCountSequence(0, delay, loopType, quads.Count, quads[0], quads.GetRange(1, quads.Count - 1));
            PlayTimeline(0);
            return true;
        }
    }
}
