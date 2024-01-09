using System.Diagnostics;
using System.Numerics;
using Raylib_cs;

public class MouseCursors
{
	private readonly MouseCursor[] cursors =
	{
		MouseCursor.MOUSE_CURSOR_RESIZE_EW,
		MouseCursor.MOUSE_CURSOR_RESIZE_NWSE,
		MouseCursor.MOUSE_CURSOR_RESIZE_NS,
		MouseCursor.MOUSE_CURSOR_RESIZE_NESW,
	};

	private readonly SectorGraph sectorGraph;

	public MouseCursors(SectorGraph sectorGraph)
	{
		this.sectorGraph = sectorGraph;
		Debug.Assert(sectorGraph.SectorCount == 8);
	}

	public void UpdateDirectionsFromPoints(Vector2 center, params Vector2[] points)
	{
		sectorGraph.UpdateDirectionsFromPoints(center, points);
	}

	public MouseCursor GetCursor(Vector2 mousePosition)
	{
		(int index, float _) = sectorGraph.FindSectorIndex(mousePosition);
		return cursors[index % cursors.Length];
	}
}