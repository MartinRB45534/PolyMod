using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace PolyMod
{
	internal static class Popup
	{
		internal static void Show(string value)
		{
			Il2CppReferenceArray<PopupBase.PopupButtonData> buttons = Plugin.ToIl2CppArray(new PopupBase.PopupButtonData("buttons.ok"));
			PopupManager.GetBasicPopup(new PopupManager.BasicPopupData("PolyMod", value, buttons)).Show();
		}

		internal static void ShowStatus(string name, bool value)
		{
			string status = "<color=\"red\">DISABLED";
			if (value)
			{
				status = "<color=\"green\">ENABLED";
			}
			Show($"Module {name} is now {status}");
		}
	}
}
