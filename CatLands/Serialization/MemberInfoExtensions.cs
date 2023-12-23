namespace CatLands;

using System.Reflection;

internal static class MemberInfoExtensions
{
	public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
	{
		return memberInfo.GetCustomAttribute<T>() != null;
	}
}