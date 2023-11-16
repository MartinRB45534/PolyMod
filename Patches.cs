using HarmonyLib;

namespace PolyMod
{
	internal class Patches
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(TileData), nameof(TileData.GetExplored))]
		private static void TileData_GetExplored(ref bool __result, ref byte playerId)
		{
			if (GameManager.LocalPlayer == null) return;
			if (Plugin.foghack && playerId == GameManager.LocalPlayer.Id)
			{
				__result = true;
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.Update))]
		private static void GameManager_Update(GameManager __instance)
		{
			Plugin.Update();
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
