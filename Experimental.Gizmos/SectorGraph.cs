namespace Experimental.Gizmos;

using System.Numerics;

/// <summary>
/// A pie-chart-like model which detects in which slice of the pie the mouse is.
/// </summary>
public class SectorGraph
{
	public int SectorCount => directions.Length;

	protected IList<Vector2> Directions => directions;
	protected Vector2 Center { get; private set; }

	private readonly Vector2[] directions;

	public SectorGraph(int sectorCount)
	{
		directions = new Vector2[sectorCount];
	}

	public virtual void UpdateDirectionsFromPoints(Vector2 center, params Vector2[] points)
	{
		Center = center;
		for (int i = 0; i < points.Length; i++)
		{
			directions[i] = Vector2.Normalize(points[i] - center);
		}
	}

	public virtual (int index, float dot) FindSectorIndex(Vector2 point)
	{
		Vector2 direction = Vector2.Normalize(point - Center);
		return Enumerable.Range(0, directions.Length)
			.Select(i => (
				index: i,
				dot: Vector2.Dot(direction, directions[i])))
			.MaxBy(item => item.dot);
	}
}