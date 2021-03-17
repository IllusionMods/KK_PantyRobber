using HarmonyLib;

namespace KK_PantyRobber
{
    internal static class Extensions
    {
        public static bool SetProperty(this object self, string name, object value)
        {
            return Traverse.Create(self).Property(name).SetValue(value).PropertyExists();
        }
    }
}