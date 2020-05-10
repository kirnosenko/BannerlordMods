using HarmonyLib;
using TaleWorlds.Core;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(GameManagerBase), "Initialize")]
	public class GameManagerBasePatch
	{
		private static void Postfix()
		{
			HeroCollection.Clear();
		}
	}
}
