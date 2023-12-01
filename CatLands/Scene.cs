namespace CatLands;

public class Scene : GameObject
{
	public static Scene Current { get; private set; } = new();

	public Scene()
	{
		Current = this;
	}
}