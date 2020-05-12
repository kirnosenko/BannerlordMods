using System.Collections.Generic;
using Common;

namespace Separatism
{
	public class SeparatismConfig : ModConfig
	{
		public static SeparatismConfig Instance;

		private SeparatismConfig(IReadOnlyDictionary<string, string> options)
		{
			AverageAmountOfKingdomFiefsIsEnoughToRebel = options.LoadBool(nameof(AverageAmountOfKingdomFiefsIsEnoughToRebel), true);
			MinimalAmountOfKingdomFiefsToRebel = options.LoadInt(nameof(MinimalAmountOfKingdomFiefsToRebel), 3, 0, 100);
			DailyChanceToRebelWhenHaveAReason = options.LoadInt(nameof(DailyChanceToRebelWhenHaveAReason), 100, 0, 100);
			KeepEmptyKingdoms = options.LoadBool(nameof(KeepEmptyKingdoms), false);
			KeepOriginalKindomWars = options.LoadBool(nameof(KeepOriginalKindomWars), false);
			KeepRebelBannerColors = options.LoadBool(nameof(KeepRebelBannerColors), false);
			OneColorForAllRebels = options.LoadBool(nameof(OneColorForAllRebels), false);
		}

		public bool AverageAmountOfKingdomFiefsIsEnoughToRebel { get; private set; }
		public int MinimalAmountOfKingdomFiefsToRebel { get; private set; }
		public int DailyChanceToRebelWhenHaveAReason { get; private set; }
		public bool KeepEmptyKingdoms { get; private set; }
		public bool KeepOriginalKindomWars { get; private set; }
		public bool KeepRebelBannerColors { get; private set; }
		public bool OneColorForAllRebels { get; private set; }

		public static void Load(string path)
		{
			var options = LoadOptions(path);
			Instance = new SeparatismConfig(options);
		}
	}
}
