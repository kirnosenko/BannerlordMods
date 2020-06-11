using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace DeadShot
{
	public class DeadShotSubModule : MBSubModuleBase
	{
		private GameKey playerZoomKey;
		private bool? affectCurrentShot = null;
		private bool zoomState = false;

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			playerZoomKey = HotKeyManager.GetCategory("CombatHotKeyCategory").RegisteredGameKeys
				.SingleOrDefault(x => x != null && x.StringId == "Zoom");
		}

		protected override void OnApplicationTick(float dt)
		{
			if (Agent.Main != null && 
				Agent.Main.IsActive() &&
				Agent.Main.WieldedWeapon.IsAnyRanged(out var ranged))
			{
				var currentStage = Agent.Main.GetCurrentActionStage(1);

				if (currentStage == Agent.ActionStage.AttackReady || currentStage == Agent.ActionStage.AttackRelease)
				{
					var newZoomState = IsZoomActive();
					if (newZoomState != zoomState)
					{
						zoomState = newZoomState;
						affectCurrentShot = null;
					}

					if (affectCurrentShot == null)
					{
						var settings = DeadShotSettings.Instance;
						affectCurrentShot = MBRandom.RandomFloat <= settings.ActivationProbabilityPerShot &&
							(!settings.ActivateWithZoomOnly || IsZoomActive());
						if (affectCurrentShot.Value)
						{
							Mission.Current.Scene.SlowMotionFactor = settings.SlowMotionFactor;
							Mission.Current.Scene.SlowMotionMode = true;
						}
						else
						{
							Mission.Current.Scene.SlowMotionMode = false;
						}
					}
				}
				else if (affectCurrentShot != null)
				{
					Mission.Current.Scene.SlowMotionMode = false;
					affectCurrentShot = null;
					zoomState = false;
				}
			}
		}
		private bool IsZoomActive()
		{
			if (playerZoomKey == null)
			{
				return false;
			}

			return Input.IsKeyDown(playerZoomKey.PrimaryKey.InputKey) || Input.IsKeyDown(playerZoomKey.ControllerKey.InputKey);
		}
	}
}
