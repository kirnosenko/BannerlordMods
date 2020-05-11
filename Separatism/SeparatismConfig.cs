using System;
using System.Collections.Generic;
using System.Xml;

namespace Separatism
{
	static class OptionsExtension
	{
		public static int LoadInt(this IReadOnlyDictionary<string, string> options, string name, int defaultValue, int minValue, int maxValue)
		{
			if (options.TryGetValue(name, out var valueString)
				&& Int32.TryParse(valueString, out var value))
			{
				return Math.Min(Math.Max(value, minValue), maxValue);
			}

			return defaultValue;
		}

		public static bool LoadBool(this IReadOnlyDictionary<string, string> options, string name, bool defaultValue)
		{
			if (options.TryGetValue(name, out var valueString)
				&& bool.TryParse(valueString, out var value))
			{
				return value;
			}

			return defaultValue;
		}
	}

	public class SeparatismConfig
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
			var options = new Dictionary<string, string>();

			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(path);

				foreach (XmlNode node in xmlDocument.DocumentElement.ChildNodes)
				{
					if (node.NodeType != XmlNodeType.Comment)
					{
						options.Add(node.Name, node.InnerText);
					}
				}
			}
			catch
			{
			}

			Instance = new SeparatismConfig(options);
		}
	}
}
