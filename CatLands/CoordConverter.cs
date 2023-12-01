namespace CatLands;

using System.ComponentModel;
using System.Globalization;

// Since dictionary keys in json are always strings, we must use this instead of the JsonConverter.
public class CoordConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
	{
		return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}

	public override object? ConvertFrom(
		ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		return value is string s && s.Contains('|')
			? Coord.Parse(s)
			: base.ConvertFrom(context, culture, value);
	}

	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		return destinationType == typeof(string) && value is Coord coord
			? coord.ToString()
			: base.ConvertTo(context, culture, value, destinationType);
	}
}