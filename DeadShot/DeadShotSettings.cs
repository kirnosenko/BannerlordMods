using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using MBOptionScreen.Attributes.v2;
using MBOptionScreen.Settings;

namespace DeadShot
{
	public class DeadShotSettings : AttributeSettings<DeadShotSettings>
	{
		private static string modVersion;
		private static string modName;
		private static string modId;
		
		static DeadShotSettings()
		{
			modVersion = ModuleInfo.GetModules().SingleOrDefault(x => x.Name == nameof(DeadShot))?.Version.ToString() ?? string.Empty;
			modName = $"{new TextObject($"{nameof(DeadShot)} Mod").ToString()} {modVersion}";
			modId = $"{nameof(DeadShot)}_{modVersion}";
		}
		public DeadShotSettings()
		{
			Id = modId;
		}

		public override string ModName => modName;

		public override string ModuleFolderName => nameof(DeadShot);

		public override string Id { get; set; }

		[SettingPropertyFloatingInteger(displayName: "Slow Motion Factor", minValue: 0.1f, maxValue: 1, HintText = "Set low value to make the effect stronger.", Order = 0, RequireRestart = false)]
		public float SlowMotionFactor { get; set; } = 0.4f;

		[SettingPropertyFloatingInteger(displayName: "Activation Probability Per Shot", minValue: 0, maxValue: 1, HintText = "1 - activete for every shot. 0 - never.", Order = 1, RequireRestart = false)]
		public float ActivationProbabilityPerShot { get; set; } = 1.0f;

		[SettingPropertyBool(displayName: "Activate With Zoom Only", HintText = "Effect will not be triggered if zoom is not used.", Order = 2, RequireRestart = false)]
		public bool ActivateWithZoomOnly { get; set; } = false;
	}
}
