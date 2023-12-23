namespace CatLands;

using System.Reflection;

public static class MemberInfoExtensions
{
	public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
	{
		return memberInfo.GetCustomAttribute<T>() != null;
	}

	public static Type GetMemberType(this MemberInfo member) => member switch
	{
		PropertyInfo property => property.PropertyType,
		FieldInfo field => field.FieldType,
		_ => throw new ArgumentOutOfRangeException(nameof(member)),
	};
	
	public static bool CanWrite(this MemberInfo member) => member switch
	{
		PropertyInfo property => property.CanWrite,
		FieldInfo field => !field.IsInitOnly,
		_ => false,
	};
}