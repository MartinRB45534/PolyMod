using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace PolyMod
{
	[BepInPlugin("com.polymod", "PolyMod", "1.0.0.0")]
	public class Plugin : BasePlugin
	{
		internal static bool foghack = false;

		public override void Load() => Load_();

		private static void Load_()
		{
			Harmony.CreateAndPatchAll(typeof(Patches));
		}

		internal static void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				foghack = !foghack;
				Popup.ShowStatus(nameof(foghack), foghack);
			}
		}

		internal static Il2CppReferenceArray<T> ToIl2CppArray<T>(params T[] array) where T : Il2CppObjectBase
		{
			return new Il2CppReferenceArray<T>(array);
		}
	}
}
