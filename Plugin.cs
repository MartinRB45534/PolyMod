using BepInEx;
using HarmonyLib;
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
		internal static readonly string MODS_PATH = Path.Combine(BepInEx.Paths.BepInExRootPath, "..", "Mods/");

		internal static bool start = false;

		public override void Load()
		{
			Harmony.CreateAndPatchAll(typeof(Patches));
		}

		internal static void Start()
		{
			DevConsole.Init();
		}

		internal static void Update()
		{
			if (!start)
			{
				Start();
				start = true;
			}

			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab))
			{
				DevConsole.Toggle();
			}
		}

		internal static void PatchEnum<T>(string name, int idx) where T : struct, IComparable, IFormattable, IConvertible
		{
			if (EnumCache<T>.nameMap == null) 
			{
				EnumCache<T>.nameMap = new();
			}
			if (EnumCache<T>.typeMap == null)
			{
				EnumCache<T>.typeMap = new();
			}
			EnumCache<T>.nameMap.TryAdd((T)(object)idx, name);
			EnumCache<T>.typeMap.TryAdd(name, (T)(object)idx);
		}
	}
}
