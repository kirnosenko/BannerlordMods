using System;
using System.Collections.Generic;
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
{/*
	public class NationalRebellionBehaviour : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnDailyTick()
		{
			var readyClans = Clan.All
				.Where(c => c.Culture != c.Kingdom?.Culture && c.Settlements.Any(s => s.Culture == c.Culture))
				.GroupBy(c => c.Kingdom)
				.OrderByDescending(x => x.Count());
			foreach (var group in readyClans)
			{
				if (group.Count() >= 2)
				{
					var rebelKingdom = GoRebelKingdom(group.OrderByDescending(c => c.Tier).ThenByDescending(c => c.Renown));
					var textObject = new TextObject("{=Separatism_Anarchy_Rebel}{Culture} people of {Kingdom} have rised a rebellion to fight for their independence and found the {RebelKingdom}.", null);
					textObject.SetTextVariable("Settlement", settlement.Name);
					textObject.SetTextVariable("Kingdom", clan.Kingdom.Name);
					textObject.SetTextVariable("Ruler", newRulerClan.Leader.Name);
					textObject.SetTextVariable("RebelKingdom", rebelKingdom.Name);
					GameLog.Warn(textObject.ToString());
					return;
				}
			}
		}

		private Kingdom GoRebelKingdom(IEnumerable<Clan> clans)
		{
			var ruler = clans.First();
			// create a new kingdom for the clan
			TextObject kingdomIntroText = new TextObject("{=Separatism_Kingdom_Intro}{RebelKingdom} was found as a result of {ClanName} rebellion against {Ruler} ruler of {Kingdom}.", null);
			kingdomIntroText.SetTextVariable("ClanName", clan.Name);
			kingdomIntroText.SetTextVariable("Ruler", clan.Kingdom.Ruler.Name);
			kingdomIntroText.SetTextVariable("Kingdom", clan.Kingdom.Name);
			var kingdom = clan.CreateKingdom(kingdomIntroText);
			// keep policies from the old clan kingdom
			foreach (var policy in clan.Kingdom.ActivePolicies)
			{
				kingdom.AddPolicy(policy);
			}
			// move the clan into its new kingdom
			clan.ChangeKingdom(kingdom, true);

			return kingdom;
		}
	}*/
}
