using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Localization;

namespace Separatism.Patches
{
	[HarmonyPatch(typeof(LordDefectionCampaignBehavior), "AddLordDefectionPersuasionOptions")]
	public static class LordDefectionCampaignBehaviorPatch
	{
		public static void Postfix(CampaignGameStarter starter)
		{
			starter.AddPlayerLine(
				"player_is_requesting_fallen_to_join",
				"lord_talk_speak_diplomacy_2",
				"persuasion_leave_faction_npc",
				"{=Separatism_Clan_Recruit}{FIRST_NAME}, I have heard you are in search of a new sovereign...",
				new ConversationSentence.OnConditionDelegate(
					conversation_player_is_asking_to_recruit_fallen_on_condition), null, 100, null, null);
		}

		public static bool conversation_player_is_asking_to_recruit_fallen_on_condition()
		{
			if (Hero.MainHero.Clan.Kingdom != null &&
				Hero.MainHero == Hero.MainHero.Clan.Kingdom.Ruler &&
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
	}
}
