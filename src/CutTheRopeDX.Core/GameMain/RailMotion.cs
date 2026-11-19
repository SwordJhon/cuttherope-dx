using System;
using System.Numerics;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Framework.Helpers.MathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A hook the player drags along a straight rail. The rail is player-driven, so no platform may
    /// capture it. Owns the rail's geometry, its three images, and the drag itself, which used to be
    /// spread across three separate blocks in <c>GameScene.Input</c>.
    /// </summary>
    internal sealed class RailMotion : AnchorMotion
    {
        /// <summary>Half-width of the square tap zone that starts a rail drag.</summary>
        private const float DragTapRadius = 65f;

        /// <summary>Initializes a rail.</summary>
        /// <param name="length">Rail length in world units.</param>
        /// <param name="angle">The angle of the rail in degrees.</param>
        /// <param name="offset">The object's offset along the rail from its authored end.</param>
        /// <param name="anchorX">The object's authored X position.</param>
        /// <param name="anchorY">The object's authored Y position.</param>
        public RailMotion(float length, float angle, float offset, float anchorX, float anchorY)
        {
            Length = length;
            Angle = angle;
            Offset = offset;
            DraggingTouch = -1;

            float radians = Angle * MathF.PI / 180;
            Direction = new Vector2(MathF.Cos(radians), MathF.Sin(radians));
            StartPos = new Vector2(anchorX, anchorY) - (Direction * Offset);
        }

        /// <summary>Gets the rail length.</summary>
        public float Length { get; }

        /// <summary>Gets the angle of the rail.</summary>
        public float Angle { get; }

        /// <summary>Gets the hook's offset along the rail from its authored end.</summary>
        public float Offset { get; }

        /// <summary>Gets the direction of the hook.</summary>
        public Vector2 Direction { get; }

        /// <summary>Gets the start position of the hook.</summary>
        private Vector2 StartPos { get; }

        /// <summary>Gets the touch index currently dragging the hook, or -1 when idle.</summary>
        public int DraggingTouch { get; private set; }

        /// <summary>Gets or sets the tiled rail background image.</summary>
        public HorizontallyTiledImage Background { get; set; }

        /// <summary>Gets or sets the draggable hook image.</summary>
        public Image Mover { get; set; }

        /// <summary>Gets or sets the highlighted draggable hook image.</summary>
        public Image MoverHighlight { get; set; }

        /// <inheritdoc />
        public override bool CanBind => false;

        /// <summary>Starts a drag when a touch lands on the hook.</summary>
        /// <param name="obj">The object being dragged.</param>
        /// <param name="worldX">Touch X in world space.</param>
        /// <param name="worldY">Touch Y in world space.</param>
        /// <param name="touchIndex">Touch index.</param>
        /// <returns><see langword="true"/> when this touch now owns the object.</returns>
        public bool TryBeginDrag(GameObject obj, float worldX, float worldY, int touchIndex)
        {
            if (!PointInRect(
                    worldX, worldY,
                    obj.x - DragTapRadius, obj.y - DragTapRadius,
                    DragTapRadius * 2f, DragTapRadius * 2f))
            {
                return false;
            }

            DraggingTouch = touchIndex;
            return true;
        }

        /// <summary>Slides the object to follow an active drag.</summary>
        /// <param name="obj">The object being dragged.</param>
        /// <param name="worldX">Touch X in world space.</param>
        /// <param name="worldY">Touch Y in world space.</param>
        public void DragTo(GameObject obj, float worldX, float worldY)
        {
            Vector2 touchPos = new(worldX, worldY);
            float distance = Vector2.Dot(touchPos - StartPos, Direction);
            float clampedDistance = Math.Clamp(distance, 0f, Length);
            Vector2 finalPos = StartPos + (clampedDistance * Direction);
            obj.x = finalPos.X;
            obj.y = finalPos.Y;
        }

        /// <summary>Ends the drag if this touch owns it.</summary>
        /// <param name="touchIndex">Touch index being released.</param>
        public void EndDrag(int touchIndex)
        {
            if (DraggingTouch == touchIndex)
            {
                DraggingTouch = -1;
            }
        }
    }
}
