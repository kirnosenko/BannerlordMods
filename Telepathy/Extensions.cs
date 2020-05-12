using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
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

		public static float Distance(this Vec2 value1, Vec2 value2)
		{
			float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			return (float)Math.Sqrt((v1 * v1) + (v2 * v2));
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
