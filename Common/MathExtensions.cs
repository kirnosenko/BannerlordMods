using System;
using TaleWorlds.Library;

namespace Common
{
	public static class MathExtensions
	{
		public static float Distance(this Vec2 value1, Vec2 value2)
		{
			float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			return (float)Math.Sqrt((v1 * v1) + (v2 * v2));
		}
	}
}
