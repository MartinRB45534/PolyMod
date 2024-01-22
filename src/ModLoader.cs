using Cpp2IL.Core.Extensions;
using I2.Loc;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Linq;
using Newtonsoft.Json.Linq;
using Polytopia.Data;
using System.IO.Compression;
using UnityEngine;

namespace PolyMod
{
	internal static class ModLoader
	{
		internal static void Init(JObject gld)
		{
			foreach (string modname in Directory.GetFiles(Plugin.MODS_PATH, "*.polymod"))
			{
				ZipArchive mod = new(File.OpenRead(modname));

				foreach (var entry in mod.Entries)
				{
					string name = entry.ToString();
					Stream stream = entry.Open();

					if (Path.GetExtension(name) == ".png")
					{
						
					}
				}

				ZipArchiveEntry? patch = mod.GetEntry("patch.json");
				if (patch != null) 
				{
					Patch(gld, JObject.Parse(new StreamReader(patch.Open()).ReadToEnd()));
				}
			}
		}

		private static void Patch(JObject gld, JObject patch)
		{
			int idx = 0;

			foreach (JToken jtoken in patch.SelectTokens("$.localizationData.*").ToArray())
			{
				JArray token = jtoken.Cast<JArray>();
				TermData term = LocalizationManager.Sources[0].AddTerm(Plugin.GetJTokenName(token).Replace('_', '.'));
				List<string> strings = new();
				for (int i = 0; i < token.Count; i++)
				{
					strings.Add((string)token[i]);
				}
				term.Languages = new Il2CppStringArray(strings.ToArray());
			}
			patch.Remove("localizationData");

			foreach (JToken jtoken in patch.SelectTokens("$.*.*").ToArray())
			{
				JObject token = jtoken.Cast<JObject>();

				if (token["idx"] != null && (int)token["idx"] == -1)
				{
					token["idx"] = --idx;
					string id = Plugin.GetJTokenName(token);
					switch (Plugin.GetJTokenName(token, 2))
					{
						case "tribeData":
							EnumCache<TribeData.Type>.AddMapping(id, (TribeData.Type)idx);
							break;
						case "terrainData":
							EnumCache<Polytopia.Data.TerrainData.Type>.AddMapping(id, (Polytopia.Data.TerrainData.Type)idx);
							break;
						case "resourceData":
							EnumCache<ResourceData.Type>.AddMapping(id, (ResourceData.Type)idx);
							PrefabManager.resources.TryAdd((ResourceData.Type)idx, PrefabManager.resources[ResourceData.Type.Game]);
							break;
						case "taskData":
							EnumCache<TaskData.Type>.AddMapping(id, (TaskData.Type)idx);
							break;
						case "improvementData":
							EnumCache<ImprovementData.Type>.AddMapping(id, (ImprovementData.Type)idx);
							PrefabManager.improvements.TryAdd((ImprovementData.Type)idx, PrefabManager.improvements[ImprovementData.Type.CustomsHouse]);
							break;
						case "unitData":
							EnumCache<UnitData.Type>.AddMapping(id, (UnitData.Type)idx);
							PrefabManager.units.TryAdd((UnitData.Type)idx, PrefabManager.units[UnitData.Type.Scout]);
							break;
						case "techData":
							EnumCache<TechData.Type>.AddMapping(id, (TechData.Type)idx);
							break;
					}
				}
			}

			gld.Merge(patch);
		}

		private static Sprite BuildSprite(byte[] data)
		{
			Texture2D texture = new(1, 1);
			texture.LoadImage(data);
			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 2112);
		}
	}
}
