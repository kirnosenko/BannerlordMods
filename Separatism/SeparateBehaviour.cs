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
	public class SeparateBehaviour : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
			CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
			CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnGameLoaded(CampaignGameStarter game)
		{
			var kingdoms = new Kingdom[] {
				StoryModeData.NorthernEmpireKingdom,
				StoryModeData.WesternEmpireKingdom,
				StoryModeData.SouthernEmpireKingdom,
				StoryModeData.SturgiaKingdom,
				StoryModeData.AseraiKingdom,
				StoryModeData.VlandiaKingdom,
				StoryModeData.BattaniaKingdom,
				StoryModeData.KhuzaitKingdom,
			};
			Campaign.Current.RemoveEmptyKingdoms();
			Kingdom.All.Do(k => k.SetKingdomText());
		}

		private void OnDailyTick()
		{
			Campaign.Current.RemoveEmptyKingdoms(kingdom =>
			{
				var textObject = new TextObject("{=Separatism_Kingdom_Destroyed}The {Kingdom} has been destroyed and the stories about it will be lost in time.", null);
				textObject.SetTextVariable("Kingdom", kingdom.Name);
				GameLog.Warn(textObject.ToString());
			});
		}

		private void OnDailyTickClan(Clan clan)
		{
			var kingdom = clan.Kingdom;
			if (kingdom == null
				|| clan == Clan.PlayerClan
				|| clan.IsUnderMercenaryService
				|| !clan.Leader.IsAlive
				|| clan.Leader.IsPrisoner
				|| clan.Fortifications.Any(x => x.IsUnderSiege)
				|| clan.Parties.Any(x => x.MapEvent != null))
			{
				return;
			}
			var ruler = kingdom.Ruler;

			if (clan.Leader != ruler)
			{
				var hasReason = !clan.Leader.HasGoodRelationWith(ruler);
				var kingdomFiefs = kingdom.Settlements.Sum(x => x.IsTown ? 2 : x.IsCastle ? 1 : 0);
				var kingdomClans = kingdom.Clans.Count(x => !x.IsUnderMercenaryService);
				var clanFiefs = clan.Settlements.Sum(x => x.IsTown ? 2 : x.IsCastle ? 1 : 0);
				var hasEnoughFiefs = (kingdomFiefs > 0 &&
					((SeparatismConfig.Settings.AverageAmountOfKingdomFiefsIsEnoughToRebel && clanFiefs >= (float)kingdomFiefs / kingdomClans) ||
					SeparatismConfig.Settings.MinimalAmountOfKingdomFiefsToRebel <= clanFiefs));
				var rebelRightNow = SeparatismConfig.Settings.DailyChanceToRebelWhenClanIsReady >= 1 ||
					(MBRandom.RandomFloat <= SeparatismConfig.Settings.DailyChanceToRebelWhenClanIsReady);

				if (hasReason && hasEnoughFiefs && rebelRightNow)
				{
					var rebelKingdom = GoRebelKingdom(clan);
					var textObject = new TextObject("{=Separatism_Clan_Rebel}The {ClanName} have broken from {Kingdom} to found the {RebelKingdom}.", null);
					textObject.SetTextVariable("ClanName", clan.Name);
					textObject.SetTextVariable("Kingdom", kingdom.Name);
					textObject.SetTextVariable("RebelKingdom", rebelKingdom.Name);
					GameLog.Warn(textObject.ToString());
				}
			}
			else
			{
				if (kingdom.Clans.Where(x => x.Leader.IsAlive).Count() == 1) // kingdom has the only one ruler clan
				{
					if (clan.Settlements.Count() == 0) // no any fiefs
					{
						ClanChangeKingdom(clan, null, false);
						var textObject = new TextObject("{=Separatism_Kingdom_Abandoned}The {Kingdom} has been destroyed and the {ClanName} are in search of a new sovereign.", null);
						textObject.SetTextVariable("Kingdom", kingdom.Name);
						textObject.SetTextVariable("ClanName", clan.Name);
						GameLog.Warn(textObject.ToString());
					}
					else // look for ally
					{
						var enemies = FactionManager.GetEnemyKingdoms(kingdom).ToArray();
						var potentialAllies = enemies
							.SelectMany(x => FactionManager.GetEnemyKingdoms(x))
							.Distinct()
							.Except(enemies)
							.Intersect(clan.CloseKingdoms())
							.Where(x => x != kingdom && x.Settlements.Count() > 0)
							.ToArray();
						foreach (var pa in potentialAllies)
						{
							if (kingdom.Leader.HasGoodRelationWith(pa.Leader) &&
								pa.Leader.Clan.Tier >= clan.Tier)
							{
								var commonEnemies = FactionManager.GetEnemyKingdoms(pa)
									.Intersect(enemies)
									.Where(x => x.Settlements.Count() > 0)
									.ToArray();
								foreach (var enemy in commonEnemies)
								{
									var kingdomDistance = enemy.FactionMidPoint.Distance(kingdom.FactionMidPoint);
									var paDistance = enemy.FactionMidPoint.Distance(pa.FactionMidPoint);
									var allianceDistance = kingdom.FactionMidPoint.Distance(pa.FactionMidPoint);

									if (allianceDistance <= Math.Sqrt(kingdomDistance * kingdomDistance + paDistance * paDistance) ||
										(kingdom.IsInsideKingdomTeritory(enemy) && pa.IsInsideKingdomTeritory(enemy)))
									{
										ClanChangeKingdom(clan, pa, false);
										var textObject = new TextObject("{=Separatism_Kingdom_Union}The {Kingdom} has joined to the {Ally} to fight against common enemy the {Enemy}.", null);
										textObject.SetTextVariable("Kingdom", kingdom.Name);
										textObject.SetTextVariable("Ally", pa.Name);
										textObject.SetTextVariable("Enemy", enemy.Name);
										GameLog.Warn(textObject.ToString());
										return;
									}
								}
							}
						}
					}
				}
			}
		}

		private (uint,uint) GetRebelKingdomColors()
		{
			var colors = BannerManager.ColorPalette.Values.Select(x => x.Color).Distinct().ToList();
			uint color1 = colors.Max();
			uint color2 = colors.Min();

			if (!SeparatismConfig.Settings.SameColorsForAllRebels)
			{
				colors = colors.Except(Kingdom.All.Select(x => x.Color)).ToList();
				color1 = TakeRandomColor(colors);
				color2 = color1;
				while (colors.Count > 0 && ColorDiff(color1, color2) < 0.3)
				{
					color2 = TakeRandomColor(colors);
				}
			}

			return (color1, color2);
		}

		private uint TakeRandomColor(List<uint> colors)
		{
			int index = MBRandom.RandomInt(colors.Count);
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

		private Kingdom GoRebelKingdom(Clan clan)
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
				TextObject kingdomIntroText = new TextObject("{=Separatism_Kingdom_Intro}{RebelKingdom} was found as a result of {ClanName} rebellion against {Ruler} ruler of {Kingdom}.", null);
				kingdomIntroText.SetTextVariable("RebelKingdom", kingdomNameText);
				kingdomIntroText.SetTextVariable("ClanName", clan.Name);
				kingdomIntroText.SetTextVariable("Ruler", clan.Kingdom.Ruler.Name);
				kingdomIntroText.SetTextVariable("Kingdom", clan.Kingdom.Name);

				// set colors for a rebel kingdom and the ruler clan
				var (color1, color2) = (0u, 0u);
				if (!SeparatismConfig.Settings.KeepRebelBannerColors)
				{
					(color1, color2) = GetRebelKingdomColors();
					clan.Banner.ChangePrimaryColor(color1);
					clan.Banner.ChangeIconColors(color2);
					clan.Color = color1;
					clan.Color2 = color2;
				}
				else
				{
					(color1, color2) = clan.GetColors();
				}

				kingdom.InitializeKingdom(kingdomNameText, informalNameText, clan.Culture, clan.Banner, color1, color2, clan.InitialPosition);
				AccessTools.Property(typeof(Kingdom), "EncyclopediaText").SetValue(kingdom, kingdomIntroText);
				AccessTools.Property(typeof(Kingdom), "EncyclopediaTitle").SetValue(kingdom, kingdomNameText);
				AccessTools.Property(typeof(Kingdom), "EncyclopediaRulerTitle").SetValue(kingdom, kingdomRulerTitleText);
				AccessTools.Property(typeof(Kingdom), "AlternativeColor").SetValue(kingdom, color1);
				AccessTools.Property(typeof(Kingdom), "AlternativeColor2").SetValue(kingdom, color2);
				AccessTools.Property(typeof(Kingdom), "LabelColor").SetValue(kingdom, clan.Kingdom.LabelColor);

				// apply policies from original kingdom
				foreach (var policy in clan.Kingdom.ActivePolicies)
				{
					kingdom.AddPolicy(policy);
				}

				kingdom.RulingClan = clan;
			}

			ClanChangeKingdom(clan, kingdom, true);
			Campaign.Current.AddKingdom(kingdom);

			return kingdom;
		}

		private void ClanChangeKingdom(Clan clan, Kingdom newKingdom, bool rebellion)
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
			clan.ClanLeaveKingdom(false);
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
					InheritWarsFromKingdom(oldKingdom, newKingdom);
				}
			}
			if (oldKingdom.Clans.Where(x => x.Leader.IsAlive).Count() == 0) // old kingdom is empty so we destroy it
			{
				if (newKingdom == null)
				{
					NotifyClanChangedKingdom(clan, oldKingdom, null, false, true);
				}
				Campaign.Current.RemoveEmptyKingdoms();
			}

			CheckIfPartyIconIsDirty(clan, oldKingdom);
		}

		private void InheritWarsFromKingdom(Kingdom src, Kingdom dest)
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
