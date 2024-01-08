using System.Numerics;

/// <summary>
/// A pie-chart-like model which detects in which slice of the pie the mouse is.
/// </summary>
public class SectorGraph
{
	public int SectorCount => directions.Length;
	protected IEnumerable<Vector2> Directions => directions;
	protected Vector2 DirectionAt(int index) => directions[index];

	private readonly Vector2[] directions;

	public SectorGraph(int sectorCount)
	{
		directions = new Vector2[sectorCount];
		float sectorAngle = (float)(2 * Math.PI) / sectorCount;

		for (int i = 0; i < sectorCount; i++)
		{
			float radians = i * sectorAngle;
			float cos = (float)Math.Cos(radians);
			float sin = (float)Math.Sin(radians);

			directions[i] = new Vector2(cos, sin);
		}
	}

	public virtual (int index, float dot) FindSectorIndex(Vector2 direction)
	{
		return Enumerable.Range(0, directions.Length)
			.Select(i => (
				index: i,
				dot: Vector2.Dot(direction, directions[i])))
			.MaxBy(item => item.dot);
	}
}