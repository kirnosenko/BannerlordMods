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
			CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnGameLoaded(CampaignGameStarter game)
		{
			heroesToTalk.Clear();
		}

		private void OnDailyTickClan(Clan clan)
		{
			//if (!hero.HasMet) hero.HasMet = true;
			//Campaign.Current.ConversationManager.BeginConversation()
			Campaign.Current.CurrentConversationContext = ConversationContext.Default;
			ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, CharacterObject.PlayerCharacter.HeroObject.PartyBelongedTo.Party);
			ConversationCharacterData conversationPartnerData = new ConversationCharacterData(clan.Leader.CharacterObject, clan.Leader.PartyBelongedTo?.Party);
			CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData, "", "");
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

				//if (!hero.HasMet) hero.HasMet = true;
				Campaign.Current.CurrentConversationContext = ConversationContext.Default;
				ConversationCharacterData playerCharacterData = new ConversationCharacterData(CharacterObject.PlayerCharacter, CharacterObject.PlayerCharacter.HeroObject.PartyBelongedTo.Party);
				ConversationCharacterData conversationPartnerData = new ConversationCharacterData(hero.CharacterObject, hero.PartyBelongedTo?.Party);
				CampaignMission.OpenConversationMission(playerCharacterData, conversationPartnerData, "", "");
			}
		}
	}
}
