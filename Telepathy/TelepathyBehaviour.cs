using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace Telepathy
{
	public class TelepathyBehaviour : CampaignBehaviorBase
	{
		private static Queue<Hero> heroesToTalk = new Queue<Hero>();
		private static PlayerEncounter encounter = null;

		public static void CallToTalk(Hero hero)
		{
			if (!heroesToTalk.Contains(hero))
			{
				heroesToTalk.Enqueue(hero);
			}
		}

		public override void RegisterEvents()
		{
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
			CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
			CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnGameLoaded(CampaignGameStarter game)
		{
			heroesToTalk.Clear();
		}

		private void OnHourlyTick()
		{
			if (Hero.MainHero.IsOccupiedByAnEvent())
			{
				return;
			}
			if (heroesToTalk.Count > 0)
			{
				var hero = heroesToTalk.Dequeue();
				if (hero.IsAlive)
				{
					if (hero.IsOccupiedByAnEvent())
					{
						heroesToTalk.Enqueue(hero);
					}
					else
					{
						StartMeeting(hero);
					}
				}
			}
		}

		private void OnTick(float time)
		{
			if (encounter != null)
			{
				PlayerEncounter.Finish(true);
				encounter = null;
			}
		}

		private void StartMeeting(Hero hero)
		{
			if (PlayerEncounter.Current != null)
			{
				PlayerEncounter.Finish(false);
			}
			PlayerEncounter.Start();
			PlayerEncounter.Current.SetupFields(CharacterObject.PlayerCharacter.HeroObject.PartyBelongedTo.Party, CharacterObject.PlayerCharacter.HeroObject.PartyBelongedTo.Party);
			encounter = PlayerEncounter.Current;

			Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
			Campaign.Current.CurrentConversationContext = ConversationContext.Default;
			AccessTools.Field(typeof(PlayerEncounter), "_mapEventState").SetValue(PlayerEncounter.Current, PlayerEncounterState.Begin);
			AccessTools.Field(typeof(PlayerEncounter), "_stateHandled").SetValue(PlayerEncounter.Current, true);
			AccessTools.Field(typeof(PlayerEncounter), "_meetingDone").SetValue(PlayerEncounter.Current, true);

			ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter);
			ConversationCharacterData conversationPartnerData = new ConversationCharacterData(hero.CharacterObject);
			CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData, "", "");
		}
	}
}
