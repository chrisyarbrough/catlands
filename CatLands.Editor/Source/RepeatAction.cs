namespace CatLands.Editor;

public class RepeatAction
{
	private readonly float initialDelay;
	private readonly float repeatInterval;

	private float elapsedTime;
	private bool isActive;

	public RepeatAction(float initialDelay = 0.5f, float repeatInterval = 0.1f)
	{
		this.initialDelay = initialDelay;
		this.repeatInterval = repeatInterval;
	}

	public void Begin()
	{
		isActive = true;
		elapsedTime = 0f;
	}

	public bool Update(float deltaTime)
	{
		if (!isActive)
			return false;

		elapsedTime += deltaTime;

		if (elapsedTime > initialDelay)
		{
			if ((elapsedTime - initialDelay) % repeatInterval < deltaTime)
			{
				return true;
			}
		}

		return false;
	}

	public void End()
	{
		isActive = false;
	}
}