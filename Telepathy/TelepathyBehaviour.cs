using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace Telepathy
{
	public class TelepathyBehaviour : CampaignBehaviorBase
	{
		private static Queue<Hero> heroesToTalk = new Queue<Hero>();

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
				if (hero.IsOccupiedByAnEvent())
				{
					heroesToTalk.Enqueue(hero);
				}

				ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter);
				ConversationCharacterData conversationPartnerData = new ConversationCharacterData(hero.CharacterObject);
				CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData, "", "");
			}
		}
	}
}
