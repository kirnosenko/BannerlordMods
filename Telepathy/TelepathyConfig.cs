using System.Collections.Generic;
using System.Xml;

namespace Telepathy
{
	static class OptionsExtension
	{
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

	public class TelepathyConfig
	{
		public static TelepathyConfig Instance;

		private TelepathyConfig(IReadOnlyDictionary<string, string> options)
		{
			PigeonPostMode = options.LoadBool(nameof(PigeonPostMode), false);
		}

		public bool PigeonPostMode { get; private set; }
		
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

			Instance = new TelepathyConfig(options);
		}
	}
}
