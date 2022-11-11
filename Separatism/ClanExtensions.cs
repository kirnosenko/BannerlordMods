﻿using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using Helpers;
using HarmonyLib;

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

		public static Kingdom CreateKingdom(this Clan clan, Settlement capital, TextObject intro)
		{
			string kingdomId = clan.GetKingdomId();
			var kingdom = Kingdom.All.SingleOrDefault(x => x.StringId == kingdomId);

			if (kingdom == null)
			{
				// create a rebel kingdom and set its name
				kingdom = MBObjectManager.Instance.CreateObject<Kingdom>(kingdomId);
				TextObject informalNameText = new TextObject("{=72pbZgQL}{CLAN_NAME}", null);
				informalNameText.SetTextVariable("CLAN_NAME", clan.Name);
				clan.GetKingdomNameAndRulerTitle(out var kingdomNameText, out var kingdomRulerTitleText);
				intro.SetTextVariable("RebelKingdom", kingdomNameText);

				// set colors for a rebel kingdom and the ruler clan
				var (color1, color2) = (0u, 0u);
				if (!SeparatismConfig.Settings.KeepRebelBannerColors)
				{
					(color1, color2) = ColorExtensions.GetRebelKingdomColors();
					clan.Banner.ChangePrimaryColor(color1);
					clan.Banner.ChangeIconColors(color2);
					clan.Color = color1;
					clan.Color2 = color2;
				}
				else
				{
					(color1, color2) = clan.GetColors();
				}

				clan.UpdateHomeSettlement(capital);
				kingdom.InitializeKingdom(
					kingdomNameText,
					informalNameText,
					clan.Culture,
					clan.Banner,
					color1,
					color2,
					capital,
					intro,
					kingdomNameText,
					kingdomRulerTitleText);
				AccessTools.Property(typeof(Kingdom), "AlternativeColor").SetValue(kingdom, color1);
				AccessTools.Property(typeof(Kingdom), "AlternativeColor2").SetValue(kingdom, color2);
				AccessTools.Property(typeof(Kingdom), "LabelColor").SetValue(kingdom, clan.Kingdom?.LabelColor ?? clan.LabelColor);

				kingdom.RulingClan = clan;
				Campaign.Current.AddKingdom(kingdom);
			}
			
			return kingdom;
		}

		public static void ChangeKingdom(this Clan clan, Kingdom newKingdom, bool rebellion)
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

			clan.EndMercenaryService(true);
			if (oldKingdom != null)
			{
				clan.ClanLeaveKingdom(false);
			}
			if (newKingdom != null)
			{
				clan.Kingdom = newKingdom;
				NotifyClanChangedKingdom(clan, oldKingdom, newKingdom, rebellion, true);

				if (rebellion)
				{
					foreach (Clan c in oldKingdom.Clans)
					{
						int relationChange = SeparatismConfig.Settings.RelationChangeRebelWithRulerVassals;
						if (c.Leader == oldKingdom.Leader)
						{
							relationChange = SeparatismConfig.Settings.RelationChangeRebelWithRuler;
						}
						else if (c.Leader.IsFriend(oldKingdom.Leader))
						{
							relationChange = SeparatismConfig.Settings.RelationChangeRebelWithRulerFriendVassals;
						}
						else if (c.Leader.IsEnemy(oldKingdom.Leader))
						{
							relationChange = SeparatismConfig.Settings.RelationChangeRebelWithRulerEnemyVassals;
						}

						if (relationChange != 0)
						{
							ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, c.Leader, relationChange, true);
						}
					}

					newKingdom.InheritsWarsFromKingdom(oldKingdom);
					DeclareWarAction.Apply(oldKingdom, newKingdom);
				}
				else
				{
					int relationChange = SeparatismConfig.Settings.RelationChangeUnitedRulers;
					if (relationChange != 0)
					{
						ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, newKingdom.Leader, relationChange, true);
					}
					if (oldKingdom != null)
					{
						newKingdom.InheritsWarsFromKingdom(oldKingdom);
					}
				}
			}
			if (oldKingdom != null && oldKingdom.Clans.Where(x => x.Leader.IsAlive).Count() == 0) // old kingdom is empty so we destroy it
			{
				if (newKingdom == null)
				{
					NotifyClanChangedKingdom(clan, oldKingdom, null, false, true);
				}
				Campaign.Current.RemoveEmptyKingdoms();
			}

			CheckIfPartyIconIsDirty(clan, oldKingdom);
		}

		public static bool IsReadyToGoAndEmpty(this Clan clan)
		{
			return clan.IsReadyToGo() && (clan.Kingdom == null || clan.Settlements.Count() == 0);
		}

		public static bool IsReadyToGoAndNotEmpty(this Clan clan)
		{
			return clan.IsReadyToGo() && clan.Kingdom != null && clan.Settlements.Count() > 0;
		}

		public static bool IsReadyToGo(this Clan clan)
		{
			return clan.IsReady() && clan.Kingdom?.RulingClan != clan;
		}

		public static bool IsReady(this Clan clan)
		{
			return
				clan != Clan.PlayerClan &&
				clan.Leader != null &&
				clan.Leader.IsAlive &&
				!clan.Leader.IsPrisoner &&
				!clan.IsUnderMercenaryService &&
				!clan.IsClanTypeMercenary &&
				!clan.IsMinorFaction &&
				!clan.IsBanditFaction &&
				!clan.IsMafia &&
				!clan.IsNomad &&
				!clan.IsOutlaw &&
				!clan.IsRebelClan &&
				!clan.IsSect &&
				clan.StringId != "test_clan" &&
				!clan.Fiefs.Any(x => x.IsUnderSiege) &&
				!clan.WarPartyComponents.Any(x => x.MobileParty.MapEvent != null);
		}

		private static void NotifyClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, bool byRebellion, bool showNotification = true)
		{
			AccessTools.Method(CampaignEventDispatcher.Instance.GetType(), "OnClanChangedKingdom")
				.Invoke(CampaignEventDispatcher.Instance, new object[]
				{
					clan,
					oldKingdom,
					newKingdom,
					byRebellion
						? ChangeKingdomAction.ChangeKingdomActionDetail.LeaveWithRebellion
						: newKingdom != null
							? ChangeKingdomAction.ChangeKingdomActionDetail.JoinKingdom
							: ChangeKingdomAction.ChangeKingdomActionDetail.LeaveKingdom,
					showNotification
				});
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
