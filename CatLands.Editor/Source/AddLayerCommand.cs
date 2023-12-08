namespace CatLands.Editor;

public class AddLayerCommand : Command
{
	public int AddedLayerId { get; private set; }

	private readonly Map map;
	private readonly string textureId;

	public AddLayerCommand(Map map, string textureId)
	{
		this.map = map;
		this.textureId = textureId;
	}

	public override void Do()
	{
		AddedLayerId = map.AddLayer(new Layer(textureId));
	}

	public override void Undo()
	{
		map.RemoveLayer(AddedLayerId);
	}
}