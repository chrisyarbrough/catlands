using System.Numerics;
using Raylib_cs;

internal class DebugSectorGraph : SectorGraph
{
	public Vector2 Center;

	public DebugSectorGraph(int sectorCount) : base(sectorCount)
	{
	}

	public override (int index, float dot) FindSectorIndex(Vector2 direction)
	{
		foreach (Vector2 sectorDirection in Directions)
		{
			(int _, float dot) = base.FindSectorIndex(sectorDirection);
			Vector2 end = Center + sectorDirection * 50;
			Raylib.DrawLineV(Center, end, Color.GRAY);
			Raylib.DrawText(dot.ToString("0.0"), (int)end.X, (int)end.Y, 12, Color.WHITE);
		}

		int sector = base.FindSectorIndex(direction).index;
		Raylib.DrawText(sector.ToString(), (int)Center.X, (int)Center.Y, 24, Color.WHITE);
		Raylib.DrawLineV(Center, Center + DirectionAt(sector) * 50, Color.ORANGE);

		return base.FindSectorIndex(direction);
	}
}