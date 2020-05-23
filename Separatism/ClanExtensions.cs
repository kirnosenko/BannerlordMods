using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Separatism
{
	public static class ClanExtensions
	{
		public static string GetKingdomId(this Clan clan)
		{
			return $"{clan.Name.ToString().ToLower()}_kingdom";
		}

		public static void GetKingdomNameAndRulerTitle(this Clan clan, out TextObject kingdomNameText, out TextObject kingdomRulerTitleText)
		{
			var kingdomName = "{=Separatism_Kingdom_Name}Kingdom of {ClanName}";
			var kingdomRulerTitle = "{=Separatism_Kingdom_Ruler_Title}King";

			switch (clan.Culture.GetCultureCode())
			{
				case CultureCode.Aserai:
					kingdomName = "{=Separatism_Sultanate_Name}Sultanate of {ClanName}";
					kingdomRulerTitle = "{=Separatism_Sultanate_Ruler_Title}Sultan";
					break;
				case CultureCode.Khuzait:
					kingdomName = "{=Separatism_Khanate_Name}Khanate of {ClanName}";
					kingdomRulerTitle = "{=Separatism_Khanate_Ruler_Title}Khan";
					break;
				case CultureCode.Sturgia:
					kingdomName = "{=Separatism_Principality_Name}Principality of {ClanName}";
					kingdomRulerTitle = "{=Separatism_Principality_Ruler_Title}Knyaz";
					break;
				case CultureCode.Empire:
					kingdomName = "{=Separatism_Empire_Name}Empire of {ClanName}";
					kingdomRulerTitle = "{=Separatism_Empire_Ruler_Title}Emperor";
					break;
				default:
					break;
			}

			kingdomNameText = new TextObject(kingdomName, null);
			kingdomNameText.SetTextVariable("ClanName", clan.Name);
			kingdomRulerTitleText = new TextObject(kingdomRulerTitle, null);
		}

		public static (uint, uint) GetColors(this Clan clan)
		{
			var bannerData = clan.Banner.BannerDataList;
			var color1 = clan.Color;
			var color2 = clan.Color2;

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
	}
}
