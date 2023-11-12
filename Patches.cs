using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace PolyMod
{
	internal class Patches
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(TileData), nameof(TileData.GetExplored))]
		private static void TileData_GetExplored(ref bool __result)
		{
			if (Plugin.foghack)
			{
				__result = true;
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
		private static void GameManager_Awake(GameManager __instance)
		{
			ClassInjector.RegisterTypeInIl2Cpp(typeof(Behaviour));
			__instance.gameObject.AddComponent<Behaviour>();
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.Generate))]
		static void MapGenerator_Generate(ref GameState state)
		{
			MapEditor.PreGenerate(ref state);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.Generate))]
		private static void MapGenerator_Generate_(ref GameState state)
		{
			MapEditor.PostGenerate(ref state);
		}
	}
}
