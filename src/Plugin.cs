using BepInEx;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PolyMod
{
	[BepInPlugin("com.polymod", "PolyMod", "0.0.0")]
	public class Plugin : BepInEx.Unity.IL2CPP.BasePlugin
	{
		internal const uint MAP_MIN_SIZE = 6;
		internal const uint MAP_MAX_SIZE = 100;
		internal const int MAP_MAX_PLAYERS = 100;
		internal const int CAMERA_CONSTANT = 1000;
		internal const int AUTOIDX_STARTS_FROM = 1000;
		internal static readonly string BASE_PATH = Path.Combine(BepInEx.Paths.BepInExRootPath, "..");
		internal static readonly string MODS_PATH = Path.Combine(BASE_PATH, "Mods");
		internal static readonly string MAPS_PATH = Path.Combine(BASE_PATH, "Maps");
		internal static JsonMergeSettings GLD_MERGE_SETTINGS = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Merge };

		internal static int version = -1;
		internal static bool start = false;

		internal static ClientBase? replayClient;

		public override void Load()
		{
			Harmony.CreateAndPatchAll(typeof(Patcher));
		}

		internal static void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				ReplayResumer.Resume();
			}
			if (Input.GetKeyDown(KeyCode.T))
			{
				ReplayResumer.BackToMove();
			}
		}

		internal static string GetJTokenName(JToken token, int n = 1)
		{
			return token.Path.Split('.')[^n];
		}
	}
}
