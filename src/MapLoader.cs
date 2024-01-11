using Newtonsoft.Json.Linq;
using Polytopia.Data;

namespace PolyMod
{
	internal static class MapLoader
	{
		internal static JObject? map;

		internal static Il2CppSystem.Collections.Generic.List<int> GetCapitals(Il2CppSystem.Collections.Generic.List<int> originalCapitals, int width, int playerCount)
		{
			if (map == null || map["capitals"] == null)
			{
				return originalCapitals;
			}

			JArray jcapitals = map["capitals"].Cast<JArray>();
			Il2CppSystem.Collections.Generic.List<int> capitals = new();
			for (int i = 0; i < jcapitals.Count; i++)
			{
				capitals.Add((int)jcapitals[i]);
			}

			if (capitals.Count < originalCapitals.Count)
			{
				throw new Exception("Too few capitals provided");
			}
			return capitals.GetRange(0, originalCapitals.Count);
		}

		internal static void PreGenerate(ref GameState state, ref MapGeneratorSettings settings)
		{
			if (map == null)
			{
				return;
			}
			ushort size = (ushort)map["size"];

			if (size < Plugin.MAP_MIN_SIZE || size > Plugin.MAP_MAX_SIZE)
			{
				throw new Exception($"The map size must be between {Plugin.MAP_MIN_SIZE} and {Plugin.MAP_MAX_SIZE}");
			}
			state.Map = new(size, size);
			settings.mapType = PolytopiaBackendBase.Game.MapPreset.Dryland;
		}

		internal static void PostGenerate(ref GameState state)
		{
			if (map == null)
			{
				return;
			}
			MapData originalMap = state.Map;

			for (int i = 0; i < originalMap.tiles.Length; i++)
			{
				TileData tile = originalMap.tiles[i];
				JToken tileJson = map["map"][i];

				if (tileJson["skip"] != null && (bool)tileJson["skip"]) continue;
				if (tile.rulingCityCoordinates != tile.coordinates)
				{
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
				}

				tile.climate = (tileJson["climate"] == null || (int)tileJson["climate"] < 0 || (int)tileJson["climate"] > 16) ? 0 : (int)tileJson["climate"];
				tile.skinType = tileJson["skinType"] == null ? SkinType.Default : EnumCache<SkinType>.GetType((string)tileJson["skinType"]);
				tile.terrain = tileJson["terrain"] == null ? TerrainData.Type.None : EnumCache<TerrainData.Type>.GetType((string)tileJson["terrain"]);
				tile.resource = tileJson["resource"] == null ? null : new() { type = EnumCache<ResourceData.Type>.GetType((string)tileJson["resource"]) };

				if (tile.rulingCityCoordinates == tile.coordinates && map["autoTribe"] != null && (bool)map["autoTribe"])
				{
					state.TryGetPlayer(tile.owner, out PlayerState player);
					if (player == null)
					{
						throw new Exception($"Player {tile.owner} does not exist");
					}
					switch (tile.climate)
					{
						case 1:
							player.tribe = TribeData.Type.Xinxi;
							break;
						case 2:
							player.tribe = TribeData.Type.Imperius;
							break;
						case 3:
							player.tribe = TribeData.Type.Bardur;
							break;
						case 4:
							player.tribe = TribeData.Type.Oumaji;
							break;
						case 5:
							player.tribe = TribeData.Type.Kickoo;
							break;
						case 6:
							player.tribe = TribeData.Type.Hoodrick;
							break;
						case 7:
							player.tribe = TribeData.Type.Luxidoor;
							break;
						case 8:
							player.tribe = TribeData.Type.Vengir;
							break;
						case 9:
							player.tribe = TribeData.Type.Zebasi;
							break;
						case 10:
							player.tribe = TribeData.Type.Aimo;
							break;
						case 11:
							player.tribe = TribeData.Type.Aquarion;
							break;
						case 12:
							player.tribe = TribeData.Type.Quetzali;
							break;
						case 13:
							player.tribe = TribeData.Type.Elyrion;
							break;
						case 14:
							player.tribe = TribeData.Type.Yadakk;
							break;
						case 15:
							player.tribe = TribeData.Type.Polaris;
							break;
						case 16:
							player.tribe = TribeData.Type.Cymanti;
							break;
					}
				}

				switch (tile.terrain)
				{
					case TerrainData.Type.Water:
						tile.altitude = -1;
						break;
					case TerrainData.Type.Ocean:
					case TerrainData.Type.Ice:
						tile.altitude = -2;
						break;
					case TerrainData.Type.Field:
					case TerrainData.Type.Forest:
						tile.altitude = 1;
						break;
					case TerrainData.Type.Mountain:
						tile.altitude = 2;
						break;
				}

				originalMap.tiles[i] = tile;
			}

			map = null;
		}
	}
}
