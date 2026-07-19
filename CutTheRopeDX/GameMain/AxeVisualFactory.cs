using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal static class AxeDefinition
    {
        public static CandyCapabilities Capabilities => CandyCapabilities.Axe;

        public static bool EmitsLight => false;

        public const float ChainCutRadius = 64f;

        public const float HazardCollisionDistance = 1.35f * GameScene.STAR_RADIUS;
    }

    internal static class AxeVisualFactory
    {
        public static CandyContext Create(ConstraintedPoint point, string axeNumber)
        {
            Axe axe = new(point, axeNumber);

            return new CandyContext
            {
                candyNumber = null,
                axeNumber = axe.axeNumber,
                point = point,
                candy = axe,
                candyMain = axe,
                axe = axe,
                candyBubbleAnimation = axe.bubbleAnimation,
                candyGhostBubbleAnimation = axe.ghostBubbleAnimation,
                Capabilities = AxeDefinition.Capabilities,
                emitsLight = AxeDefinition.EmitsLight,
                collisionDistanceOverride = AxeDefinition.HazardCollisionDistance,
                noCandy = false,
            };
        }
    }
}
