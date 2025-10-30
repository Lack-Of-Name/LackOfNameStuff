using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;

namespace LackOfNameStuff.Common
{
	public static class Rarity
	{
		private sealed class CycleProfile
		{
			public CycleProfile(double durationSeconds, Color[] colors)
			{
				DurationSeconds = Math.Max(0.1, durationSeconds);
				Colors = colors;
			}

			public double DurationSeconds { get; }
			public Color[] Colors { get; }
		}

		private static readonly Dictionary<string, CycleProfile> _itemProfiles = new();

		public static string GetCyclingColorTag(Color color) => $"[c/{color.R:X2}{color.G:X2}{color.B:X2}]";

		public static void RegisterItemCycle(string localizationKey, double durationSeconds, params Color[] colors)
		{
			if (string.IsNullOrWhiteSpace(localizationKey) || colors == null || colors.Length == 0)
			{
				_itemProfiles.Remove(localizationKey ?? string.Empty);
				return;
			}

			Color[] copy = new Color[colors.Length];
			Array.Copy(colors, copy, colors.Length);

			_itemProfiles[localizationKey] = new CycleProfile(durationSeconds, copy);
		}

		public static bool TryBuildCyclingName(string localizationKey, out string styledName)
		{
			styledName = null;
			if (string.IsNullOrWhiteSpace(localizationKey) || !_itemProfiles.TryGetValue(localizationKey, out CycleProfile profile))
			{
				return false;
			}

			styledName = BuildCyclingName(localizationKey, profile);
			return true;
		}

		public static string GetLocalizedName(string localizationKey)
		{
			return TryBuildCyclingName(localizationKey, out string styled)
				? styled
				: Language.GetTextValue(localizationKey);
		}

		private static string BuildCyclingName(string localizationKey, CycleProfile profile)
		{
			ReadOnlySpan<Color> colors = profile.Colors;
			double duration = profile.DurationSeconds;

			double time = Main.GlobalTimeWrappedHourly * 3600.0;
			double progress = time / duration;
			double wrappedProgress = progress - Math.Floor(progress);
			int currentIndex = (int)Math.Floor(progress) % colors.Length;
			if (currentIndex < 0)
			{
				currentIndex += colors.Length;
			}
			int nextIndex = (currentIndex + 1) % colors.Length;
			float blend = (float)wrappedProgress;

			Color interpolated = Color.Lerp(colors[currentIndex], colors[nextIndex], blend);
			string baseText = Language.GetTextValue(localizationKey);
			return $"{GetCyclingColorTag(interpolated)}{baseText}[/c]";
		}
	}
}
