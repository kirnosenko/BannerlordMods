using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Separatism
{
	public static class FactionExtension
	{
		public static (uint color1, uint color2) GetColors(this IFaction faction)
		{
			var bannerData = faction.Banner.BannerDataList;
			var color1 = faction.Color;
			var color2 = faction.Color2;

			if (bannerData != null && bannerData.Count > 0)
			{
				color1 = BannerManager.GetColor(bannerData[0].ColorId);
				if (bannerData.Count > 1)
				{
					color2 = BannerManager.GetColor(bannerData[1].ColorId);
				}
			}

			return (color1, color2);
		}

		public static int GetFiefsAmount(this IFaction faction)
		{
			return faction.Settlements.Sum(x => x.IsTown ? 2 : x.IsCastle ? 1 : 0);
		}
	}
}
