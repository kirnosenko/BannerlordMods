using System;
using System.Collections.Generic;
using System.Xml;
using TaleWorlds.CampaignSystem;

namespace LifeIsShort
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
	}

	public class LifeIsShortConfig
	{
		public static LifeIsShortConfig Instance;

		private LifeIsShortConfig(IReadOnlyDictionary<string, string> options)
		{
			OneYearOfHeroLifeInDays = options.LoadInt(nameof(OneYearOfHeroLifeInDays), 10, 1, int.MaxValue);
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

			Instance = new LifeIsShortConfig(options);
		}
	}
}
