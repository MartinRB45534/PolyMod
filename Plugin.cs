using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace PolyMod
{
	[BepInPlugin("com.polymod", "PolyMod", "1.0.0.0")]
	public class Plugin : BepInEx.Unity.IL2CPP.BasePlugin
	{
		internal const uint MAP_MIN_SIZE = 6;
		internal const uint MAP_MAX_SIZE = 100;

		internal static bool console = false;
		internal static bool foghack = false;

		public override void Load()
		{
			Harmony.CreateAndPatchAll(typeof(Patches));

			Commands.Add("foghack", delegate 
			{
				foghack = !foghack;
				DebugConsole.Write($"Foghack status is now {foghack}");
			});
			Commands.Add("currencyhack", delegate
			{
				GameManager.LocalPlayer.Currency += 1000;
				DebugConsole.Write($"+1000 stars");
			});
		}

		internal static void Update()
		{
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab))
			{
				if (console)
				{
					DebugConsole.Hide();
				}
				else
				{
					DebugConsole.Show();
				}
				console = !console;
			}
		}

		internal static Il2CppReferenceArray<T> ToIl2CppArray<T>(params T[] array) where T : Il2CppObjectBase
		{
			return new Il2CppReferenceArray<T>(array);
		}
	}
}
