using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Newtonsoft.Json.Linq;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;

namespace PolyMod
{
	internal class Patcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameStateUtils), nameof(GameStateUtils.GetRandomPickableTribe), new System.Type[] { typeof(GameState) })]
		public static bool GameStateUtils_GetRandomPickableTribe(GameState gameState)
		{
			if (Plugin.version > 0)
			{
				gameState.Version = Plugin.version;
				DebugConsole.Write($"Changed version to {Plugin.version}");
				Plugin.version = -1;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameModeScreen), nameof(GameModeScreen.OnGameMode))]
		private static bool GameModeScreen_OnGameMode(GameMode gameMode)
		{
			if (!Plugin.bots_only || gameMode != GameMode.Custom)
			{
				return true;
			}
            GameSettingsExtensions.TryLoadFromDisk(out GameSettings gameSettings, GameType.SinglePlayer, gameMode);
            GameManager.PreliminaryGameSettings = gameSettings;
			GameManager.PreliminaryGameSettings.BaseGameMode = gameMode;
			GameManager.debugAutoPlayLocalPlayer = true;
			UIManager.Instance.ShowScreen(UIConstants.Screens.GameSetup, false);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameSetupScreen), nameof(GameSetupScreen.CreateCustomGameModeList))]
		static private bool GameSetupScreen_CreateCustomGameModeList(UIHorizontalList __result, GameSetupScreen __instance)
		{
			if (!Plugin.bots_only)
			{
				return true;
			}
			string[] array = new string[]
			{
				Localization.Get(GameModeUtils.GetTitle(GameMode.Perfection)),
				Localization.Get(GameModeUtils.GetTitle(GameMode.Domination))
				// Remove the Sandbox (infinite) mode
			};
			__result = __instance.CreateHorizontalList("gamesettings.mode", array, new Action<int>(__instance.OnCustomGameModeChanged), __instance.GetCustomGameModeIndexFromSettings(), null, -1, null);
			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameSetupScreen), nameof(GameSetupScreen.CreateOpponentList))]
		private static bool GameSetupScreen_CreateOpponentList(ref UIHorizontalList __result, GameSetupScreen __instance, RectTransform? parent = null)
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			int maxOpponents = GameManager.GetMaxOpponents();
			int num = maxOpponents;
			if (GameManager.PreliminaryGameSettings.GameType == GameType.SinglePlayer)
			{
				num = Mathf.Min(GameManager.GetPurchaseManager().GetUnlockedTribeCount() - 1, maxOpponents);
			}
			string[] array = new string[maxOpponents + 1];
			for (int i = 0; i <= maxOpponents; i++)
			{
				array[i] = (i+1).ToString();
			}
			int num2 = Mathf.Min(num, GameManager.PreliminaryGameSettings.OpponentCount);
			__result = __instance.CreateHorizontalList("gamesettings.opponents", array, new Action<int>(__instance.OnOpponentsChanged), num2, parent, num + 1, new Action(__instance.OnTriedSelectDisabledOpponent));
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(LocalClient), nameof(LocalClient.IsPlayerLocal))]
		public static bool LocalClient_IsPlayerLocal(ref bool __result, byte playerId)
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			__result = true;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.IsPlayerViewing))]
		public static bool GameManager_IsPlayerViewing(ref bool __result)
		{
			if (Plugin.unview)
			{
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(WipePlayerReaction), nameof(WipePlayerReaction.Execute))]
		public static bool WipePlayerReaction_Execute()
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			GameManager.Client.ActionManager.isRecapping = true;
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(WipePlayerReaction), nameof(WipePlayerReaction.Execute))]
		public static void WipePlayerReaction_Execute_Postfix()
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return;
			}
			GameManager.Client.ActionManager.isRecapping = false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(StartTurnReaction), nameof(StartTurnReaction.Execute))]
		public static bool StartTurnReaction_Execute()
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			Plugin.localClient = GameManager.Client as LocalClient;
			if (Plugin.localClient == null)
			{
				return true;
			}
			// Replace the client (temporarily)
			GameManager.instance.client = new ReplayClient();
			GameManager.Client.currentGameState = Plugin.localClient.GameState;
			GameManager.Client.CreateOrResetActionManager(Plugin.localClient.lastSeenCommand);
			GameManager.Client.ActionManager.isRecapping = true;
			LevelManager.GetClientInteraction().DeselectUnit();
			LevelManager.GetClientInteraction().TryDeselectUnit(LevelManager.GetClientInteraction().selectedUnit);
			LevelManager.GetClientInteraction().DeselectTile(); // Just in case the human was clicking on stuff
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MapRenderer), nameof(MapRenderer.Refresh))]
		public static bool MapRenderer_Refresh()
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			if (Plugin.localClient != null)
			{ // Repair the client as soon as possible
				GameManager.instance.client = Plugin.localClient;
				Plugin.localClient = null;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(TaskCompletedReaction), nameof(TaskCompletedReaction.Execute))]
		public static bool TaskCompletedReaction_Execute(ref TaskCompletedReaction __instance, ref byte __state)
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			__state = __instance.action.PlayerId;
			__instance.action.PlayerId = 255;
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(TaskCompletedReaction), nameof(TaskCompletedReaction.Execute))]
		public static void TaskCompletedReaction_Execute_Postfix(ref TaskCompletedReaction __instance, ref byte __state)
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return;
			}
			__instance.action.PlayerId = __state;
		}

		// Patch multiple classes with the same method
		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeetReaction), nameof(MeetReaction.Execute))]
		[HarmonyPatch(typeof(EnableTaskReaction), nameof(EnableTaskReaction.Execute))]
		[HarmonyPatch(typeof(ExamineRuinsReaction), nameof(ExamineRuinsReaction.Execute))]
		[HarmonyPatch(typeof(InfiltrationRewardReaction), nameof(InfiltrationRewardReaction.Execute))]
		[HarmonyPatch(typeof(EstablishEmbassyReaction), nameof(EstablishEmbassyReaction.Execute))]
		[HarmonyPatch(typeof(DestroyEmbassyReaction), nameof(DestroyEmbassyReaction.Execute))]
		[HarmonyPatch(typeof(ReceiveDiplomacyMessageReaction), nameof(ReceiveDiplomacyMessageReaction.Execute))]
		public static bool Patch_Execute(ReactionBase __instance)
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			Plugin.unview = true;
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MeetReaction), nameof(MeetReaction.Execute))]
		[HarmonyPatch(typeof(EnableTaskReaction), nameof(EnableTaskReaction.Execute))]
		[HarmonyPatch(typeof(ExamineRuinsReaction), nameof(ExamineRuinsReaction.Execute))]
		[HarmonyPatch(typeof(InfiltrationRewardReaction), nameof(InfiltrationRewardReaction.Execute))]
		[HarmonyPatch(typeof(EstablishEmbassyReaction), nameof(EstablishEmbassyReaction.Execute))]
		[HarmonyPatch(typeof(DestroyEmbassyReaction), nameof(DestroyEmbassyReaction.Execute))]
		[HarmonyPatch(typeof(ReceiveDiplomacyMessageReaction), nameof(ReceiveDiplomacyMessageReaction.Execute))]
		[HarmonyPatch(typeof(StartTurnReaction), nameof(StartTurnReaction.Execute))]
		public static void Patch_Execute_Post()
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				if (Plugin.unview)
				{
					DebugConsole.Write("Uh, what!?");
				}
				return;
			}
			Plugin.unview = false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ClientActionManager), nameof(ClientActionManager.Update))]
		public static void Patch_Update()
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				if (Plugin.unview)
				{
					DebugConsole.Write("Uh, what!?");
				}
				if (Plugin.localClient != null)
				{
					DebugConsole.Write("Sorry, what!?");
				}
				return;
			}
			if (Plugin.localClient != null)
			{
				GameManager.instance.client = Plugin.localClient;
				Plugin.localClient = null;
			}
			Plugin.unview = false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AI), nameof(AI.GetMove))]
		public static bool AI_GetMove()
		{
			DebugConsole.Write("AI.GetMove");
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}

			//Save a screenshot
			ScreenCapture.CaptureScreenshot(Plugin.ML_PATH + "/screenshot" + Plugin.ml_idx + ".png");
			//Save the game state as a binary using GameManager.GameState.Serialize()
			Il2CppSystem.IO.BinaryWriter writer = new Il2CppSystem.IO.BinaryWriter(Il2CppSystem.IO.File.Open(Plugin.ML_PATH + "/gamestate" + Plugin.ml_idx++, Il2CppSystem.IO.FileMode.Create));
			GameManager.GameState.Serialize(writer, GameManager.GameState.Version);
			writer.Close();
			//Save some extra parameters in a JSON file
			Vector3 position1 = Camera.main.WorldToScreenPoint(GameManager.GameState.Map.tiles[0].coordinates.ToPosition());
			Vector3 position2 = Camera.main.WorldToScreenPoint(GameManager.GameState.Map.tiles[GameManager.GameState.Map.tiles.Length - 1].coordinates.ToPosition());
			Vector3 position3 = Camera.main.WorldToScreenPoint(GameManager.GameState.Map.tiles[GameManager.GameState.Map.Width - 1].coordinates.ToPosition());
			Vector3 position4 = Camera.main.WorldToScreenPoint(GameManager.GameState.Map.tiles[GameManager.GameState.Map.tiles.Length - GameManager.GameState.Map.Width].coordinates.ToPosition());
			JObject json = new JObject();
			json["x1"] = position1.x;
			json["y1"] = position1.y;
			json["x2"] = position2.x;
			json["y2"] = position2.y;
			json["x3"] = position3.x;
			json["y3"] = position3.y;
			json["x4"] = position4.x;
			json["y4"] = position4.y;
			//Save the JSON file
			Il2CppSystem.IO.File.WriteAllText(Plugin.ML_PATH + "/parameters" + Plugin.ml_idx + ".json", json.ToString());
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(LocalClient), nameof(LocalClient.CreateSession))]
		public static void LocalClient_CreateSession(ref Il2CppSystem.Threading.Tasks.Task<CreateSessionResult> __result, ref LocalClient __instance, GameSettings settings, List<PlayerState> players)
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return;
			}
			for (int j = 0; j < __instance.GameState.PlayerCount; j++)
			{
				PlayerState playerState = __instance.GameState.PlayerStates[j];
				playerState.AutoPlay = false;
				playerState.UserName = AccountManager.AliasInternal;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(LocalClient), nameof(LocalClient.GetCurrentLocalPlayer))]
		public static bool LocalClient_GetCurrentLocalPlayer(ref PlayerState __result, ref LocalClient __instance)
		{
			if (!Plugin.bots_only || GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				return true;
			}
			__result = __instance.GameState.PlayerStates[__instance.GameState.CurrentPlayerIndex];
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.Update))]
		private static void GameManager_Update(GameManager __instance)
		{
			Plugin.Update();
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.Generate))]
		static void MapGenerator_Generate(ref GameState state, ref MapGeneratorSettings settings)
		{
			MapLoader.PreGenerate(ref state, ref settings);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.Generate))]
		private static void MapGenerator_Generate_(ref GameState state)
		{
			MapLoader.PostGenerate(ref state);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GeneratePlayerCapitalPositions))]
		private static void MapGenerator_GeneratePlayerCapitalPositions(ref Il2CppSystem.Collections.Generic.List<int> __result, int width, int playerCount)
		{
			__result = MapLoader.GetCapitals(__result, width, playerCount);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.GetMaxOpponents))]
		private static void GameManager_GetMaxOpponents(ref int __result)
		{
			__result = Plugin.MAP_MAX_PLAYERS - 1;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MapDataExtensions), nameof(MapDataExtensions.GetMaximumOpponentCountForMapSize))]
		private static void MapDataExtensions_GetMaximumOpponentCountForMapSize(ref int __result)
		{
			__result = Plugin.MAP_MAX_PLAYERS;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PurchaseManager), nameof(PurchaseManager.GetUnlockedTribeCount))]
		private static void PurchaseManager_GetUnlockedTribeCount(ref int __result)
		{
			__result = Plugin.MAP_MAX_PLAYERS + 2;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PurchaseManager), nameof(PurchaseManager.IsTribeUnlocked))]
		private static void PurchaseManager_IsTribeUnlocked(ref bool __result, TribeData.Type type)
		{
			__result = (int)type >= Plugin.AUTOIDX_STARTS_FROM || __result;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.AddGameLogicPlaceholders))]
		private static void GameLogicData_Parse(JObject rootObject)
		{
			ModLoader.Init(rootObject);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(CameraController), nameof(CameraController.Awake))]
		private static void CameraController_Awake()
		{
			CameraController.Instance.maxZoom = Plugin.CAMERA_CONSTANT;
			CameraController.Instance.techViewBounds = new(
				new(Plugin.CAMERA_CONSTANT, Plugin.CAMERA_CONSTANT), CameraController.Instance.techViewBounds.size
			);
			UnityEngine.GameObject.Find("TechViewWorldSpace").transform.position = new(Plugin.CAMERA_CONSTANT, Plugin.CAMERA_CONSTANT);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetTileSpriteAddress), new Type[] { typeof(Polytopia.Data.TerrainData.Type), typeof(string) })]
		private static void SpriteData_GetTileSpriteAddress(ref SpriteAddress __result, Polytopia.Data.TerrainData.Type terrain, string skinId)
		{
			__result = ModLoader.GetSprite(__result, EnumCache<Polytopia.Data.TerrainData.Type>.GetName(terrain), skinId);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetResourceSpriteAddress), new Type[] { typeof(ResourceData.Type), typeof(string) })]
		private static void SpriteData_GetResourceSpriteAddress(ref SpriteAddress __result, ResourceData.Type type, string skinId)
		{
			__result = ModLoader.GetSprite(__result, EnumCache<ResourceData.Type>.GetName(type), skinId);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetBuildingSpriteAddress), new Type[] { typeof(ImprovementData.Type), typeof(string) })]
		private static void SpriteData_GetBuildingSpriteAddress(ref SpriteAddress __result, ImprovementData.Type type, string skinId)
		{
			__result = ModLoader.GetSprite(__result, EnumCache<ImprovementData.Type>.GetName(type), skinId);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetUnitIconAddress))]
		private static void SpriteData_GetUnitIconAddress(ref SpriteAddress __result, UnitData.Type type)
		{
			__result = ModLoader.GetSprite(__result, "icon", EnumCache<UnitData.Type>.GetName(type));
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetHeadSpriteAddress), new Type[] { typeof(int) })]
		private static void SpriteData_GetHeadSpriteAddress_1(ref SpriteAddress __result, int tribe)
		{
			__result = ModLoader.GetSprite(__result, "head", $"{tribe}");
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetHeadSpriteAddress), new Type[] { typeof(string) })]
		private static void SpriteData_GetHeadSpriteAddress_2(ref SpriteAddress __result, string specialId)
		{
			__result = ModLoader.GetSprite(__result, "head", specialId);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetAvatarPartSpriteAddress))]
		private static void SpriteData_GetAvatarPartSpriteAddress(ref SpriteAddress __result, string sprite)
		{
			__result = ModLoader.GetSprite(__result, sprite, "");
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpriteData), nameof(SpriteData.GetHouseAddresses))]
		private static void SpriteData_GetHouseAddresses(ref Il2CppReferenceArray<SpriteAddress> __result, int type, string styleId, SkinType skinType)
		{
			List<SpriteAddress> sprites = new()
			{
				ModLoader.GetSprite(__result[0], "house", styleId, type)
			};
			if (skinType != SkinType.Default)
			{
				sprites.Add(ModLoader.GetSprite(__result[1], "house", EnumCache<SkinType>.GetName(skinType), type));
			}
			__result = sprites.ToArray();
		}

		[HarmonyPrefix]
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
	}
}
