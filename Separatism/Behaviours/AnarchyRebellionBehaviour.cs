using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using StoryMode;
using HarmonyLib;
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
			var clanFiefs = clan.GetFiefsAmount();
			if (clanFiefs > 10)
			{
				var availableClans = Clan.All.ReadyToGo().ToArray();
				foreach (var settlement in clan.Settlements.Where(x => x.IsTown).OrderBy(x => x.Prosperity))
				{
					var newRulerClan = availableClans
						.Where(x => x.Culture == settlement.Culture)
						.OrderByDescending(x => x.TotalStrength)
						.FirstOrDefault();
					if (newRulerClan != null)
					{
						var rebelKingdom = GoRebelKingdom(newRulerClan, settlement);
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
		}

		private Kingdom GoRebelKingdom(Clan clan, Settlement settlement)
		{
			// create a new kingdom for the clan
			TextObject kingdomIntroText = new TextObject("{=Separatism_Kingdom_Intro_Anarchy}{RebelKingdom} was found as a result of anarchy in fiefs of the {ClanName}. People of {Settlement} have called {Ruler} on rulership.", null);
			kingdomIntroText.SetTextVariable("ClanName", settlement.OwnerClan.Name);
			kingdomIntroText.SetTextVariable("Settlement", settlement.Name);
			kingdomIntroText.SetTextVariable("Ruler", clan.Leader.Name);
			var kingdom = clan.CreateKingdom(kingdomIntroText);
			// keep policies from the old settlement kingdom
			foreach (var policy in settlement.OwnerClan.Kingdom.ActivePolicies)
			{
				kingdom.AddPolicy(policy);
			}
			// move the clan out of its current kingdom
			clan.ChangeKingdom(null, false);
			// change settlement ownership
			settlement.OwnerClan = clan;
			// move the clan into the new kingdom
			clan.ChangeKingdom(kingdom, false);

			return kingdom;
		}
	}
}
