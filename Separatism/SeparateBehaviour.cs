using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using Helpers;
using HarmonyLib;

namespace Separatism
{
	public class SeparateBehaviour : CampaignBehaviorBase
	{
		private static Random rng = new Random();

		public override void RegisterEvents()
		{
			CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnClanTick);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnClanTick(Clan clan)
		{
			var kingdom = clan.Kingdom;
			if (kingdom == null || clan == Clan.PlayerClan) return;
			var ruler = kingdom.Ruler;

			if (clan.Leader.IsAlive
				&& !clan.Leader.IsFactionLeader
				&& !clan.Leader.IsPrisoner
				&& clan.Leader.IsEnemy(ruler)
				&& clan.Settlements.Where(x => x.IsCastle || x.IsTown).Count() >= 2)
			{
				var colors = BannerManager.ColorPalette.Values.Select(x => x.Color).ToList();
				uint color1 = TakeColor(colors);
				uint color2 = color1;
				while (colors.Count > 0 && ColorDiff(color1, color2) < 0.3)
				{
					color2 = TakeColor(colors);
				}

				clan.Banner.ChangePrimaryColor(color1);
				clan.Banner.ChangeIconColors(color2);
				var rebelKingdom = GoRebelKingdom(clan);

				GameLog.Warn($"Clan {clan.Name} leave kingdom {kingdom} to found their own {rebelKingdom}.");
			}
		}

		private uint TakeColor(List<uint> colors)
		{
			int index = rng.Next(colors.Count);
			uint color = colors[index];
			colors.RemoveAt(index);

			return color;
		}

		private double ColorDiff(uint color1, uint color2)
		{
			var gray1 = (0.2126 * (color1 >> 16 & 0xFF) + 0.7152 * (color1 >> 8 & 0xFF) + 0.0722 * (color1 & 0xFF)) / 255;
			var gray2 = (0.2126 * (color2 >> 16 & 0xFF) + 0.7152 * (color2 >> 8 & 0xFF) + 0.0722 * (color2 & 0xFF)) / 255;

			return Math.Abs(gray1 - gray2);
		}

		private string GetClanKingdomId(Clan clan)
		{
			return $"{clan.Name.ToString().ToLower()}_kingdom";
		}

		private Kingdom GoRebelKingdom(Clan clan)
		{
			string kingdomId = GetClanKingdomId(clan);
			var kingdom = Kingdom.All.SingleOrDefault(x => x.StringId == kingdomId);

			if (kingdom == null)
			{
				kingdom = MBObjectManager.Instance.CreateObject<Kingdom>(kingdomId);
				TextObject textObject = new TextObject("{=72pbZgQL}{CLAN_NAME}", null);
				textObject.SetTextVariable("CLAN_NAME", clan.Name);
				TextObject textObject2 = new TextObject("{=EXp18CLD}Kingdom of the {CLAN_NAME}", null);
				textObject2.SetTextVariable("CLAN_NAME", clan.Name);
				kingdom.InitializeKingdom(textObject2, textObject, clan.Culture, clan.Banner, clan.Color, clan.Color2, clan.InitialPosition);
				kingdom.RulingClan = clan;
			}

			ClanChangeKingdom(clan, kingdom);
			if (!Kingdom.All.Contains(kingdom))
			{
				ModifyKingdomList(kingdoms => kingdoms.Add(kingdom));
			}

			return kingdom;
		}

		private void ModifyKingdomList(Action<List<Kingdom>> modificator)
		{
			List<Kingdom> kingdoms = new List<Kingdom>(Campaign.Current.Kingdoms.ToList());
			modificator(kingdoms);
			AccessTools.Field(Campaign.Current.GetType(), "_kingdoms").SetValue(Campaign.Current, new MBReadOnlyList<Kingdom>(kingdoms));
		}

		private static void ClanChangeKingdom(Clan clan, Kingdom newKingdom)
		{
			Kingdom oldKingdom = clan.Kingdom;
			
			foreach (Kingdom k in Kingdom.All)
			{
				if (k == newKingdom || !newKingdom.IsAtWarWith(k))
				{
					FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(clan, k);
					FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(k, clan);
				}
			}
			foreach (Clan c in Clan.All)
			{
				if (c != clan && c.Kingdom == null && !newKingdom.IsAtWarWith(c))
				{
					FactionHelper.FinishAllRelatedHostileActions(clan, c);
				}
			}

			StatisticsDataLogHelper.AddLog(StatisticsDataLogHelper.LogAction.ChangeKingdomAction, new object[]
			{
				clan,
				oldKingdom,
				newKingdom,
				false
			});
			clan.IsUnderMercenaryService = false;
			clan.ClanLeaveKingdom(false);
			clan.ClanJoinFaction(newKingdom);
			foreach (Clan c in oldKingdom.Clans)
			{
				ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, c.Leader, -10, true);
			}
			DeclareWarAction.Apply(oldKingdom, newKingdom);
			
			//CampaignEventDispatcher.Instance.OnClanChangedKingdom(clan, oldKingdom, newKingdom, true, true);
			CheckIfPartyIconIsDirty(clan, oldKingdom);
		}

		private static void CheckIfPartyIconIsDirty(Clan clan, Kingdom oldKingdom)
		{
			IFaction faction;
			if (clan.Kingdom == null)
			{
				faction = clan;
			}
			else
			{
				IFaction kingdom = clan.Kingdom;
				faction = kingdom;
			}
			IFaction faction2 = faction;
			IFaction faction3 = (IFaction)oldKingdom ?? clan;
			foreach (MobileParty mobileParty in MobileParty.All)
			{
				if (mobileParty.IsVisible && ((mobileParty.Party.Owner != null && mobileParty.Party.Owner.Clan == clan) || (clan == Clan.PlayerClan && ((!FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction2) && FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction3)) || (FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction2) && !FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction3))))))
				{
					mobileParty.Party.Visuals.SetMapIconAsDirty();
				}
			}
			foreach (Settlement settlement in clan.Settlements)
			{
				settlement.Party.Visuals.SetMapIconAsDirty();
			}
		}
	}
}
