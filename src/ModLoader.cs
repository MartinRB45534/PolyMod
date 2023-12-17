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
			Directory.CreateDirectory(Plugin.MODS_PATH);
			int idx = 0;

			foreach (string path in Directory.GetFiles(Plugin.MODS_PATH, "*.polymod"))
			{
				foreach (var entry in new ZipArchive(File.OpenRead(path)).Entries)
				{
					if (entry.ToString() == "patch.json")
					{
						JObject patch = JObject.Parse(new StreamReader(entry.Open()).ReadToEnd());

						foreach (JToken token in patch.SelectTokens("$.*.*").ToArray())
						{
							JObject jobject = token.Cast<JObject>();

							if (jobject["idx"] != null && (int)jobject["idx"] == -1)
							{
								jobject["idx"] = --idx;
								string id = Plugin.GetJTokenName(jobject);
								switch (Plugin.GetJTokenName(jobject, 2))
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
