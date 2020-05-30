using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using StoryMode;
using Helpers;
using HarmonyLib;
using Common;

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

			StatisticsDataLogHelper.AddLog(StatisticsDataLogHelper.LogAction.ChangeKingdomAction, new object[]
			{
				clan,
				oldKingdom,
				newKingdom,
				newKingdom == null || rebellion
			});
			clan.IsUnderMercenaryService = false;
			if (oldKingdom != null)
			{
				clan.ClanLeaveKingdom(false);
			}
			if (newKingdom != null)
			{
				clan.ClanJoinFaction(newKingdom);
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

					InheritWarsFromKingdom(oldKingdom, newKingdom);
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
						InheritWarsFromKingdom(oldKingdom, newKingdom);
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

		private static void InheritWarsFromKingdom(Kingdom src, Kingdom dest)
		{
			if (SeparatismConfig.Settings.KeepOriginalKindomWars)
			{
				var oldKingdomEnemies = FactionManager.GetEnemyKingdoms(src).ToArray();
				foreach (var enemy in oldKingdomEnemies)
				{
					DeclareWarAction.Apply(enemy, dest);
				}
			}
		}

		private static void NotifyClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, bool byRebellion, bool showNotification = true)
		{
			AccessTools.Method(CampaignEventDispatcher.Instance.GetType(), "OnClanChangedKingdom")
				.Invoke(CampaignEventDispatcher.Instance, new object[] { clan, oldKingdom, newKingdom, byRebellion, showNotification });
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
