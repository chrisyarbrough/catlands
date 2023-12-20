namespace CatLands;

using System.Numerics;

[Serializable]
public struct Rect : IEquatable<Rect>
{
	private float x;
	private float y;
	private float width;
	private float height;

	public Rect(float x, float y, float width, float height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public static Rect FromPoints(float left, float top, float right, float bottom) =>
		new(left, top, right - left, bottom - top);

	public static Rect FromPoints(Vector2 a, Vector2 b)
	{
		float minX = Math.Min(a.X, b.X);
		float minY = Math.Min(a.Y, b.Y);
		float maxX = Math.Max(a.X, b.X);
		float maxY = Math.Max(a.Y, b.Y);

		return new Rect(
			minX,
			minY,
			maxX - minX,
			maxY - minY
		);
	}

	public float X
	{
		readonly get => x;
		set => x = value;
	}

	public float Y
	{
		readonly get => y;
		set => y = value;
	}

	public float Width
	{
		readonly get => width;
		set => width = value;
	}

	public float Height
	{
		readonly get => height;
		set => height = value;
	}

	public readonly float Left => width > 0f ? x : x + width;
	public readonly float Top => height > 0f ? y : y + height;
	public readonly float Right => x + width;
	public readonly float Bottom => y + height;

	public readonly Vector2 TopLeft => new(Left, Top);
	public readonly Vector2 TopRight => new(Right, Top);
	public readonly Vector2 BottomLeft => new(Left, Bottom);
	public readonly Vector2 BottomRight => new(Right, Bottom);

	public Vector2 Size => new(width, height);

	public readonly Vector2 Min => new(Left, Top);
	public readonly Vector2 Max => new(Right, Bottom);

	public Vector2 Position
	{
		readonly get => new(x, y);
		set
		{
			x = value.X;
			y = value.Y;
		}
	}

	public readonly Vector2 Center => new(x + width / 2f, y + height / 2f);

	public readonly float Area => width * height;

	public readonly bool Contains(float x, float y) => X <= x && x < X + Width && Y <= y && y < Y + Height;

	public readonly bool Contains(Vector2 point) => Contains(point.X, point.Y);

	public readonly bool IntersectsWith(Rect rect) =>
		rect.X < X + Width &&
		X < rect.X + rect.Width &&
		rect.Y < Y + Height &&
		Y < rect.Y + rect.Height;

	public readonly bool Encloses(Rect other) => Contains(other.Min) && Contains(other.Max);

	public Rect Inflate(float amount) => Inflate(amount, amount);
	
	public Rect Inflate(float xAmount, float yAmount)
	{
		X -= xAmount;
		Y -= yAmount;
		Width += 2 * xAmount;
		Height += 2 * yAmount;
		return this;
	}

	public bool Equals(Rect other)
	{
		return Approximately(x, other.X) &&
		       Approximately(y, other.Y) &&
		       Approximately(width, other.Width) &&
		       Approximately(height, other.Height);
	}

	private static bool Approximately(float a, float b)
	{
		// Use a relative epsilon that attempts to scale with the input.
		return Math.Abs(b - a) < Math.Max(0.000001f * Math.Max(Math.Abs(a), Math.Abs(b)), float.Epsilon * 8);
	}

	public readonly override string ToString() => $"{{X={X},Y={Y},Width={Width},Height={Height}}}";

	public override bool Equals(object? obj) => obj is Rect other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(x, y, width, height);

	public static bool operator ==(Rect left, Rect right) => left.Equals(right);

	public static bool operator !=(Rect left, Rect right) => !left.Equals(right);

	public static implicit operator Raylib_cs.Rectangle(Rect rect) => new(rect.X, rect.Y, rect.Width, rect.Height);

	public static implicit operator Rect(Raylib_cs.Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);
}