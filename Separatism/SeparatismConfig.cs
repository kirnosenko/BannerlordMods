using TaleWorlds.CampaignSystem.CampaignBehaviors;
using HarmonyLib;

namespace Separatism
{
	public static class SeparatismConfig
	{
		private static SeparatismSettings instance;

		public static SeparatismSettings Settings
		{
			get
			{
				if (instance == null)
				{
					Refresh();
				}
				return instance;
			}
		}
		public static void Refresh()
		{
			instance = SeparatismSettings.Instance;
		}
	}
}
