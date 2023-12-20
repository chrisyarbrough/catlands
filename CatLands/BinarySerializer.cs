// using System.Text;
//
// namespace CatLands;
//
// public class BinarySerializer : ISerializer
// {
//	public string FileExtension => ".bin";
//
// 	public void Serialize(Map map, Stream stream)
// 	{
// 		using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
//
// 		writer.Write(map.FileVersion);
//
// 		writer.Write(map.Tilesets.Length);
// 		foreach (string tileSet in map.Tilesets)
// 			writer.Write(tileSet);
//
// 		writer.Write(map.Tiles.Count);
// 		foreach (Tile tile in map.Tiles)
// 		{
// 			writer.Write(tile.Id);
// 			writer.Write(tile.TilesetId);
// 			writer.Write(tile.Coord.X);
// 			writer.Write(tile.Coord.Y);
// 		}
// 	}
//
// 	public Map Deserialize(Stream stream)
// 	{
// 		using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
//
// 		var map = new Map();
// 		map.FileVersion = reader.ReadInt32();
//
// 		int tilesetCount = reader.ReadInt32();
// 		map.Tilesets = new string[tilesetCount];
// 		for (int i = 0; i < tilesetCount; i++)
// 		{
// 			map.Tilesets[i] = reader.ReadString();
// 		}
//
// 		int tileCount = reader.ReadInt32();
// 		map.Tiles = new List<Tile>(tileCount);
// 		for (int i = 0; i < tileCount; i++)
// 		{
// 			var tile = new Tile
// 			{
// 				Id = reader.ReadInt32(),
// 				TilesetId = reader.ReadInt32(),
// 				Coord = new Coord
// 				{
// 					X = reader.ReadInt32(),
// 					Y = reader.ReadInt32()
// 				}
// 			};
// 			map.Tiles.Add(tile);
// 		}
//
// 		return map;
// 	}
// }