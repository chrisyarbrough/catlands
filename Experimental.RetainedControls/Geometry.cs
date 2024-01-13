using System.Numerics;

public static class Geometry
{
	/// <summary>
	/// Projects a point onto an infinite line defined by two points.
	/// </summary>
	public static Vector2 ClosestPointOnLine(Vector2 point, Vector2 linePointA, Vector2 linePointB)
	{
		Vector2 line = linePointB - linePointA;
		Vector2 normalizedDirection = line / line.Length();
		Vector2 cornerToMouse = point - linePointA;

		float projectedLength = Vector2.Dot(cornerToMouse, normalizedDirection);
		Vector2 projectedVector = projectedLength * normalizedDirection;

		return linePointA + projectedVector;
	}
}

public readonly record struct Line(Vector2 P0, Vector2 P1)
{
	public Vector2 Vector => P1 - P0;
	public Vector2 Center => (P0 + P1) / 2f;
	public Vector2 ClosestPointTo(Vector2 point) => Geometry.ClosestPointOnLine(point, P0, P1);
}