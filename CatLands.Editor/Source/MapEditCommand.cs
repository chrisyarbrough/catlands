namespace CatLands.Editor;

public class MapEditCommand : Command
{
	private readonly Map map;
	private readonly int layerId;
	private readonly IEnumerable<(Coord coord, int tileId)> changedTiles;

	private Dictionary<Coord, int?> originalTileIds = new();

	public MapEditCommand(Map map, int layerId, Coord coord, int tileId)
		: this(map, layerId, Enumerable.Repeat((coord, tileId), 1))
	{
	}

	public MapEditCommand(Map map, int layerId, IEnumerable<(Coord coord, int tileId)> changedTiles)
	{
		this.map = map;
		this.layerId = layerId;
		this.changedTiles = changedTiles;
	}

	public override void Do()
	{
		Layer layer = map.GetLayer(layerId);
		foreach ((Coord coord, int tileId) in changedTiles)
		{
			if (layer.TryGetTileId(coord, out int existingTileId))
				originalTileIds[coord] = existingTileId;
			else
				originalTileIds[coord] = null;

			layer.SetTileId(coord, tileId);
		}
	}

	public override void Undo()
	{
		Layer layer = map.GetLayer(layerId);
		foreach ((Coord coord, int? originalTileId) in originalTileIds)
		{
			if (originalTileId.HasValue)
				layer.SetTileId(coord, originalTileId.Value);
			else
				layer.RemoveTile(coord);
		}
	}
}