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
				ShowStatusPopup(nameof(foghack), foghack);
			}
		}

		private static void ShowPopup(string value)
		{
			Il2CppReferenceArray<PopupBase.PopupButtonData> buttons = ToIl2CppArray(new PopupBase.PopupButtonData("buttons.ok"));
			PopupManager.GetBasicPopup(new PopupManager.BasicPopupData("PolyMod", value, buttons)).Show();
		}

		private static void ShowStatusPopup(string name, bool value)
		{
			string status = "<color=\"red\">DISABLED";
			if (value)
			{
				status = "<color=\"green\">ENABLED";
			}
			ShowPopup($"Module {name} is now {status}");
		}

		private static Il2CppReferenceArray<T> ToIl2CppArray<T>(params T[] array) where T : Il2CppObjectBase
		{
			return new Il2CppReferenceArray<T>(array);
		}
	}
}
