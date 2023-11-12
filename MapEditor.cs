using Newtonsoft.Json.Linq;
using Polytopia.Data;

namespace PolyMod
{
	internal static class MapEditor
	{
		internal static string mapPath = File.ReadAllText(BepInEx.Paths.BepInExRootPath + "/map.json"); //TODO: file open dialog

		internal static void PreGenerate(ref GameState state)
		{
			JObject json = JObject.Parse(mapPath);
			ushort size = (ushort)json["size"];
			state.Map = new(size, size);
			//TODO: edit players number
		}

		internal static void PostGenerate(ref GameState state)
		{
			JObject json = JObject.Parse(mapPath);
			MapData map = state.Map;

			for (int i = 0; i < map.tiles.Length; i++)
			{
				TileData tile = map.tiles[i];
				JToken tileJson = json["map"][i];

				tile.climate = (tileJson["climate"] == null || (int)tileJson["climate"] < 0 || (int)tileJson["climate"] > 16) ? 0 : (int)tileJson["climate"];

				if (tile.rulingCityCoordinates != WorldCoordinates.NULL_COORDINATES) continue;

				tile.terrain = tileJson["terrain"] == null ? TerrainData.Type.None : EnumCache<TerrainData.Type>.GetType((string)tileJson["terrain"]);
				tile.improvement = tileJson["improvement"] == null ? null : new() { type = EnumCache<ImprovementData.Type>.GetType((string)tileJson["improvement"]) };
				if (tile.improvement != null && tile.improvement.type == ImprovementData.Type.City)
				{
					tile.improvement = new ImprovementState
					{
						type = ImprovementData.Type.City,
						founded = 0,
						level = 1,
						borderSize = 1,
						production = 1
					};
				}
				tile.resource = tileJson["resource"] == null ? null : new() { type = EnumCache<ResourceData.Type>.GetType((string)tileJson["resource"]) };

				map.tiles[i] = tile;
			}
		}
	}
}
