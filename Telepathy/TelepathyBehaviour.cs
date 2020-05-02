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
		public static bool CalledToTalk(Hero hero)
		{
			return heroesToTalk.Contains(hero);
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
				if (hero.CanTalkTo())
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
