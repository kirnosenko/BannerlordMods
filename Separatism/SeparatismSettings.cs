using System.ComponentModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using MBOptionScreen.Attributes;
using MBOptionScreen.Attributes.v2;
using MBOptionScreen.Settings;

namespace Separatism
{
	public class SeparatismSettings : AttributeSettings<SeparatismSettings>, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private static string modVersion;
		private static string modName;
		private static string modId;
		private int friendThreshold, enemyThreshold;

		static SeparatismSettings()
		{
			modVersion = ModuleInfo.GetModules().SingleOrDefault(x => x.Name == nameof(Separatism))?.Version.ToString() ?? string.Empty;
			modName = $"{new TextObject("{=Separatism_Mod_Name}Separatism Mod").ToString()} {modVersion}";
			modId = $"{nameof(Separatism)}_{modVersion}";
		}
		public SeparatismSettings()
		{
			Id = modId;
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
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(EnemyThreshold)));
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
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(FriendThreshold)));
				}
			}
		}

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", 2)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_310}For rebel clan with the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_311}Relation change between a clan and the ruler as the result of rebellion.", Order = 0, RequireRestart = false)]
		public int RelationChangeRebelWithRuler { get; set; } = -20;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", 2)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_320}For rebel clan with friend vassals of the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_321}Relation change between a clan and friend vassals of the ruler as the result of rebellion.", Order = 1, RequireRestart = false)]
		public int RelationChangeRebelWithRulerFriendVassals { get; set; } = -10;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", 2)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_330}For rebel clan with enemy vassals of the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_331}Relation change between a clan and enemy vassals of the ruler as the result of rebellion.", Order = 2, RequireRestart = false)]
		public int RelationChangeRebelWithRulerEnemyVassals { get; set; } = +10;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", 2)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_340}For rebel clan with other vassals of the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_341}Relation change between a clan and other vassals of the ruler as the result of rebellion.", Order = 3, RequireRestart = false)]
		public int RelationChangeRebelWithRulerVassals { get; set; } = 0;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", 2)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_350}For rulers of united kingdoms", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_351}Relation change between two rulers as the result of union of their kingdoms.", Order = 4, RequireRestart = false)]
		public int RelationChangeUnitedRulers { get; set; } = +20;

		[SettingPropertyGroup("{=Separatism_Settings_500}Politics", 3)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_510}Keep empty kingdoms", HintText = "{=Separatism_Settings_511}Allows to keep empty kingdoms unremoved for compatibility with other mods. Separatist empty kingdoms will be removed anyway.", Order = 0, RequireRestart = false)]
		public bool KeepEmptyKingdoms { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_500}Politics", 3)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_520}Keep original kindom wars", HintText = "{=Separatism_Settings_521}Allows to keep all original kingdom wars for a new rebel kingdom.", Order = 1, RequireRestart = false)]
		public bool KeepOriginalKindomWars { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_600}Banners", 4)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_610}Keep separatist banner colors", HintText = "{=Separatism_Settings_611}Allows to keep separatist banners unchanged for compatibility with other mods.", Order = 0, RequireRestart = false)]
		public bool KeepRebelBannerColors { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_600}Banners", 4)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_620}Same banner colors for all separatists", HintText = "{=Separatism_Settings_621}Not works if you choose to keep separatist banner colors.", Order = 1, RequireRestart = false)]
		public bool SameColorsForAllRebels { get; set; } = false;
	}
}
