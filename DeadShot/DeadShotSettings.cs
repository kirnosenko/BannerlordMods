using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

namespace DeadShot
{
	public class DeadShotSettings : AttributeGlobalSettings<DeadShotSettings>
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

		public override string DisplayName => modName;

		public override string FolderName => nameof(DeadShot);

		public override string Id => modId;

		public override string FormatType => "json2";

		[SettingPropertyGroup("Slow Motion Effect", GroupOrder = 0)]
		[SettingPropertyFloatingInteger(displayName: "Slow Motion Factor", minValue: 0.1f, maxValue: 1, HintText = "Set low value to make the effect stronger.", Order = 0, RequireRestart = false)]
		public float SlowMotionFactor { get; set; } = 0.4f;

		[SettingPropertyGroup("Slow Motion Effect", GroupOrder = 0)]
		[SettingPropertyFloatingInteger(displayName: "Activation Probability Per Shot", minValue: 0, maxValue: 1, HintText = "1 - activete for every shot. 0 - never.", Order = 1, RequireRestart = false)]
		public float ActivationProbabilityPerShot { get; set; } = 1.0f;

		[SettingPropertyGroup("Slow Motion Effect", GroupOrder = 0)]
		[SettingPropertyBool(displayName: "Activate With Zoom Only", HintText = "Effect will not be triggered if zoom is not used.", Order = 2, RequireRestart = false)]
		public bool ActivateWithZoomOnly { get; set; } = false;
	}
}
