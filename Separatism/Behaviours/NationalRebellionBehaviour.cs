using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
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
				.GroupBy(c => c.Kingdom)
				.OrderByDescending(x => x.Count());
			foreach (var group in readyClans)
			{
				var rebelRightNow = SeparatismConfig.Settings.DailyNationalRebellionChance >= 1 ||
					(MBRandom.RandomFloat <= SeparatismConfig.Settings.DailyNationalRebellionChance);

				if (group.Count() >= SeparatismConfig.Settings.MinimalRequiredNumberOfNativeLords && rebelRightNow)
				{
					var rebelKingdom = GoRebelKingdom(group.OrderByDescending(c => c.Tier).ThenByDescending(c => c.Renown));
					var textObject = new TextObject("{=Separatism_National_Rebel}{Culture} people of {Kingdom} leading by their native lords have rised a rebellion to fight for their independence and found the {RebelKingdom}.", null);
					textObject.SetTextVariable("Culture", group.First().Culture.Name);
					textObject.SetTextVariable("Kingdom", group.Key.Name);
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
			var kingdom = ruler.CreateKingdom(kingdomIntroText);
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

			return kingdom;
		}
	}
}
