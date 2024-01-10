using DG.Tweening.Plugins;
using HarmonyLib;
using I2.Loc;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Linq;
using Newtonsoft.Json.Linq;
using Polytopia.Data;
using System.IO.Compression;

namespace PolyMod
{
	internal static class ModLoader
	{
		internal static void Init(JObject gld)
		{
			int idx = 0;

			foreach (string path in Directory.GetFiles(Plugin.MODS_PATH, "*.polymod"))
			{
				foreach (var entry in new ZipArchive(File.OpenRead(path)).Entries)
				{
					if (entry.ToString() == "patch.json")
					{
						JObject patch = JObject.Parse(new StreamReader(entry.Open()).ReadToEnd());

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
										EnumCache<TerrainData.Type>.AddMapping(id, (TerrainData.Type)idx);
										break;
									case "resourceData":
										EnumCache<ResourceData.Type>.AddMapping(id, (ResourceData.Type)idx);
										break;
									case "taskData":
										EnumCache<TaskData.Type>.AddMapping(id, (TaskData.Type)idx);
										break;
									case "improvementData":
										EnumCache<ImprovementData.Type>.AddMapping(id, (ImprovementData.Type)idx);
										break;
									case "unitData":
										EnumCache<UnitData.Type>.AddMapping(id, (UnitData.Type)idx);
										break;
									case "techData":
										EnumCache<TechData.Type>.AddMapping(id, (TechData.Type)idx);
										break;
								}
							}
						}

						gld.Merge(patch);
					}
				}
			}
		}
	}
}
