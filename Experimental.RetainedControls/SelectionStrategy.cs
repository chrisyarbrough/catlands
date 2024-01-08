using System.Numerics;
using Raylib_cs;

internal sealed class SelectionStrategy
{
	private static Vector2 mousePosition;

	public static Gizmo FindHoveredControl(Vector2 mousePosition, IEnumerable<Gizmo> gizmos)
	{
		SelectionStrategy.mousePosition = mousePosition;

		return FindHoveredGizmos(gizmos)
			.OrderBy(IsWithinParentRect)
			.ThenBy(DistanceToCenter)
			.ThenBy(Area)
			.FirstOrDefault();
	}

	private static IEnumerable<Gizmo> FindHoveredGizmos(IEnumerable<Gizmo> gizmos)
	{
		foreach (Gizmo gizmo in gizmos)
		{
			if (Raylib.CheckCollisionPointRec(mousePosition, (Rectangle)gizmo.Rect))
			{
				yield return gizmo;
			}
		}
	}

	/// <summary>
	/// Prefer smaller gizmos to avoid occlusion by the larger ones.
	/// </summary>
	private static float Area(Gizmo gizmo) => gizmo.Rect.Area;

	/// <summary>
	/// Allow switching between overlapping, similar handles by preferring the one closest to the mouse.
	/// </summary>
	private static float DistanceToCenter(Gizmo gizmo)
	{
		Vector2 center = gizmo.Rect.Center;
		return Vector2.DistanceSquared(mousePosition, new Vector2(center.X, center.Y));
	}

	/// <summary>
	/// If two identical handles overlap, prefer the one whose main gizmo contains the mouse.
	/// </summary>
	private static int IsWithinParentRect(Gizmo gizmo)
	{
		if (gizmo.Parent == null)
			return 0;

		return Raylib.CheckCollisionPointRec(mousePosition, (Rectangle)gizmo.Parent.Rect) ? -1 : 0;
	}
}