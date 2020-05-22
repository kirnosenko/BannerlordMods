using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using HarmonyLib;

namespace Telepathy
{
	public static class Extensions
	{
		public static bool CanTalkTo(this Hero hero)
		{
			return (hero.IsAlive && hero != Hero.MainHero) && 
				(!TelepathyConfig.Instance.PigeonPostMode || hero.HasMet);
		}

		public static ConversationSentence GetSentence(this CampaignGameStarter gameInitializer, string id)
		{
			var conversationManager = AccessTools.Field(typeof(CampaignGameStarter), "_conversationManager")
				.GetValue(gameInitializer) as ConversationManager;
			var sentences = AccessTools.Field(typeof(ConversationManager), "_sentences")
				.GetValue(conversationManager) as List<ConversationSentence>;
			return sentences.SingleOrDefault(x => x.Id == id);
		}

		public static void BlockSentencesForMeeting(this CampaignGameStarter gameInitializer, params string[] sentenceIds)
		{
			foreach (var sentenceId in sentenceIds)
			{
				var sentence = gameInitializer.GetSentence(sentenceId);
				if (sentence != null)
				{
					var condition = sentence.OnCondition;
					sentence.OnCondition = () =>
					{
						return !TelepathyBehaviour.MeetingInProgress && (condition == null || condition());
					};
				}
			}
		}
	}
}
