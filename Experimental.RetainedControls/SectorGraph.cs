using System.Numerics;

/// <summary>
/// A pie-chart-like model which detects in which slice of the pie the mouse is.
/// </summary>
public class SectorGraph
{
	public int SectorCount => directions.Length;
	protected IList<Vector2> Directions => directions;
	protected Vector2 DirectionAt(int index) => directions[index];
	protected Vector2 Center { get; private set; }

	private readonly Vector2[] directions;

	public SectorGraph(int sectorCount)
	{
		directions = new Vector2[sectorCount];
	}

	public static SectorGraph FromCircle(int sectorCount)
	{
		var sectorGraph = new SectorGraph(sectorCount);

		float sectorAngle = (float)(2 * Math.PI) / sectorCount;

		for (int i = 0; i < sectorCount; i++)
		{
			float radians = i * sectorAngle;
			float cos = (float)Math.Cos(radians);
			float sin = (float)Math.Sin(radians);

			sectorGraph.directions[i] = new Vector2(cos, sin);
		}

		return sectorGraph;
	}

	public virtual void UpdateDirectionsFromPoints(Vector2 center, params Vector2[] points)
	{
		Center = center;
		for (int i = 0; i < points.Length; i++)
		{
			directions[i] = Vector2.Normalize(points[i] - center);
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