using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Localization;
using StoryMode;
using HarmonyLib;
using Common;
using Helpers;

namespace Separatism.Behaviours
{
	public class UtilityBehaviour : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
			CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnGameStarted);
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
			CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnSessionLaunched(CampaignGameStarter game)
		{
			game.AddPlayerLine(
				"player_is_requesting_fallen_to_join",
				"lord_talk_speak_diplomacy_2",
				"persuasion_leave_faction_npc",
				"{=Separatism_Clan_Recruit}{FIRST_NAME}, I have heard you are in search of a new sovereign...",
				new ConversationSentence.OnConditionDelegate(
					conversation_player_is_asking_to_recruit_fallen_on_condition), null, 100, null, null);
		}

		private bool conversation_player_is_asking_to_recruit_fallen_on_condition()
		{
			if (Hero.MainHero.Clan.Kingdom != null &&
				Hero.MainHero == Hero.MainHero.Clan.Kingdom.Ruler() &&
				Hero.OneToOneConversationHero != null &&
				Hero.OneToOneConversationHero.Clan.Kingdom == null &&
				!Hero.OneToOneConversationHero.Clan.IsMinorFaction &&
				Hero.OneToOneConversationHero == Hero.OneToOneConversationHero.MapFaction.Leader &&
				!FactionManager.IsAtWarAgainstFaction(Hero.OneToOneConversationHero.MapFaction, Hero.MainHero.MapFaction))
			{
				Hero.OneToOneConversationHero.SetTextVariables();
				return true;
			}
			return false;
		}

		private void OnGameStarted(CampaignGameStarter starter)
		{
			if (!SeparatismConfig.Settings.ChaosStartEnabled)
			{
				return;
			}

			var kingdoms = Kingdom.All.ToArray();
			foreach (var oldKingdom in kingdoms)
			{
				var clans = oldKingdom.Clans.Where(c => c.IsReadyToGoAndNotEmpty()).ToArray();
				foreach (var clan in clans)
				{
					// create a new kingdom for the clan
					TextObject kingdomIntroText = new TextObject("{=Separatism_Kingdom_Intro_Chaos}{RebelKingdom} was found in {Year} as a result of BIG separation of Calradia.", null);
					kingdomIntroText.SetTextVariable("Year", CampaignTime.Now.GetYear);
					var capital = clan.Settlements
						.Where(x => x.Town != null)
						.OrderByDescending(x => x.Town.Prosperity)
						.First();
					var kingdom = clan.CreateKingdom(capital, kingdomIntroText);
					// keep policies from the old clan kingdom
					foreach (var policy in clan.Kingdom.ActivePolicies)
					{
						kingdom.AddPolicy(policy);
					}
					// move the clan into its new kingdom
					clan.ClanLeaveKingdom(false);
					clan.Kingdom = kingdom;
				}
			}

			kingdoms = Kingdom.All.ToArray();
			foreach (var kingdom in kingdoms)
			{
				var closeKingdoms = kingdom.RulingClan.CloseKingdoms().Where(k => k != kingdom).ToArray();
				var wars = FactionHelper.GetEnemyKingdoms(kingdom).Count();
				
				foreach (var closeKingdom in closeKingdoms)
				{
					if (wars < SeparatismConfig.Settings.MinimalNumberOfWarsPerChaosKindom)
					{
						DeclareWarAction.ApplyByDefault(closeKingdom, kingdom);
						wars++;
					}
					else
					{
						break;
					}
				}
			}
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
