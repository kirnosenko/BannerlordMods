using System;
using System.Collections.Generic;
using System.Xml;

namespace Common
{
	public static class OptionsExtension
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

	public abstract class ModConfig
	{
		protected static IReadOnlyDictionary<string, string> LoadOptions(string path)
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

			return options;
		}
	}
}
