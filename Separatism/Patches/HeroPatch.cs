using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace Separatism.Patches
{
	[HarmonyPatch(typeof(Hero), "IsFriend")]
	public static class HeroIsFriendPatch
	{
		public static bool Prefix(Hero otherHero, Hero __instance, ref bool __result)
		{
			__result = CharacterRelationManager.GetHeroRelation(__instance, otherHero) > SeparatismSettings.Instance.FriendThreshold;
			return false;
		}
	}

	[HarmonyPatch(typeof(Hero), "IsEnemy")]
	public static class HeroIsEnemyPatch
	{
		public static bool Prefix(Hero otherHero, Hero __instance, ref bool __result)
		{
			__result = CharacterRelationManager.GetHeroRelation(__instance, otherHero) < SeparatismSettings.Instance.EnemyThreshold;
			return false;
		}
	}
}
