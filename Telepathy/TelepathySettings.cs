using System.Linq;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace Telepathy
{
	public class TelepathySettings : AttributeGlobalSettings<TelepathySettings>
	{
		private static string modVersion;
		private static string modName;
		private static string modId;

		static TelepathySettings()
		{
			modVersion = ModuleHelper.GetModules().SingleOrDefault(x => x.Name == nameof(Telepathy))?.Version.ToString() ?? string.Empty;
			modName = $"{new TextObject("{=Telepathy_Mod_Name}Telepathy Mod").ToString()} {modVersion}";
			modId = $"{nameof(Telepathy)}_{modVersion}";
		}

		public override string DisplayName => modName;

		public override string FolderName => nameof(Telepathy);

		public override string Id => modId;

		public override string FormatType => "json2";

		[SettingPropertyGroup("{=Telepathy_Settings_100}Distant conversation realism", GroupOrder = 0)]
		[SettingPropertyBool(displayName: "{=Telepathy_Settings_110}Prevent talking to dead", HintText = "{=Telepathy_Settings_111}Prevent distant conversation with heroes passed away.", Order = 0, RequireRestart = false)]
		public bool PreventTalkingToDead { get; set; } = true;

		[SettingPropertyGroup("{=Telepathy_Settings_100}Distant conversation realism", GroupOrder = 0)]
		[SettingPropertyBool(displayName: "{=Telepathy_Settings_120}Pigeon post mode", HintText = "{=Telepathy_Settings_121}Message delivery will takes some time depending on distance to hero you are going to talk to.", Order = 1, RequireRestart = false)]
		public bool PigeonPostMode { get; set; } = false;

		[SettingPropertyGroup("{=Telepathy_Settings_100}Distant conversation realism", GroupOrder = 0)]
		[SettingPropertyBool(displayName: "{=Telepathy_Settings_130}Prevent talking to heroes have not met before", HintText = "{=Telepathy_Settings_131}Prevent distant conversation with heroes which main hero have not met before.", Order = 2, RequireRestart = false)]
		public bool PreventTalkingToHeroesHaveNotMetBefore { get; set; } = false;

		[SettingPropertyGroup("{=Telepathy_Settings_100}Distant conversation realism", GroupOrder = 0)]
		[SettingPropertyBool(displayName: "{=Telepathy_Settings_140}Hide quest dialog lines", HintText = "{=Telepathy_Settings_141}Hide quest related dialogs during distant conversation.", Order = 3, RequireRestart = false)]
		public bool HideQuestDialogLines { get; set; } = false;
	}
}
