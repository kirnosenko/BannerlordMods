using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using Common;

namespace Separatism.Behaviours
{
	public class AnarchyRebellionBehaviour : CampaignBehaviorBase
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
			var clanFiefsAmount = clan.GetFiefsAmount();
			if (clanFiefsAmount < 10) return;
			var anarchySettlements = clan.Settlements.Where(x => x.IsTown &&
				CampaignTime.Hours(x.LastVisitTimeOfOwner) + CampaignTime.Days(3) < CampaignTime.Now).ToArray();
			if (anarchySettlements.Length == 0) return;
			
			var availableClans = Clan.All.ReadyToRule().ToArray();
			foreach (var settlement in anarchySettlements.OrderByDescending(x => x.Position2D.Distance(clan.FactionMidPoint)))
			{
				var newRulerClan = availableClans
					.Where(x => x.Culture == settlement.Culture)
					.OrderByDescending(x => x.TotalStrength)
					.FirstOrDefault();
				if (newRulerClan != null)
				{
					var rebelSettlements = new List<Settlement>();
					rebelSettlements.Add(settlement);
					int bonusSettlements = newRulerClan.Tier > 4 ? 1 : 0;
					if (bonusSettlements > 0)
					{
						var neighborClanFiefs = new Queue<Settlement>(Settlement
							.FindSettlementsAroundPosition(settlement.Position2D, 50, x => x.OwnerClan == clan)
							.Where(x => x.IsCastle)
							.Except(rebelSettlements)
							.OrderBy(x => x.Position2D.Distance(settlement.Position2D)));
						while (bonusSettlements > 0 && neighborClanFiefs.Count > 0)
						{
							var nextFief = neighborClanFiefs.Dequeue();
							if (nextFief.Culture == settlement.Culture)
							{
								rebelSettlements.Add(nextFief);
								bonusSettlements--;
							}
						}
					}

					var rebelKingdom = GoRebelKingdom(newRulerClan, rebelSettlements);
					var textObject = new TextObject("{=Separatism_Anarchy_Rebel}People of {Settlement} have broken from {Kingdom} to call {Ruler} on rulership and found the {RebelKingdom}.", null);
					textObject.SetTextVariable("Settlement", settlement.Name);
					textObject.SetTextVariable("Kingdom", clan.Kingdom.Name);
					textObject.SetTextVariable("Ruler", newRulerClan.Leader.Name);
					textObject.SetTextVariable("RebelKingdom", rebelKingdom.Name);
					GameLog.Warn(textObject.ToString());
					return;
				}
			}
		}

		private Kingdom GoRebelKingdom(Clan clan, IEnumerable<Settlement> settlements)
		{
			var capital = settlements.First();
			var owner = capital.OwnerClan;
			// create a new kingdom for the clan
			TextObject kingdomIntroText = new TextObject("{=Separatism_Kingdom_Intro_Anarchy}{RebelKingdom} was found as a result of anarchy in fiefs of the {ClanName}. People of {Settlement} have called {Ruler} on rulership.", null);
			kingdomIntroText.SetTextVariable("ClanName", owner.Name);
			kingdomIntroText.SetTextVariable("Settlement", capital.Name);
			kingdomIntroText.SetTextVariable("Ruler", clan.Leader.Name);
			var kingdom = clan.CreateKingdom(kingdomIntroText);
			// keep policies from the old settlement kingdom
			foreach (var policy in owner.Kingdom.ActivePolicies)
			{
				kingdom.AddPolicy(policy);
			}
			// move the clan out of its current kingdom
			clan.ChangeKingdom(null, false);
			// change settlement ownership
			foreach (var s in settlements)
			{
				ChangeOwnerOfSettlementAction.ApplyByRevolt(clan.Leader, s);
			}
			// move the clan into the new kingdom
			clan.ChangeKingdom(kingdom, false);

			return kingdom;
		}
	}
}
