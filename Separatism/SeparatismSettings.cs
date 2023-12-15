using System.ComponentModel;
using System.Linq;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace Separatism
{
	public class SeparatismSettings : AttributeGlobalSettings<SeparatismSettings>, INotifyPropertyChanged
	{
		private static string modVersion;
		private static string modName;
		private static string modId;
		private int friendThreshold, enemyThreshold;
		private int settlementRebellionStartLoyaltyThreshold, settlementRebellionEndLoyaltyThreshold;

		static SeparatismSettings()
		{
			modVersion = ModuleHelper.GetModules().SingleOrDefault(x => x.Name == nameof(Separatism))?.Version.ToString() ?? string.Empty;
			modName = $"{new TextObject("{=Separatism_Mod_Name}Separatism Mod").ToString()} {modVersion}";
			modId = $"{nameof(Separatism)}_{modVersion}";
		}
		public SeparatismSettings()
		{
			friendThreshold = 10;
			enemyThreshold = -10;
			settlementRebellionStartLoyaltyThreshold = 25;
			settlementRebellionEndLoyaltyThreshold = 50;
		}

		public override string DisplayName => modName;

		public override string FolderName => nameof(Separatism);

		public override string Id => modId;

		public override string FormatType => "json2";

		[SettingPropertyGroup("{=Separatism_Settings_000}Chaos start", GroupOrder = 0)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_000}Chaos start", HintText = "{=Separatism_Settings_001}Enable BIG Calradia separation on start of a new game. All clans with fiefs will found their own independent kingdoms.", Order = 0, IsToggle = true, RequireRestart = false)]
		public bool ChaosStartEnabled { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_000}Chaos start", GroupOrder = 0)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_010}Minimal number of wars per kindom at start", minValue: 0, maxValue: 10, HintText = "{=Separatism_Settings_011}At start of a new game each kindom will be at war with specified number of nearest kingdoms.", Order = 1, RequireRestart = false)]
		public int MinimalNumberOfWarsPerChaosKindom { get; set; } = 1;

		[SettingPropertyGroup("{=Separatism_Settings_100}Lord rebellions", GroupOrder = 1)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_100}Lord rebellions", HintText = "{=Separatism_Settings_101}Enable rebellions depending on lord relationship.", Order = 0, IsToggle = true, RequireRestart = false)]
		public bool LordRebellionsEnabled { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_100}Lord rebellions", GroupOrder = 1)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_110}Average amount of kingdom fiefs is enough to rebel", HintText = "{=Separatism_Settings_111}A clan can rebel when amount of its fiefs >= amount of kingdom fiefs / number of kingdom non-mercenary clans", Order = 1, RequireRestart = false)]
		public bool AverageAmountOfKingdomFiefsIsEnoughToRebel { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_100}Lord rebellions", GroupOrder = 1)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_120}Minimal amount of kingdom fiefs to rebel", minValue: 1, maxValue: 20, HintText = "{=Separatism_Settings_121}A clan can rebel when amount of its fiefs >= specified number. The amount of fiefs counts like this: 2 per town, 1 per castle, 0 per village.", Order = 2, RequireRestart = false)]
		public int MinimalAmountOfKingdomFiefsToRebel { get; set; } = 3;

		[SettingPropertyGroup("{=Separatism_Settings_100}Lord rebellions", GroupOrder = 1)]
		[SettingPropertyFloatingInteger(displayName: "{=Separatism_Settings_130}Daily rebellion chance", minValue: 0, maxValue: 1, HintText = "{=Separatism_Settings_131}Daily chance of lord rebellion when all requirements are met. 1 - start a rebellion as soon as possible. 0 - never.", Order = 3, RequireRestart = false)]
		public float DailyLordRebellionChance { get; set; } = 1;

		[SettingPropertyGroup("{=Separatism_Settings_800}National rebellions", GroupOrder = 2)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_800}National rebellions", HintText = "{=Separatism_Settings_801}Enable rebellions depending on number of foreign lords having their culture fiefs in a single kingdom.", Order = 0, IsToggle = true, RequireRestart = false)]
		public bool NationalRebellionsEnabled { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_800}National rebellions", GroupOrder = 2)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_810}Minimal required number of lords", minValue: 1, maxValue: 5, HintText = "{=Separatism_Settings_811}Number of foreign lords having their culture fiefs in a single kingdom required to start a national rebellion.", Order = 1, RequireRestart = false)]
		public int MinimalRequiredNumberOfNativeLords { get; set; } = 2;

		[SettingPropertyGroup("{=Separatism_Settings_800}National rebellions", GroupOrder = 2)]
		[SettingPropertyFloatingInteger(displayName: "{=Separatism_Settings_820}Daily rebellion chance", minValue: 0, maxValue: 1, HintText = "{=Separatism_Settings_821}Daily chance of national rebellion when all requirements are met. 1 - start a rebellion as soon as possible. 0 - never.", Order = 2, RequireRestart = false)]
		public float DailyNationalRebellionChance { get; set; } = 1;

		[SettingPropertyGroup("{=Separatism_Settings_900}Anarchy rebellions", GroupOrder = 3)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_900}Anarchy rebellions", HintText = "{=Separatism_Settings_901}Enable rebellions depending on number of fiefs per single clan and how often lords visit their fiefs.", Order = 0, IsToggle = true, RequireRestart = false)]
		public bool AnarchyRebellionsEnabled { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_900}Anarchy rebellions", GroupOrder = 3)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_910}Critical amount of fiefs per single clan", minValue: 1, maxValue: 20, HintText = "{=Separatism_Settings_911}A settlement can rebel when owner clan has not less than specified amount of fiefs. The amount of fiefs counts like this: 2 per town, 1 per castle, 0 per village.", Order = 1, RequireRestart = false)]
		public int CriticalAmountOfFiefsPerSingleClan { get; set; } = 10;

		[SettingPropertyGroup("{=Separatism_Settings_900}Anarchy rebellions", GroupOrder = 3)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_920}Number of days after owner visit to keep order", minValue: 1, maxValue: 30, HintText = "{=Separatism_Settings_921}A settlement can not rebel when its owner has visited it in last number of days.", Order = 2, RequireRestart = false)]
		public int NumberOfDaysAfterOwnerVisitToKeepOrder { get; set; } = 14;

		[SettingPropertyGroup("{=Separatism_Settings_900}Anarchy rebellions", GroupOrder = 3)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_930}Bonus rebel fief for high tier clan", HintText = "{=Separatism_Settings_931}An anarchy rebellion takes an additional fief when a high tier clan was called on rulership.", Order = 3, RequireRestart = false)]
		public bool BonusRebelFiefForHighTierClan { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_900}Anarchy rebellions", GroupOrder = 3)]
		[SettingPropertyFloatingInteger(displayName: "{=Separatism_Settings_940}Daily rebellion chance", minValue: 0, maxValue: 1, HintText = "{=Separatism_Settings_941}Daily chance of anarchy rebellion when all requirements are met. 1 - start a rebellion as soon as possible. 0 - never.", Order = 4, RequireRestart = false)]
		public float DailyAnarchyRebellionChance { get; set; } = 1;

		[SettingPropertyGroup("{=Separatism_Settings_700}Settlement rebellions", GroupOrder = 4)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_700}Settlement rebellions", HintText = "{=Separatism_Settings_701}Enable rebellions depending on settlement loyalty. Build in game rebellion mechanic. By default is inactive probably for a reason. So use it at your own risk.", Order = 0, IsToggle = true, RequireRestart = false)]
		public bool SettlementRebellionsEnabled { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_700}Settlement rebellions", GroupOrder = 4)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_710}Start loyalty threshold", minValue: 0, maxValue: 100, HintText = "{=Separatism_Settings_711}Loyalty threshold where a rebellion begins. May not be greater than end loyalty threshold.", Order = 1, RequireRestart = false)]
		public int SettlementRebellionStartLoyaltyThreshold
		{
			get { return settlementRebellionStartLoyaltyThreshold; }
			set
			{
				settlementRebellionStartLoyaltyThreshold = value;
				if (settlementRebellionStartLoyaltyThreshold > settlementRebellionEndLoyaltyThreshold)
				{
					settlementRebellionEndLoyaltyThreshold = value;
					OnPropertyChanged(nameof(settlementRebellionEndLoyaltyThreshold));
				}
			}
		}

		[SettingPropertyGroup("{=Separatism_Settings_700}Settlement rebellions", GroupOrder = 4)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_720}End loyalty threshold", minValue: 0, maxValue: 100, HintText = "{=Separatism_Settings_721}Loyalty threshold where a rebellion stops. May not be lesser than start loyalty threshold.", Order = 2, RequireRestart = false)]
		public int SettlementRebellionEndLoyaltyThreshold
		{
			get { return settlementRebellionEndLoyaltyThreshold; }
			set
			{
				settlementRebellionEndLoyaltyThreshold = value;
				if (settlementRebellionEndLoyaltyThreshold < settlementRebellionStartLoyaltyThreshold)
				{
					settlementRebellionStartLoyaltyThreshold = value;
					OnPropertyChanged(nameof(settlementRebellionStartLoyaltyThreshold));
				}
			}
		}

		[SettingPropertyGroup("{=Separatism_Settings_200}Relation thresholds", GroupOrder = 5)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_210}Friendship threshold", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_211}Relation threshold where heroes become friends. May not be lesser than hostility threshold.", Order = 0, RequireRestart = false)]
		public int FriendThreshold
		{
			get { return friendThreshold; }
			set {
				friendThreshold = value;
				if (enemyThreshold > friendThreshold)
				{
					enemyThreshold = value;
					OnPropertyChanged(nameof(EnemyThreshold));
				}
			}
		}

		[SettingPropertyGroup("{=Separatism_Settings_200}Relation thresholds", GroupOrder = 5)]
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
					OnPropertyChanged(nameof(FriendThreshold));
				}
			}
		}

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", GroupOrder = 6)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_310}For rebel clan with the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_311}Relation change between a clan and the ruler as the result of rebellion.", Order = 0, RequireRestart = false)]
		public int RelationChangeRebelWithRuler { get; set; } = -20;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", GroupOrder = 6)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_320}For rebel clan with friend vassals of the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_321}Relation change between a clan and friend vassals of the ruler as the result of rebellion.", Order = 1, RequireRestart = false)]
		public int RelationChangeRebelWithRulerFriendVassals { get; set; } = -10;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", GroupOrder = 6)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_330}For rebel clan with enemy vassals of the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_331}Relation change between a clan and enemy vassals of the ruler as the result of rebellion.", Order = 2, RequireRestart = false)]
		public int RelationChangeRebelWithRulerEnemyVassals { get; set; } = +10;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", GroupOrder = 6)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_340}For rebel clan with other vassals of the ruler", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_341}Relation change between a clan and other vassals of the ruler as the result of rebellion.", Order = 3, RequireRestart = false)]
		public int RelationChangeRebelWithRulerVassals { get; set; } = 0;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", GroupOrder = 6)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_350}For rulers of united kingdoms", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_351}Relation change between two rulers as the result of union of their kingdoms.", Order = 4, RequireRestart = false)]
		public int RelationChangeUnitedRulers { get; set; } = +20;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", GroupOrder = 6)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_360}For ruler and supporting clan", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_361}Relation change between the ruler of a weak kingdom and a supporting clan that joined the kingdom.", Order = 5, RequireRestart = false)]
		public int RelationChangeRulerWithSupporter { get; set; } = +20;

		[SettingPropertyGroup("{=Separatism_Settings_300}Relation changes", GroupOrder = 6)]
		[SettingPropertyInteger(displayName: "{=Separatism_Settings_370}For clans participating in national rebellion", minValue: -100, maxValue: 100, HintText = "{=Separatism_Settings_371}Relation change between the rulers of clans participating in national rebellion.", Order = 6, RequireRestart = false)]
		public int RelationChangeNationalRebellionClans { get; set; } = +20;

		[SettingPropertyGroup("{=Separatism_Settings_500}Politics", GroupOrder = 7)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_510}Keep empty kingdoms", HintText = "{=Separatism_Settings_511}Allows to keep empty kingdoms unremoved for compatibility with other mods. Separatist empty kingdoms will be removed anyway.", Order = 0, RequireRestart = false)]
		public bool KeepEmptyKingdoms { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_500}Politics", GroupOrder = 7)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_520}Keep original kindom wars", HintText = "{=Separatism_Settings_521}Allows to keep all original kingdom wars for a new rebel kingdom.", Order = 1, RequireRestart = false)]
		public bool KeepOriginalKindomWars { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_500}Politics", GroupOrder = 7)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_530}Allow unions", HintText = "{=Separatism_Settings_531}Allows friendly kingdoms to unite under banner of one of them.", Order = 2, RequireRestart = false)]
		public bool AllowUnions { get; set; } = true;

		[SettingPropertyGroup("{=Separatism_Settings_600}Banners", GroupOrder = 8)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_610}Keep separatist banner colors", HintText = "{=Separatism_Settings_611}Allows to keep separatist banners unchanged for compatibility with other mods.", Order = 0, RequireRestart = false)]
		public bool KeepRebelBannerColors { get; set; } = false;

		[SettingPropertyGroup("{=Separatism_Settings_600}Banners", GroupOrder = 8)]
		[SettingPropertyBool(displayName: "{=Separatism_Settings_620}Same banner colors for all separatists", HintText = "{=Separatism_Settings_621}Not works if you choose to keep separatist banner colors.", Order = 1, RequireRestart = false)]
		public bool SameColorsForAllRebels { get; set; } = false;
	}
}
