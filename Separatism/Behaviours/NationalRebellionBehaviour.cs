using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using Common;

namespace Separatism.Behaviours
{
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
			if (!SeparatismConfig.Settings.NationalRebellionsEnabled)
			{
				return;
			}
			
			var readyClans = Clan.All
				.Where(c => c.Culture != c.Kingdom?.Culture && c.Settlements.Any(s => s.Culture == c.Culture))
				.GroupBy(c => new { Kingdom = c.Kingdom, Culture = c.Culture })
				.OrderByDescending(x => x.Count());
			foreach (var group in readyClans)
			{
				var rebelRightNow = SeparatismConfig.Settings.DailyNationalRebellionChance >= 1 ||
					(MBRandom.RandomFloat <= SeparatismConfig.Settings.DailyNationalRebellionChance);

				if (group.Count() >= SeparatismConfig.Settings.MinimalRequiredNumberOfNativeLords && rebelRightNow)
				{
					var rebelKingdom = GoRebelKingdom(group.OrderByDescending(c => c.Tier).ThenByDescending(c => c.Renown));
					var textObject = new TextObject("{=Separatism_National_Rebel}{Culture} people of {Kingdom} leading by their native lords have rised a rebellion to fight for their independence and found the {RebelKingdom}.", null);
					textObject.SetTextVariable("Culture", group.Key.Culture.Name);
					textObject.SetTextVariable("Kingdom", group.Key.Kingdom.Name);
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
			TextObject kingdomIntroText = new TextObject("{=Separatism_Kingdom_Intro_National}{RebelKingdom} was found in {Year} when {Culture} people of {Kingdom} have made a declaration of independence.", null);
			kingdomIntroText.SetTextVariable("Year", CampaignTime.Now.GetYear);
			kingdomIntroText.SetTextVariable("Culture", ruler.Culture.Name);
			kingdomIntroText.SetTextVariable("Kingdom", ruler.Kingdom.Name);
			var capital = ruler.Settlements.OrderByDescending(x => x.Prosperity).First();
			var kingdom = ruler.CreateKingdom(capital, kingdomIntroText);
			// keep policies from the old clan kingdom
			foreach (var policy in ruler.Kingdom.ActivePolicies)
			{
				kingdom.AddPolicy(policy);
			}
			// move all clans into the new kingdom
			foreach (var clan in clans)
			{
				clan.ChangeKingdom(kingdom, true);
			}
			// make relation changes for participating clans
			int relationChange = SeparatismConfig.Settings.RelationChangeNationalRebellionClans;
			if (relationChange != 0)
			{
				Queue<Clan> clanQueue = new Queue<Clan>(clans);
				while (clanQueue.Count > 1)
				{
					var c1 = clanQueue.Dequeue();
					foreach (var c2 in clanQueue)
					{
						ChangeRelationAction.ApplyRelationChangeBetweenHeroes(c1.Leader, c2.Leader, relationChange, true);
					}
				}
			}

			return kingdom;
		}
	}
}
