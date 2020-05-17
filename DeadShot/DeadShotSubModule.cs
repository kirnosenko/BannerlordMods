using TaleWorlds.MountAndBlade;

namespace DeadShot
{
	public class DeadShotSubModule : MBSubModuleBase
	{
		protected override void OnApplicationTick(float dt)
		{
			if (Agent.Main != null && Agent.Main.IsActive())
			{
				if (Agent.Main.WieldedWeapon.IsAnyRanged(out var ranged))
				{
					var currentStage = Agent.Main.GetCurrentActionStage(1);

					if (currentStage == Agent.ActionStage.AttackReady || currentStage == Agent.ActionStage.AttackRelease)
					{
						Mission.Current.Scene.SlowMotionFactor = DeadShotSettings.Instance.SlowMotionFactor;
						Mission.Current.Scene.SlowMotionMode = true;
					}
					else
					{
						Mission.Current.Scene.SlowMotionMode = false;
					}
				}
			}
		}
	}
}
