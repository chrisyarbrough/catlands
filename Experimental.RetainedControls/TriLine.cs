using System.Numerics;
using Raylib_cs;

// https://github.com/petuzk/raylib-line-triangulator
internal sealed class TriLine
{
	public float Thickness
	{
		get => thickness;
		set => thickness = Math.Max(0, value);
	}

	private float thickness;

	private readonly Vector2[] points;
	private readonly Vector2[] strip;

	private const float EPSILON = 0.00001f;

	public TriLine(float thickness, int pointCount)
	{
		this.thickness = thickness;
		this.points = new Vector2[pointCount];
		this.strip = new Vector2[GetStripLength(pointCount, loop: false)];
	}

	public void UpdatePoint(int index, Vector2 point)
	{
		points[index] = point;
	}

	public void Triangulate()
	{
		TriangulateLine(points, points.Length, thickness, loop: false, strip);
	}

	public void Draw(Color color)
	{
		Raylib.DrawTriangleStrip(strip, strip.Length, color);
	}

	void TriangulateLine(Vector2[] points, int numPoints, float thickness, bool loop, Vector2[] strip)
	{
		if (numPoints == 2)
			loop = false;
		else if (numPoints < 2)
			return;

		thickness /= 2.0f;

		Vector2 A, O, B, p1, p2;
		int offset, reverseOrder;

		if (loop)
		{
			A = points[numPoints - 1];
			O = points[0];
		}
		else
		{
			A = points[0];
			O = points[1];
			FindPerpendiculars(A, O, thickness, out strip[0], out strip[1]);
		}

		for (int i = loop ? 0 : 1; i < numPoints - (loop ? 0 : 1); i++)
		{
			/* O is points[i], A is previous point and B is the next one.
			 * a = OA, b = OB, s = OX is a bisector.
			 *
			 *         O
			 *         ^
			 *        /|\
			 *      a/ |s\b
			 *      /__|__\
			 *     A   x   B
			 */
			B = points[(i + 1) % numPoints];

			Vector2 s;
			Vector2 a = Vector2.Subtract(A, O);
			Vector2 b = Vector2.Subtract(B, O);

			float len_b = b.Length();
			float slopeDiff = CrossProduct(a, b);

			if (-EPSILON < slopeDiff && slopeDiff < EPSILON)
			{
				float s_scale = thickness / len_b;
				s.X = -b.Y * s_scale;
				s.Y = b.X * s_scale;
			}
			else
			{
				float sides_len_ratio = a.Length() / len_b;

				Vector2 AB = Vector2.Subtract(B, A);
				Vector2 AX = Vector2.Multiply(AB, sides_len_ratio / (sides_len_ratio + 1));
				s = Vector2.Add(a, AX);

				float len_s = s.Length();
				float cos_b_s = Vector2.Dot(b, s) / len_b / len_s;
				float sin_b_s = (float)Math.Sqrt(1 - cos_b_s * cos_b_s);
				float s_scale = thickness / sin_b_s / len_s;
				s = Vector2.Multiply(s, s_scale);
			}

			p1 = Vector2.Add(O, s);
			p2 = Vector2.Subtract(O, s);

			offset = 2 * i;
			reverseOrder =
				(i > 0 && DoLinesIntersect(A, O, strip[offset - 2], p1)) ||
				(i == 0 && CrossProduct(b, s) > 0)
					? 1
					: 0;

			strip[offset + reverseOrder] = p1;
			strip[offset + (1 - reverseOrder)] = p2;

			A = O;
			O = B;
		}

		offset = GetStripLength(numPoints, loop) - 2;

		if (loop)
		{
			strip[offset + 0] = strip[0];
			strip[offset + 1] = strip[1];
		}
		else
		{
			FindPerpendiculars(points[numPoints - 1], points[numPoints - 2], thickness, out p1, out p2);
			reverseOrder = DoLinesIntersect(A, O, strip[offset - 2], p1) ? 1 : 0;

			strip[offset + reverseOrder] = p1;
			strip[offset + (1 - reverseOrder)] = p2;
		}
	}

	// Cross product of two points `a` and `b`
	float CrossProduct(Vector2 a, Vector2 b)
	{
		return a.X * b.Y - b.X * a.Y;
	}

	// Returns true if lines (`a1` - `a2`) and (`b1` - `b2`) intersect
	bool DoLinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
	{
		Vector2 a = Vector2.Subtract(a2, a1);
		Vector2 b = Vector2.Subtract(b2, b1);
		return (
			(
				// b1 and b2 are on the different sides of line defined by a1 and a2
				(CrossProduct(a, Vector2.Subtract(b1, a1)) > 0) ^
				(CrossProduct(a, Vector2.Subtract(b2, a1)) > 0)
			) && (
				// a1 and a2 are on the different sides of line defined by b1 and b2
				(CrossProduct(b, Vector2.Subtract(a1, b1)) > 0) ^
				(CrossProduct(b, Vector2.Subtract(a2, b1)) > 0)
			)
		);
	}

	// Finds `right` and `left` perpendiculars to vector pointing to `dir`.
	// Perpendiculars are scaled to have length `perp_len`.
	// All vectors are located at `center`.
	void FindPerpendiculars(Vector2 center, Vector2 dir, float perp_len, out Vector2 right, out Vector2 left)
	{
		Vector2 vec = Vector2.Subtract(dir, center);
		float scale = perp_len / vec.Length();
		left = Vector2.Add(center, new Vector2(-vec.Y * scale, vec.X * scale));
		right = Vector2.Add(center, new Vector2(vec.Y * scale, -vec.X * scale));
	}

	// Returns number of strip points needed to triangulate a line
	int GetStripLength(int numPoints, bool loop)
	{
		if (numPoints > 2)
			return 2 * (numPoints + (loop ? 1 : 0));
		else if (numPoints == 2)
			return 4; // ignore `loop`
		else
			return 0;
	}
}