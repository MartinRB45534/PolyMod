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

		[HarmonyPrefix]
		[HarmonyPatch(typeof(DebugConsole), nameof(DebugConsole.ExecuteCommand))]
		private static bool DebugConsole_ExecuteCommand(ref string command)
		{
			return !Commands.Execute(command);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(DebugConsole), nameof(DebugConsole.CmdHelp))]
		private static void DebugConsole_CmdHelp()
		{
			Commands.Help();
		}

		[HarmonyPatch(typeof(SettingsScreen), nameof(SettingsScreen.CreateLanguageList))]
		private static bool SettingsScreen_CreateLanguageList(SettingsScreen __instance, UnityEngine.Transform parent)
		{
			List<string> list = new() { "Automatic", "English", "Français", "Deutsch", "Italiano", "Português", "Русский", "Español", "日本語", "한국어" };
			List<int> list2 = new() { 1, 3, 7, 9, 6, 4, 5, 8, 11, 12 };
			if (GameManager.GetPurchaseManager().IsTribeUnlocked(Polytopia.Data.TribeData.Type.Elyrion))
			{
				list.Add("∑∫ỹriȱŋ");
				list2.Add(10);
			}
			list.Add("Custom...");
			list2.Add(2);
			__instance.languageSelector = UnityEngine.Object.Instantiate(__instance.horizontalListPrefab, parent ?? __instance.container);
			__instance.languageSelector.UpdateScrollerOnHighlight = true;
			__instance.languageSelector.HeaderKey = "settings.language";
			__instance.languageSelector.SetIds(list2.ToArray());
			__instance.languageSelector.SetData(list.ToArray(), 0, false);
			__instance.languageSelector.SelectId(SettingsUtils.Language, true, -1f);
			__instance.languageSelector.IndexSelectedCallback = new Action<int>(__instance.LanguageChangedCallback);
			__instance.totalHeight += __instance.languageSelector.rectTransform.sizeDelta.y;

			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SettingsToggleGroup), nameof(SettingsToggleGroup.SetupContainers))]
		private static void SettingsToggleGroup_SetupContainers(SettingsToggleGroup __instance)
		{
			__instance.compactUIContainer.gameObject.SetActive(true);
			__instance.compactUIContainer.Value = SettingsUtils.UseCompactUI;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SettingsUtils), nameof(SettingsUtils.UseCompactUI), MethodType.Getter)]
		private static void SettingsUtils_UseCompactUI(ref bool __result)
		{
			__result = PlayerPrefsUtils.GetBoolValue("useCompactUI", false);
		}

		// This makes tribe selection popups fullscreen like on mobile if compact UI is on, not sure if we should keep it but it would be funny
		
		// [HarmonyPostfix]
		// [HarmonyPatch(typeof(SelectTribePopup), nameof(SelectTribePopup.OnEnable))]
		// private static void SelectTribePopup_OnEnable(ref PopupBase __instance)
		// {
		// 	if (SettingsUtils.UseCompactUI)
		// 	{
		// 		__instance.fullscreenVariant = true;
		// 	}
		// }
	}
}
