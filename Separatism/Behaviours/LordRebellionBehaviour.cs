using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using Common;

namespace Separatism.Behaviours
{
	public class LordRebellionBehaviour : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnDailyTickClan(Clan clan)
		{
			var kingdom = clan.Kingdom;
			if (kingdom == null
				|| clan == Clan.PlayerClan
				|| clan.IsUnderMercenaryService
				|| !clan.Leader.IsAlive
				|| clan.Leader.IsPrisoner
				|| clan.Fiefs.Any(x => x.IsUnderSiege)
				|| clan.WarPartyComponents.Any(x => x.MobileParty.MapEvent != null))
			{
				return;
			}
			var ruler = kingdom.Ruler;

			if (clan.Leader != ruler)
			{
				if (!SeparatismConfig.Settings.LordRebellionsEnabled)
				{
					return;
				}

				var hasReason = !clan.Leader.HasGoodRelationWith(ruler);
				var kingdomFiefs = kingdom.GetFiefsAmount();
				var kingdomClans = kingdom.Clans.Count(x => !x.IsUnderMercenaryService);
				var clanFiefs = clan.GetFiefsAmount();
				
				var hasEnoughFiefs = (kingdomFiefs > 0 &&
					((SeparatismConfig.Settings.AverageAmountOfKingdomFiefsIsEnoughToRebel && clanFiefs >= (float)kingdomFiefs / kingdomClans) ||
					SeparatismConfig.Settings.MinimalAmountOfKingdomFiefsToRebel <= clanFiefs));
				var rebelRightNow = SeparatismConfig.Settings.DailyLordRebellionChance >= 1 ||
					(MBRandom.RandomFloat <= SeparatismConfig.Settings.DailyLordRebellionChance);

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
						clan.ChangeKingdom(null, false);
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
									var kingdomDistance = enemy.FactionMidSettlement.Position2D.Distance(kingdom.FactionMidSettlement.Position2D);
									var paDistance = enemy.FactionMidSettlement.Position2D.Distance(pa.FactionMidSettlement.Position2D);
									var allianceDistance = kingdom.FactionMidSettlement.Position2D.Distance(pa.FactionMidSettlement.Position2D);

									if (allianceDistance <= Math.Sqrt(kingdomDistance * kingdomDistance + paDistance * paDistance) ||
										(kingdom.IsInsideKingdomTeritory(enemy) && pa.IsInsideKingdomTeritory(enemy)))
									{
										clan.ChangeKingdom(pa, false);
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
						// no ally kingdoms found, so look for friendly clans at least
						var allyClan = Clan.All.ReadyToGoAndEmpty().Where(c =>
							c.Tier <= clan.Tier && 
							c.Leader.HasGoodRelationWith(clan.Leader) &&
							(c.Kingdom == null || !c.Leader.HasGoodRelationWith(c.Kingdom.Ruler)))
							.OrderByDescending(c => c.TotalStrength)
							.FirstOrDefault();
						if (allyClan != null)
						{
							allyClan.ChangeKingdom(kingdom, false);
							int relationChange = SeparatismConfig.Settings.RelationChangeRulerWithSupporter;
							if (relationChange != 0)
							{
								ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, allyClan.Leader, relationChange, true);
							}
							var textObject = new TextObject("{=Separatism_Clan_Support}The {ClanName} have joined the {Kingdom} to support {Ruler}.", null);
							textObject.SetTextVariable("ClanName", allyClan.Name);
							textObject.SetTextVariable("Kingdom", kingdom.Name);
							textObject.SetTextVariable("Ruler", kingdom.Ruler.Name);
							GameLog.Warn(textObject.ToString());
						}
					}
				}
			}
		}

		private Kingdom GoRebelKingdom(Clan clan)
		{
			// create a new kingdom for the clan
			TextObject kingdomIntroText = new TextObject("{=Separatism_Kingdom_Intro}{RebelKingdom} was found in {Year} when the {ClanName} have rised a rebellion against {Ruler} ruler of {Kingdom}.", null);
			kingdomIntroText.SetTextVariable("Year", CampaignTime.Now.GetYear);
			kingdomIntroText.SetTextVariable("ClanName", clan.Name);
			kingdomIntroText.SetTextVariable("Ruler", clan.Kingdom.Ruler.Name);
			kingdomIntroText.SetTextVariable("Kingdom", clan.Kingdom.Name);
			var capital = clan.Settlements.OrderByDescending(x => x.Prosperity).First();
			var kingdom = clan.CreateKingdom(capital, kingdomIntroText);
			// keep policies from the old clan kingdom
			foreach (var policy in clan.Kingdom.ActivePolicies)
			{
				kingdom.AddPolicy(policy);
			}
			// move the clan into its new kingdom
			clan.ChangeKingdom(kingdom, true);

			return kingdom;
		}
	}
}
