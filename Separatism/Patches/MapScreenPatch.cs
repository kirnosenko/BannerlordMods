using HarmonyLib;
using SandBox.View.Map;

namespace Separatism.Patches
{
	[HarmonyPatch(typeof(MapScreen), "OnEscapeMenuToggled")]
	public static class MapScreenPatch
	{
		public static void Postfix(bool isOpened = false)
		{
			if (!isOpened)
			{
				SeparatismConfig.Refresh();
			}
		}
	}
}
