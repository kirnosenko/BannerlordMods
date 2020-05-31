using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using StoryMode;
using HarmonyLib;
using Common;

namespace Separatism.Behaviours
{
	public class UtilityBehaviour : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
			CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
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
	}
}
