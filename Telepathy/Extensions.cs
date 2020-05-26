using System;
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
			return (hero != Hero.MainHero) &&
				(hero.IsAlive || !TelepathySettings.Instance.PreventTalkingToDead) &&
				(hero.HasMet || !TelepathySettings.Instance.PreventTalkingToHeroesHaveNotMetBefore);
		}

		public static ConversationSentence GetSentence(this CampaignGameStarter gameInitializer, string id)
		{
			var conversationManager = AccessTools.Field(typeof(CampaignGameStarter), "_conversationManager")
				.GetValue(gameInitializer) as ConversationManager;
			var sentences = AccessTools.Field(typeof(ConversationManager), "_sentences")
				.GetValue(conversationManager) as List<ConversationSentence>;
			return sentences.SingleOrDefault(x => x.Id == id);
		}

		public static void BlockSentences(this CampaignGameStarter gameInitializer, Func<bool> condition, params string[] sentenceIds)
		{
			foreach (var sentenceId in sentenceIds)
			{
				var sentence = gameInitializer.GetSentence(sentenceId);
				if (sentence != null)
				{
					var sentenceCondition = sentence.OnCondition;
					sentence.OnCondition = () =>
					{
						return condition() && (sentenceCondition == null || sentenceCondition());
					};
				}
			}
		}
	}
}
