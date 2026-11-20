using System;
using System.Globalization;

using CutTheRopeDX.Framework;

namespace CutTheRopeDX.Helpers
{
    /// <summary>
    /// Provides safe parsing methods that return default values instead of throwing on invalid input.
    /// </summary>
    public static class ParsingHelpers
    {
        /// <summary>
        /// Parses an integer from a string, returning 0 if the value is <see langword="null"/>, empty, or not a valid integer.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed integer, or 0 if parsing fails.</returns>
        public static int ParseIntOrZero(string value)
        {
            return ParseIntOrDefault(value, 0);
        }

        /// <summary>
        /// Parses a map coordinate using invariant culture and truncates its fractional part toward zero.
        /// Returns 0 when the value is missing, invalid, or outside the range of an integer.
        /// </summary>
        /// <param name="value">The coordinate string to parse.</param>
        /// <returns>The truncated integer coordinate, or 0 if parsing fails.</returns>
        public static int ParseCoordinateIntOrZero(string value)
        {
            return ParseCoordinateIntOrDefault(value, 0);
        }

        /// <summary>
        /// Parses a floating-point number from a string using invariant culture, returning 0 if the value
        /// is <see langword="null"/>, empty, or not a valid number.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed float, or 0 if parsing fails.</returns>
        public static float ParseFloatOrZero(string value)
        {
            return ParseFloatOrDefault(value, 0f);
        }

        /// <summary>
        /// Parses an integer from a string, returning a fallback if the value is <see langword="null"/>, empty, or not a valid integer.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="fallback">The fallback to use.</param>
        /// <returns>The parsed integer, or a fallback if parsing fails.</returns>
        public static int ParseIntOrDefault(string value, int fallback)
        {
            return int.TryParse(value, out int parsed) ? parsed : fallback;
        }

        /// <summary>
        /// Parses a map coordinate using invariant culture and truncates its fractional part toward zero.
        /// Returns a fallback when the value is missing, invalid, or outside the range of an integer.
        /// </summary>
        /// <param name="value">The coordinate string to parse.</param>
        /// <param name="fallback">The fallback to use.</param>
        /// <returns>The truncated integer coordinate, or a fallback if parsing fails.</returns>
        public static int ParseCoordinateIntOrDefault(string value, int fallback)
        {
            return !decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsed)
                || parsed < int.MinValue
                || parsed > int.MaxValue
                ? fallback
                : decimal.ToInt32(decimal.Truncate(parsed));
        }

        /// <summary>
        /// Parses a floating-point number from a string using invariant culture, returning a fallback if the value
        /// is <see langword="null"/>, empty, or not a valid number.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="fallback">The fallback to use.</param>
        /// <returns>The parsed float, or a fallback if parsing fails.</returns>
        public static float ParseFloatOrDefault(string value, float fallback)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : fallback;
        }

        /// <summary>Parses an authored <c>#RRGGBB</c> color.</summary>
        /// <param name="value">Authored value, or <see langword="null"/> for no override.</param>
        /// <returns>The parsed color, or <see langword="null"/> when value is invalid.</returns>
        public static RGBAColor? ParseColor(string value)
        {
            return value is null
                ? null
                : value.StartsWith('#')
                    ? ParseHex(value)
                    : ParseChannels(value);
        }

        /// <summary>
        /// Parses a <c>R,G,B</c> quatruplet of 0-255 channels, the way pack configs already spell a
        /// color (<c>boxHoleBgColor: [70, 37, 0]</c>). Spaces around a channel are allowed so a
        /// value can be copied straight across.
        /// </summary>
        /// <param name="value">Authored quatruplet.</param>
        /// <returns>The parsed color.</returns>
        private static RGBAColor? ParseChannels(string value)
        {
            string[] parts = value.Split(',');
            return parts.Length == 4
                ? RGBAColor.MakeRGBA(
                    Channel(parts[0]),
                    Channel(parts[1]),
                    Channel(parts[2]),
                    Channel(parts[3]))
                : parts.Length == 3
                ? RGBAColor.MakeRGBA(
                    Channel(parts[0]),
                    Channel(parts[1]),
                    Channel(parts[2]),
                    1f) : null;
        }

        private static float Channel(string part)
        {
            return int.TryParse(
                part.Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int channel)
                && channel is >= 0 and <= 255
                    ? channel / 255f
                    : 1f;
        }

        private static RGBAColor? ParseHex(string value)
        {
            return int.TryParse(
                    value.AsSpan(1),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out int packed)
                    ? value.Length == 9
                    ? RGBAColor.MakeRGBA(
                        ((packed >> 24) & 0xFF) / 255f,
                        ((packed >> 16) & 0xFF) / 255f,
                        ((packed >> 8) & 0xFF) / 255f,
                        (packed & 0xFF) / 255f)
                        : value.Length == 7
                        ? RGBAColor.MakeRGBA(
                        ((packed >> 16) & 0xFF) / 255f,
                        ((packed >> 8) & 0xFF) / 255f,
                        (packed & 0xFF) / 255f
                        , 1f)
                        : null
                    : null;
        }
    }
}
