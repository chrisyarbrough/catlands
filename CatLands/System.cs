namespace CatLands;

using Raylib_cs;

public interface ISystem
{
	public virtual void Update(float deltaTime)
	{
	}

	public virtual void Draw(Camera2D camera2D)
	{
	}
}