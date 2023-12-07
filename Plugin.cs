using BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PolyMod
{
	[BepInPlugin("com.polymod", "PolyMod", "1.0.0.0")]
	public class Plugin : BepInEx.Unity.IL2CPP.BasePlugin
	{
		internal const uint MAP_MIN_SIZE = 6;
		internal const uint MAP_MAX_SIZE = 100;

		internal static bool start = false;
		internal static bool console = false;

		public override void Load()
		{
			Harmony.CreateAndPatchAll(typeof(Patches));
		}

		internal static void Start()
		{
			AddCommand("hack_stars", "[amount]", (args) =>
			{
				int amount = 100;
				if (args.Length > 0)
				{
					int.TryParse(args[0], out amount);
				}

				GameManager.LocalPlayer.Currency += amount;

				DebugConsole.Write($"+{amount} stars");
			});
			AddCommand("map_set", "(path)", (args) =>
			{
				if (args.Length == 0)
				{
					DebugConsole.Write("Wrong args!");
					return;
				}

				MapEditor.map = JObject.Parse(File.ReadAllText(args[0]));

				DebugConsole.Write($"Map set");
			});
			AddCommand("map_unset", "", (args) =>
			{
				MapEditor.map = null;

				DebugConsole.Write($"Map unset");
			});
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

		internal static void AddCommand(string name, string description, Action<Il2CppStringArray> container)
		{
			DebugConsole.AddCommand(name, DelegateSupport.ConvertDelegate<DebugConsole.CommandDelegate>(container), description);
		}
	}
}
