using System.Collections.Generic;
using Common;

namespace Telepathy
{
	public class TelepathyConfig : ModConfig
	{
		public static TelepathyConfig Instance;

		private TelepathyConfig(IReadOnlyDictionary<string, string> options)
		{
			PigeonPostMode = options.LoadBool(nameof(PigeonPostMode), false);
		}

		public bool PigeonPostMode { get; private set; }
		
		public static void Load(string path)
		{
			var options = LoadOptions(path);
			Instance = new TelepathyConfig(options);
		}
	}
}
