using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
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
			//CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, OnClanChangedKingdom);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, bool byRebellion, bool showNotification = true)
		{
		}

		private void OnClanTick(Clan clan)
		{
			var kingdom = clan.Kingdom;
			if (kingdom == null
				|| clan == Clan.PlayerClan
				|| !clan.Leader.IsAlive
				|| clan.Leader.IsPrisoner)
			{
				return;
			}
			var ruler = kingdom.Ruler;

			if (clan.Leader != ruler)
			{
				var rulerIsEnemy = clan.Leader.IsEnemy(ruler);
				var rulerIsFriend = clan.Leader.IsFriend(ruler);
				var rulerIsDifferentCulture = clan.Culture.GetCultureCode() != ruler.Culture.GetCultureCode();
				var hasReason = rulerIsEnemy || (!rulerIsFriend && rulerIsDifferentCulture);

				var kingdomFiefs = kingdom.Settlements.Sum(x => x.IsTown ? 2 : x.IsCastle ? 1 : 0);
				var clanFiefs = clan.Settlements.Sum(x => x.IsTown ? 2 : x.IsCastle ? 1 : 0);
				var hasEnoughFiefs = (double)clanFiefs / kingdomFiefs >= 0.1;
				
				if (hasReason && hasEnoughFiefs)
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
					clan.Color = color1;
					clan.Color2 = color2;
					var rebelKingdom = GoRebelKingdom(clan);

					GameLog.Warn($"Clan {clan.Name} is leaving {kingdom} to found their own {rebelKingdom}.");
				}
			}
			else
			{
				var noClans = kingdom.Clans.Where(x => x.Leader.IsAlive).Count() == 1;
				var noFiefs = clan.Settlements.Count() == 0;

				if (noClans && noFiefs)
				{
					ClanChangeKingdom(clan, null);

					GameLog.Warn($"{kingdom} was destroyed so clan {clan.Name} abandon it to find a new one.");
				}
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
				var kingdomName = "Kingdom of the {CLAN_NAME}";
				switch (clan.Culture.GetCultureCode())
				{
					case CultureCode.Aserai:
						kingdomName = "Sultanate of {CLAN_NAME}";
						break;
					case CultureCode.Khuzait:
						kingdomName = "{CLAN_NAME} Khanate";
						break;
					case CultureCode.Sturgia:
						kingdomName = "Principality of {CLAN_NAME}";
						break;
					case CultureCode.Empire:
						kingdomName = "{CLAN_NAME}'s Empire";
						break;
					default:
						break;
				}
				TextObject textObject2 = new TextObject("{=EXp18CLD}" + kingdomName, null);
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

		private void ClanChangeKingdom(Clan clan, Kingdom newKingdom)
		{
			Kingdom oldKingdom = clan.Kingdom;
			
			if (newKingdom != null)
			{
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
			}

			StatisticsDataLogHelper.AddLog(StatisticsDataLogHelper.LogAction.ChangeKingdomAction, new object[]
			{
				clan,
				oldKingdom,
				newKingdom,
				newKingdom == null
			});
			clan.IsUnderMercenaryService = false;
			clan.ClanLeaveKingdom(false);
			if (newKingdom != null)
			{
				clan.ClanJoinFaction(newKingdom);
				NotifyClanChangedKingdom(clan, oldKingdom, newKingdom, true, true);
				foreach (Clan c in oldKingdom.Clans)
				{
					int relationChange = 0;
					if (c.Leader == oldKingdom.Leader)
					{
						relationChange = -20;
					}
					else if (c.Leader.IsFriend(oldKingdom.Leader))
					{
						relationChange = -10;
					}
					else if (c.Leader.IsEnemy(oldKingdom.Leader))
					{
						relationChange = +10;
					}

					ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, c.Leader, relationChange, true);
				}
				DeclareWarAction.Apply(oldKingdom, newKingdom);
			}
			else // clan is leaving empty kingdom so we destroy it
			{
				NotifyClanChangedKingdom(clan, oldKingdom, null, false, true);
				DestroyKingdomAction.Apply(oldKingdom);
				ModifyKingdomList(kingdoms => kingdoms.RemoveAll(x => x == oldKingdom));
			}

			CheckIfPartyIconIsDirty(clan, oldKingdom);
		}

		private void NotifyClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, bool byRebellion, bool showNotification = true)
		{
			AccessTools.Method(CampaignEventDispatcher.Instance.GetType(), "OnClanChangedKingdom")
				.Invoke(CampaignEventDispatcher.Instance, new object[] { clan, oldKingdom, newKingdom, byRebellion, showNotification });
		}

		private void CheckIfPartyIconIsDirty(Clan clan, Kingdom oldKingdom)
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
