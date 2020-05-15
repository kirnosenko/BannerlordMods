using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using MBOptionScreen.Attributes;
using MBOptionScreen.Attributes.v2;
using MBOptionScreen.Settings;

namespace Separatism
{
	public class SeparatismSettings : AttributeSettings<SeparatismSettings>
	{
		private string modVersion;
		private string modName;
		private int friendThreshold, enemyThreshold;

		public SeparatismSettings()
		{
			modVersion = ModuleInfo.GetModules().SingleOrDefault(x => x.Name == nameof(Separatism))?.Version.ToString() ?? "";
			modName = $"{new TextObject("{=Separatism_Mod_Name}Separatism Mod").ToString()} {modVersion}";
			Id = $"{nameof(Separatism)}_{modVersion}";
			friendThreshold = 10;
			enemyThreshold = -10;
		}

		public override string ModName => modName;

		public override string ModuleFolderName => nameof(Separatism);

		public override string Id { get; set; }

		[SettingPropertyGroup("{=Separatism_Settings_100}Rebel reasoning", 0)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_110}Average amount of kingdom fiefs is enough to rebel", HintText = "{=Separatism_Settings_111}A clan has a reason to rebel when amount of its fiefs >= amount of kingdom fiefs / number of kingdom non-mercenary clans", Order = 0, RequireRestart = false)]
		public bool AverageAmountOfKingdomFiefsIsEnoughToRebel { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_100}Rebel reasoning", 0)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_120}Minimal amount of kingdom fiefs to rebel", minValue: 1, maxValue: 20, HintText = "{=Separatism_Settings_121}A clan has a reason to rebel when amount of its fiefs >= specified number. The amount of fiefs counts like this: 2 per town, 1 per castle, 0 per village.", Order = 1, RequireRestart = false)]
		public int MinimalAmountOfKingdomFiefsToRebel { get; set; } = 3;

		[SettingPropertyGroup("{=Separatism_Settings_100}Rebel reasoning", 0)]
		[SettingPropertyFloatingInteger(displayName: "{=Separatism_Settings_130}Daily chance to rebel when a clan is ready", minValue: 0, maxValue: 1, HintText = "{=Separatism_Settings_131}1 - start a rebellion as soon as possible. 0 - never.", Order = 2, RequireRestart = false)]
		public float DailyChanceToRebelWhenClanIsReady { get; set; } = 1;

		[SettingPropertyGroup("{=Separatism_Settings_200}Relation thresholds", 1)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_210}Friendship threshold", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_211}Relation threshold where heroes become friends. May not be lesser than hostility threshold.", Order = 0, RequireRestart = false)]
		public int FriendThreshold
		{
			get { return friendThreshold; }
			set {
				friendThreshold = value;
				if (enemyThreshold > friendThreshold)
				{
					enemyThreshold = value;
				}
			}
		}

		[SettingPropertyGroup("{=Separatism_Settings_200}Relation thresholds", 1)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_220}Hostility threshold", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_221}Relation threshold where heroes become enemies. May not be greater than friendship threshold.", Order = 1, RequireRestart = false)]
		public int EnemyThreshold
		{
			get { return enemyThreshold; }
			set
			{
				enemyThreshold = value;
				if (friendThreshold < enemyThreshold)
				{
					friendThreshold = value;
				}
			}
		}

		[SettingPropertyGroup("{=Separatism_Settings_500}Politics", 2)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_510}Keep empty kingdoms", HintText = "{=Separatism_Settings_511}Allows to keep empty kingdoms unremoved for compatibility with other mods. Separatist empty kingdoms will be removed anyway.", Order = 0, RequireRestart = false)]
		public bool KeepEmptyKingdoms { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_500}Politics", 2)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_520}Keep original kindom wars", HintText = "{=Separatism_Settings_521}Allows to keep all original kingdom wars for a new rebel kingdom.", Order = 1, RequireRestart = false)]
		public bool KeepOriginalKindomWars { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_600}Banners", 3)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_610}Keep separatist banner colors", HintText = "{=Separatism_Settings_611}Allows to keep separatist banners unchanged for compatibility with other mods.", Order = 0, RequireRestart = false)]
		public bool KeepRebelBannerColors { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_600}Banners", 3)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_620}Same banner colors for all separatists", HintText = "{=Separatism_Settings_621}Not works if you choose to keep separatist banner colors.", Order = 1, RequireRestart = false)]
		public bool SameColorsForAllRebels { get; set; } = false;
	}
}
