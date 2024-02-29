using HarmonyLib;

namespace PolyMod
{
	internal class Patcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.Update))]
		private static void GameManager_Update(GameManager __instance)
		{
			Plugin.Update();
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.CreateRemoteClient))]
		private static bool GameManager_CreateRemoteClient()
		{
			// print the stack trace to find the caller
			Log.Info(Environment.StackTrace);
			return true;
		}
	}
}
