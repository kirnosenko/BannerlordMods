using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using Helpers;
using HarmonyLib;

namespace Telepathy
{
	public class TelepathyBehaviour : CampaignBehaviorBase
	{
		abstract class Call
		{
			public Call(Hero hero)
			{
				Hero = hero;
				Ready = false;
			}

			public abstract void HourlyTick();
			
			public Hero Hero { get; protected set; }
			public bool Ready { get; protected set; }
		}

		class TelepathyCall : Call
		{
			public TelepathyCall(Hero hero)
				: base(hero)
			{
			}

			public override void HourlyTick()
			{
				Ready = true;
			}
		}

		class PigeonPostCall : Call
		{
			private const float SpeedPerHour = 30;
			private Vec2 position;
			private bool answer;

			public PigeonPostCall(Hero hero)
				:base(hero)
			{
				position = GetHeroPosition(Hero.MainHero);
				answer = false;
			}

			public override void HourlyTick()
			{
				if (Ready) return;

				var distanceToCover = SpeedPerHour;
				Vec2 diff = Vec2.Zero;
				
				if (!answer)
				{
					var targetPosition = GetHeroPosition(Hero);
					diff = targetPosition - position;
					if (diff.Length <= distanceToCover)
					{
						position = targetPosition;
						distanceToCover -= diff.Length;
						answer = true;
					}
				}
				if (answer)
				{
					var targetPosition = GetHeroPosition(Hero.MainHero);
					diff = targetPosition - position;
					if (diff.Length <= distanceToCover)
					{
						position = targetPosition;
						Ready = true;
						return;
					}
				}

				position += diff.Normalized() * distanceToCover;
			}

			private Vec2 GetHeroPosition(Hero hero)
			{
				return hero.GetMapPoint()?.Position2D ?? hero.HomeSettlement?.Position2D ?? Vec2.Zero;
			}
		}

		private static LinkedList<Call> calls = new LinkedList<Call>();
		private static PlayerEncounter meetingEncounter = null;
		private static Hero meetingHero = null;
		private static PlayerEncounter keepEncounter = null;
		private static Settlement keepSettlement = null;

		public static void CallToTalk(Hero hero)
		{
			if (!CalledToTalk(hero))
			{
				Call call = (!TelepathyConfig.Instance.PigeonPostMode)
					? (Call)new TelepathyCall(hero)
					: (Call)new PigeonPostCall(hero);
				calls.AddLast(call);
			}
		}
		public static bool CalledToTalk(Hero hero)
		{
			return calls.SingleOrDefault(x => x.Hero == hero) != null;
		}
		public static bool MeetingInProgress
		{
			get { return meetingEncounter != null; }
		}

		public override void RegisterEvents()
		{
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
			CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
			CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnSessionLaunched(CampaignGameStarter game)
		{
			game.BlockSentencesForMeeting(
				"main_option_discussions_1" // joining army
				//"hero_give_issue", // taking a quest
				//"hero_task_given", // discuss a quest
				//"caravan_create_conversation_1" // form a caravan
			);

			game.AddPlayerLine(
				"lord_talk_ask_something_2",
				"lord_talk_ask_something_2",
				"telepathy_tell_location",
				"Where are you?",
				new ConversationSentence.OnConditionDelegate(() => meetingEncounter != null),
				null, 101, null, null);
			game.AddDialogLine(
				"telepathy_tell_location",
				"telepathy_tell_location",
				"lord_talk_ask_something_again",
				"{LORD_LOCATION_ANSWER}",
				new ConversationSentence.OnConditionDelegate(() => {
					HeroHelper.SetLastSeenLocation(meetingHero, true);
					var answer = meetingHero.LastSeenInSettlement
						? $"I'm in {meetingHero.LastSeenPlace.EncyclopediaLinkWithName}."
						: $"I'm near {meetingHero.LastSeenPlace.EncyclopediaLinkWithName}.";
					MBTextManager.SetTextVariable("LORD_LOCATION_ANSWER", answer, false);
					return true;
				}),
				null, 100, null);
			game.AddPlayerLine(
				"lord_talk_ask_something_2",
				"lord_talk_ask_something_2",
				"telepathy_tell_objective",
				"What are you doing?",
				new ConversationSentence.OnConditionDelegate(() => meetingEncounter != null),
				null, 101, null, null);
			game.AddDialogLine(
				"telepathy_tell_objective",
				"telepathy_tell_objective",
				"lord_talk_ask_something_again",
				"{LORD_OBJECTIVE_ANSWER}",
				new ConversationSentence.OnConditionDelegate(() => {
					string answer = meetingHero.PartyBelongedTo == null
						? "Nothing actually."
						: CampaignUIHelper.GetMobilePartyBehaviorText(meetingHero.PartyBelongedTo);
					MBTextManager.SetTextVariable("LORD_OBJECTIVE_ANSWER", answer, false);
					return true;
				}),
				null, 100, null);
		}

		private void OnGameLoaded(CampaignGameStarter game)
		{
			calls.Clear();
		}

		private void OnHourlyTick()
		{
			foreach (var c in calls)
			{
				c.HourlyTick();
			}

			if (Hero.MainHero.IsOccupiedByAnEvent() || Hero.MainHero.IsPrisoner)
			{
				return;
			}
			var call = calls.FirstOrDefault(x => x.Ready);
			if (call != null && call.Hero.CanTalkTo())
			{
				calls.Remove(call);
				if (call.Hero.IsOccupiedByAnEvent())
				{
					calls.AddLast(call);
				}
				else
				{
					StartMeeting(call.Hero);
				}
			}
		}

		private void OnConversationEnded(CharacterObject character)
		{
			if (meetingEncounter != null)
			{
				PlayerEncounter.Finish(false);
				meetingEncounter = null;
				meetingHero = null;
				AccessTools.Property(typeof(Campaign), "PlayerEncounter").SetValue(Campaign.Current, keepEncounter);
				keepEncounter = null;
				Hero.MainHero.PartyBelongedTo.CurrentSettlement = keepSettlement;
				keepSettlement = null;
			}
		}

		private void StartMeeting(Hero hero)
		{
			var heroParty = hero.PartyBelongedTo?.Party;
			var player = Hero.MainHero;
			var playerParty = player.PartyBelongedTo?.Party;

			if (!hero.IsWanderer)
			{
				keepEncounter = PlayerEncounter.Current;
				keepSettlement = player.PartyBelongedTo.CurrentSettlement;
				if (heroParty == null && hero.CurrentSettlement != null)
				{
					player.PartyBelongedTo.CurrentSettlement = hero.CurrentSettlement;
				}
				PlayerEncounter.Start();
				PlayerEncounter.Current.SetupFields(playerParty, heroParty ?? playerParty);
				meetingEncounter = PlayerEncounter.Current;
				meetingHero = hero;

				Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
				Campaign.Current.CurrentConversationContext = ConversationContext.Default;
				AccessTools.Field(typeof(PlayerEncounter), "_mapEventState").SetValue(PlayerEncounter.Current, PlayerEncounterState.Begin);
				AccessTools.Field(typeof(PlayerEncounter), "_stateHandled").SetValue(PlayerEncounter.Current, true);
				AccessTools.Field(typeof(PlayerEncounter), "_meetingDone").SetValue(PlayerEncounter.Current, true);
			}

			ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, playerParty);
			ConversationCharacterData conversationPartnerData = new ConversationCharacterData(hero.CharacterObject, heroParty);
			CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData);
		}
	}
}
