namespace CatLands;

public class Scene : GameObject
{
	public static Scene Current;

	public Scene()
	{
		Current = this;
	}
}