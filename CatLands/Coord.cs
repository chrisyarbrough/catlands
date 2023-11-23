namespace CatLands;

using System.ComponentModel;
using System.Globalization;

[TypeConverter(typeof(CoordConverter))]
public readonly struct Coord
{
	public readonly int X;
	public readonly int Y;

	public Coord(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return $"{X}|{Y}";
	}

	public static Coord Parse(string s)
	{
		string[] split = s.Split("|");
		return new Coord(
			x: int.Parse(split[0]),
			y: int.Parse(split[1]));
	}

	public bool Equals(Coord other)
	{
		return X == other.X && Y == other.Y;
	}

	public override bool Equals(object? obj)
	{
		return obj is Coord other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y);
	}

	public static bool operator ==(Coord a, Coord b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Coord a, Coord b)
	{
		return !a.Equals(b);
	}
}

// Since dictionary keys in json are always strings, we must use this instead of the JsonConverter.
public class CoordConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		return value is string s && s.Contains("|") ? Coord.Parse(s) : base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context,
		CultureInfo culture,
		object value,
		Type destinationType)
	{
		return destinationType == typeof(string) && value is Coord t
			? t.ToString()
			: base.ConvertTo(context, culture, value, destinationType);
	}
}