using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace Separatism
{
	public static class KingdomExtensions
	{
		public static Hero Ruler(this Kingdom kingdom) =>
			kingdom.RulingClan.Leader;

		public static Kingdom[] CloseKingdoms(this Clan clan)
		{
			var kingdomDistance = Kingdom.All
				.Select(k => (k, k.FactionMidSettlement.Position.Distance(clan.FactionMidSettlement.Position)))
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
				.Select(x => x.Position).ToArray();
			var positions2 = kingdom2.Settlements.Where(x => x.IsTown || x.IsCastle)
				.Select(x => x.Position).ToArray();

			return positions1.Max(p => p.X) <= positions2.Max(p => p.X) &&
				positions1.Max(p => p.Y) <= positions2.Max(p => p.Y) &&
				positions1.Min(p => p.X) >= positions2.Min(p => p.X) &&
				positions1.Min(p => p.Y) >= positions2.Min(p => p.Y);
		}

		public static void InheritsWarsFromKingdom(this Kingdom dest, Kingdom src)
		{
			if (SeparatismConfig.Settings.KeepOriginalKindomWars)
			{
				var oldKingdomEnemies = FactionHelper.GetEnemyKingdoms(src).ToArray();
				foreach (var enemy in oldKingdomEnemies)
				{
					DeclareWarAction.ApplyByDefault(enemy, dest);
				}
			}
		}

		public static void SetKingdomText(this Kingdom kingdom)
		{
			if (kingdom.EncyclopediaText == null)
			{
				AccessTools.Property(typeof(Kingdom), "EncyclopediaText").SetValue(kingdom, TextObject.GetEmpty());
			}
			if (kingdom.EncyclopediaTitle == null || kingdom.EncyclopediaRulerTitle == null)
			{
				var kingdomNameText = TextObject.GetEmpty();
				var kingdomRulerTitleText = TextObject.GetEmpty();
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
			AccessTools.Method(typeof(CampaignObjectManager), "AddKingdom")
				.Invoke(campaign.CampaignObjectManager, new object[] { kingdom });
		}

		public static void RemoveEmptyKingdoms(this Campaign campaign, Action<Kingdom> callBack = null)
		{
			var kingdoms = (List<Kingdom>)AccessTools.Field(typeof(CampaignObjectManager), "_kingdoms").GetValue(campaign.CampaignObjectManager);
			var factions = (List<IFaction>)AccessTools.Field(typeof(CampaignObjectManager), "_factions").GetValue(campaign.CampaignObjectManager);

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
					kingdoms.Remove(kingdom);
					factions.Remove(kingdom);
				}
			}
		}
	}
}
