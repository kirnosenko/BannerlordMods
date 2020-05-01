using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Common
{
	public static class GameLog
	{
		public static void Info(string text)
		{
			Print(text, Color.FromUint(0x00FFFFFF));
		}

		public static void Warn(string text)
		{
			Print(text, Color.FromUint(0x00FF0000));
		}

		public static void Error(Exception e)
		{
			Warn($"ERROR: {e.Message}{Environment.NewLine}{e.StackTrace}");
		}

		private static void Print(string text, Color color)
		{
			InformationManager.DisplayMessage(new InformationMessage(text, color));
		}
	}
}
