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
	}
}
