using I2.Loc;
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
				ZipArchive zip = new(File.OpenRead(path));
				foreach (var entry in zip.Entries)
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
								EnumCache<UnitData.Type>.AddMapping(Plugin.GetJTokenName(jobject), (UnitData.Type)idx);
							}
						}

						gld.Merge(patch);
					}
				}
			}
		}
	}
}
