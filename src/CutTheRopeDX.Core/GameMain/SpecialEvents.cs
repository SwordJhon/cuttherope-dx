using System;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Provides helper properties to determine whether seasonal events
    /// should be active based on the current system date.
    /// </summary>
    internal static class SpecialEvents
    {
        /// <summary>
        /// Date the events are judged by on this thread, or <see langword="null"/> for today.
        /// Tests pin it so a seasonal dressing does not depend on the day they run.
        /// </summary>
        [ThreadStatic]
        internal static DateTime? PinnedDate;

        /// <summary>Gets the date the events are judged by.</summary>
        private static DateTime Today => PinnedDate ?? DateTime.Now;

        #region Christmas event

        /// <summary>
        /// Gets a value indicating whether the current month is January.
        /// </summary>
        public static bool IsJanuary => (Today.Month is 1 && !Preferences.GetBooleanForKey("FORCE_EVENT_JANUARY_OFF"))
        || Preferences.GetBooleanForKey("FORCE_EVENT_JANUARY_ON");

        /// <summary>
        /// Gets a value indicating whether the Christmas event period is active.
        /// Includes December and January.
        /// </summary>
        public static bool IsXmas => (Today.Month is 12 or 1 && !Preferences.GetBooleanForKey("FORCE_EVENT_XMAS_OFF"))
        || Preferences.GetBooleanForKey("FORCE_EVENT_XMAS_ON");

        #endregion

        #region Halloween event

        /// <summary>
        /// Gets a value indicating whether the Halloween event period is active.
        /// </summary>
        public static bool IsHalloween => (Today.Month is 10 && !Preferences.GetBooleanForKey("FORCE_EVENT_HALLOWEEN_OFF"))
        || Preferences.GetBooleanForKey("FORCE_EVENT_HALLOWEEN_ON");

        #endregion
    }
}
