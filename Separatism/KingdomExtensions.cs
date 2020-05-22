using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Separatism
{
	public static class KingdomExtensions
	{
		public static Kingdom[] CloseKingdoms(this Clan clan)
		{
			Vec2 clanPosition = Vec2.Zero;
			int counter = 0;
			foreach (var spos in clan.Settlements.Where(x => x.IsTown || x.IsCastle).Select(x => x.Position2D))
			{
				clanPosition += spos;
				counter++;
			}
			if (counter == 0)
			{
				return Kingdom.All.ToArray();
			}
			clanPosition *= ((float)1 / counter);

			var kingdomDistance = Kingdom.All
				.Select(k => (k, k.FactionMidPoint.Distance(clanPosition)))
				.ToArray();
			var average = kingdomDistance.Average(x => x.Item2);
			return kingdomDistance
				.Where(x => x.Item2 <= average)
				.OrderBy(x => x.Item2)
				.Select(x => x.k)
				.ToArray();
		}

		public static bool IsInsideKingdomTeritory(this Kingdom kingdom1, Kingdom kingdom2)
		{
			var positions1 = kingdom1.Settlements.Where(x => x.IsTown || x.IsCastle)
				.Select(x => x.Position2D).ToArray();
			var positions2 = kingdom2.Settlements.Where(x => x.IsTown || x.IsCastle)
				.Select(x => x.Position2D).ToArray();

			return positions1.Max(p => p.X) <= positions2.Max(p => p.X) &&
				positions1.Max(p => p.Y) <= positions2.Max(p => p.Y) &&
				positions1.Min(p => p.X) >= positions2.Min(p => p.X) &&
				positions1.Min(p => p.Y) >= positions2.Min(p => p.Y);
		}

		public static void SetKingdomText(this Kingdom kingdom)
		{
			if (kingdom.EncyclopediaText == null)
			{
				AccessTools.Property(typeof(Kingdom), "EncyclopediaText").SetValue(kingdom, TextObject.Empty);
			}
			if (kingdom.EncyclopediaTitle == null || kingdom.EncyclopediaRulerTitle == null)
			{
				var kingdomNameText = TextObject.Empty;
				var kingdomRulerTitleText = TextObject.Empty;
				if (kingdom.RulingClan != null)
				{
					kingdom.RulingClan.GetKingdomNameAndRulerTitle(out kingdomNameText, out kingdomRulerTitleText);
				}
				AccessTools.Property(typeof(Kingdom), "EncyclopediaTitle").SetValue(kingdom, kingdomNameText);
				AccessTools.Property(typeof(Kingdom), "EncyclopediaRulerTitle").SetValue(kingdom, kingdomRulerTitleText);
			}
		}

		public static void AddKingdom(this Campaign campaign, Kingdom kingdom)
		{
			ModifyKingdomList(campaign, kingdoms =>
			{
				if (!kingdoms.Contains(kingdom))
				{
					kingdoms.Add(kingdom);
					return kingdoms;
				}

				return null;
			});
		}

		public static void RemoveEmptyKingdoms(this Campaign campaign, Action<Kingdom> callBack = null)
		{
			ModifyKingdomList(campaign, kingdoms =>
			{
				var emptyKingdomsToRemove = kingdoms
					.Where(k =>
						k.Clans.Where(x => x.Leader.IsAlive).Count() == 0 &&
						(!SeparatismConfig.Settings.KeepEmptyKingdoms || k.RulingClan?.GetKingdomId() == k.StringId))
					.ToArray();
				if (emptyKingdomsToRemove.Length > 0)
				{
					foreach (var kingdom in emptyKingdomsToRemove)
					{
						callBack?.Invoke(kingdom);
						DestroyKingdomAction.Apply(kingdom);
					}
					return kingdoms.Except(emptyKingdomsToRemove).ToList();
				}

				return null;
			});
		}

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

		private static void ModifyKingdomList(this Campaign campaign, Func<List<Kingdom>, List<Kingdom>> modificator)
		{
			List<Kingdom> kingdoms = campaign.Kingdoms.ToList();
			kingdoms = modificator(kingdoms);
			if (kingdoms != null)
			{
				AccessTools.Field(Campaign.Current.GetType(), "_kingdoms").SetValue(Campaign.Current, new MBReadOnlyList<Kingdom>(kingdoms));
			}
		}
	}
}
