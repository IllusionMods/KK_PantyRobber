using System.Reflection;

namespace KK_PantyRobber
{
    internal static class ReflectionHelper
    {
        internal static T GetValue<T>(this FieldInfo info, object instance)
        {
            return (T) info.GetValue(instance);
        }
    }
}