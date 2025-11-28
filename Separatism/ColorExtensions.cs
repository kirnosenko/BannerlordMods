using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Separatism
{
	public static class ColorExtensions
	{
		public static (uint, uint) GetRebelKingdomColors()
		{
			var colors = BannerManager.Instance.ReadOnlyColorPalette.Values.Select(x => x.Color).Distinct().ToArray();
			uint color1 = colors.Max();
			uint color2 = colors.Min();

			if (!SeparatismConfig.Settings.SameColorsForAllRebels)
			{
				var kingdomColors = Kingdom.All.Select(x => x.GetColors()).ToArray();
				var primaryColors = colors.Except(kingdomColors.Select(x => x.color1)).ToList();
				var secondaryColors = colors.Except(kingdomColors.Select(x => x.color2)).ToList();

				if (primaryColors.Count > 0 && secondaryColors.Count > 0)
				{
					color1 = TakeRandomColor(primaryColors);
					color2 = color1;
					while (secondaryColors.Count > 0 && ColorDiff(color1, color2) < 0.3)
					{
						color2 = TakeRandomColor(secondaryColors);
					}
				}
			}

			return (color1, color2);
		}

		private static uint TakeRandomColor(List<uint> colors)
		{
			int index = MBRandom.RandomInt(colors.Count);
			uint color = colors[index];
			colors.RemoveAt(index);
			return color;
		}

		private static double ColorDiff(uint color1, uint color2)
		{
			var gray1 = (0.2126 * (color1 >> 16 & 0xFF) + 0.7152 * (color1 >> 8 & 0xFF) + 0.0722 * (color1 & 0xFF)) / 255;
			var gray2 = (0.2126 * (color2 >> 16 & 0xFF) + 0.7152 * (color2 >> 8 & 0xFF) + 0.0722 * (color2 & 0xFF)) / 255;

			return Math.Abs(gray1 - gray2);
		}
	}
}
