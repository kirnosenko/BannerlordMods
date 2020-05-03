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
		private SeparatismConfig(IReadOnlyDictionary<string, string> options)
		{
			MinimalKingdomFiefsPercentToRebel = options.LoadInt(nameof(MinimalKingdomFiefsPercentToRebel), 10, 0, 100);
			DailyChanceToRebelWhenHaveAReason = options.LoadInt(nameof(DailyChanceToRebelWhenHaveAReason), 100, 0, 100);
			KeepOriginalKindomWars = options.LoadBool(nameof(KeepOriginalKindomWars), false);
			KeepRebelBannerColors = options.LoadBool(nameof(KeepRebelBannerColors), false);
			OneColorForAllRebels = options.LoadBool(nameof(OneColorForAllRebels), false);
		}

		public int MinimalKingdomFiefsPercentToRebel { get; private set; }
		public int DailyChanceToRebelWhenHaveAReason { get; private set; }
		public bool KeepOriginalKindomWars { get; private set; }
		public bool KeepRebelBannerColors { get; private set; }
		public bool OneColorForAllRebels { get; private set; }

		public static SeparatismConfig Load(string path)
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

			return new SeparatismConfig(options);
		}
	}
}
