using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using Common;

namespace LifeIsShort
{
	public class LifeIsShortConfig : ModConfig
	{
		public static LifeIsShortConfig Instance;

		private LifeIsShortConfig(IReadOnlyDictionary<string, string> options)
		{
			OneYearOfHeroLifeInDays = options.LoadInt(nameof(OneYearOfHeroLifeInDays), 5, 1, 365);
		}

		public int OneYearOfHeroLifeInDays { get; private set; }

		public float AgeMultiplier
		{
			get
			{
				return ((float)CampaignTime.DaysInYear / OneYearOfHeroLifeInDays);
			}
		}

		public static void Load(string path)
		{
			var options = LoadOptions(path);
			Instance = new LifeIsShortConfig(options);
		}
	}
}
