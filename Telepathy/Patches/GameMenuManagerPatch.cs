using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace Telepathy.Patches
{
	[HarmonyPatch(typeof(GameMenuManager), "ExitToLast")]
	public class GameMenuManagerPatch
	{
		public static bool Prefix()
		{
			if (Campaign.Current.CurrentMenuContext != null)
			{
				(Game.Current.GameStateManager.ActiveState as MapState)?.ExitMenuMode();
			}

			return false;
		}
	}
}
