using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
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
				position = Hero.MainHero.GetMapPoint().Position2D;
				answer = false;
			}

			public override void HourlyTick()
			{
				if (Ready) return;

				var distanceToCover = SpeedPerHour;
				Vec2 diff = Vec2.Zero;
				
				if (!answer)
				{
					var heroPosition = Hero.GetMapPoint().Position2D;
					diff = heroPosition - position;
					if (diff.Length <= distanceToCover)
					{
						position = heroPosition;
						distanceToCover -= diff.Length;
						answer = true;
					}
				}
				if (answer)
				{
					var mainHeroPosition = Hero.MainHero.GetMapPoint().Position2D;
					diff = mainHeroPosition - position;
					if (diff.Length <= distanceToCover)
					{
						position = mainHeroPosition;
						Ready = true;
						return;
					}
				}

				position += diff.Normalized() * distanceToCover;
			}
		}

		private static LinkedList<Call> calls = new LinkedList<Call>();
		private static PlayerEncounter encounter = null;

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

		public override void RegisterEvents()
		{
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
			CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
			CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
		}

		public override void SyncData(IDataStore dataStore)
		{
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

			if (Hero.MainHero.IsOccupiedByAnEvent())
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
			if (encounter != null)
			{
				PlayerEncounter.Finish(true);
				encounter = null;
			}
		}

		private void StartMeeting(Hero hero)
		{
			var heroParty = hero.PartyBelongedTo?.Party;
			var player = Hero.MainHero;
			var playerParty = player.PartyBelongedTo?.Party;

			if (!hero.IsWanderer)
			{
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.Finish(false);
				}
				PlayerEncounter.Start();
				PlayerEncounter.Current.SetupFields(playerParty, heroParty ?? playerParty);
				encounter = PlayerEncounter.Current;

				Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
				Campaign.Current.CurrentConversationContext = ConversationContext.Default;
				AccessTools.Field(typeof(PlayerEncounter), "_mapEventState").SetValue(PlayerEncounter.Current, PlayerEncounterState.Begin);
				AccessTools.Field(typeof(PlayerEncounter), "_stateHandled").SetValue(PlayerEncounter.Current, true);
				AccessTools.Field(typeof(PlayerEncounter), "_meetingDone").SetValue(PlayerEncounter.Current, true);
			}

			ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, playerParty);
			ConversationCharacterData conversationPartnerData = new ConversationCharacterData(hero.CharacterObject, heroParty);
			CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData, "", "");
		}
	}
}
