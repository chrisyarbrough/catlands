namespace CatLands.Editor;

public class AddLayerCommand : Command
{
	private readonly Map map;
	private readonly string textureId;
	private int addedLayerId;

	public AddLayerCommand(Map map, string textureId)
	{
		this.map = map;
		this.textureId = textureId;
	}

	public override void Do()
	{
		addedLayerId = map.AddLayer(new Layer(textureId));
	}

	public override void Undo()
	{
		map.RemoveLayer(addedLayerId);
	}
}

public class MapEditCommand : Command
{
	private readonly Map map;
	private readonly int layerId;
	private readonly Coord coord;
	private readonly int tileId;

	private int? originalTileId;

	public MapEditCommand(Map map, int layerId, Coord coord, int tileId)
	{
		this.map = map;
		this.layerId = layerId;
		this.coord = coord;
		this.tileId = tileId;
	}

	public override void Do()
	{
		Layer layer = map.GetLayer(layerId);
		if (layer.TryGetTileId(coord, out int existingTileId))
			originalTileId = existingTileId;
		layer.SetTileId(coord, tileId);
	}

	public override void Undo()
	{
		Layer layer = map.GetLayer(layerId);
		if (originalTileId.HasValue)
			layer.SetTileId(coord, originalTileId.Value);
		else
			layer.RemoveTile(coord);
	}
}