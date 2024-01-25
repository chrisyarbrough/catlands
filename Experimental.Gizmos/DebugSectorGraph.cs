namespace Experimental.Gizmos;

using System.Numerics;
using Raylib_cs;

internal class DebugSectorGraph : SectorGraph
{
	private Vector2[] points;
	private int fontSize = 12;
	private const int minFontSize = 4;

	public DebugSectorGraph(int sectorCount) : base(sectorCount)
	{
	}

	public override void UpdateDirectionsFromPoints(Vector2 center, params Vector2[] points)
	{
		this.points = points;
		float minDistance = points.Select(p => Vector2.Distance(center, p)).Min();
		fontSize = (int)(minDistance / 10f);
		base.UpdateDirectionsFromPoints(center, points);
	}

	public override (int index, float dot) FindSectorIndex(Vector2 point)
	{
		Vector2 direction = Vector2.Normalize(point - Center);

		for (int i = 0; i < Directions.Count; i++)
		{
			Vector2 sectorDirection = Directions[i];
			Vector2 sectorPoint = points[i];
			float dot = Vector2.Dot(direction, sectorDirection);
			Raylib.DrawLineV(Center, sectorPoint, Color.GRAY);

			if (fontSize >= minFontSize)
			{
				Vector2 textPosition = (Center + sectorPoint) / 2;
				Raylib.DrawText(dot.ToString("0.00"), (int)textPosition.X, (int)textPosition.Y, fontSize, Color.WHITE);
			}
		}

		Raylib.DrawLineV(Center, point, Color.GREEN);

		(int sector, float _) r = base.FindSectorIndex(point);
		if (fontSize * 2 >= minFontSize)
			Raylib.DrawText(r.sector.ToString(), (int)Center.X, (int)Center.Y, fontSize * 2, Color.WHITE);

		return r;
	}
}