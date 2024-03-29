﻿using System.Linq;
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
		private float releaseTime = 0;
		private int timeRequestId = 123456789;

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			playerZoomKey = HotKeyManager.GetCategory("CombatHotKeyCategory").RegisteredGameKeys
				.SingleOrDefault(x => x != null && x.StringId == "Zoom");
		}

		protected override void OnApplicationTick(float dt)
		{
			if (Agent.Main != null && Agent.Main.IsActive())
			{
				var currentStage = Agent.Main.GetCurrentActionStage(1);

				if (Agent.Main.WieldedWeapon.GetRangedUsageIndex() >= 0 && 
					(currentStage == Agent.ActionStage.AttackReady ||
						(currentStage == Agent.ActionStage.AttackRelease && releaseTime < 1)))
				{
					if (currentStage == Agent.ActionStage.AttackRelease)
					{
						releaseTime += dt;
					}
					else
					{
						releaseTime = 0;
					}

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
							SetSlowMotion(settings.SlowMotionFactor);
						}
						else
						{
							SetSlowMotion(null);
						}
					}
				}
				else if (affectCurrentShot != null)
				{
					SetSlowMotion(null);
					affectCurrentShot = null;
					zoomState = false;
				}
			}
		}

		private void SetSlowMotion(float? slowMotionFactor)
		{
			if (slowMotionFactor != null)
			{
				Mission.Current.AddTimeSpeedRequest(new Mission.TimeSpeedRequest(
					slowMotionFactor.Value, timeRequestId));
			}
			else if (Mission.Current.GetRequestedTimeSpeed(timeRequestId, out _))
			{
				Mission.Current.RemoveTimeSpeedRequest(timeRequestId);
			}
		}

		private bool IsZoomActive()
		{
			if (playerZoomKey == null)
			{
				return false;
			}
			var key1 = playerZoomKey.KeyboardKey?.InputKey;
			var key2 = playerZoomKey.ControllerKey?.InputKey;

			return (key1.HasValue && Input.IsKeyDown(key1.Value)) || 
				(key2.HasValue && Input.IsKeyDown(key2.Value));
		}
	}
}
