using ExtensibleSaveFormat;

namespace KK_PantyRobber
{
    public sealed class PantyData
    {
        public int StealLevel = -1;

        public static PantyData Load(PluginData data)
        {
            PantyRobber.Log("Load");
            if (data?.data == null) return new PantyData();
            foreach (var item in data.data) PantyRobber.Log($"- {item.Key}={item.Value}");
            var pantyData = new PantyData();
            if (data.data.TryGetValue(nameof(StealLevel), out var value)) pantyData.StealLevel = (int) value;
            return pantyData;
        }

        public PluginData Save()
        {
            var val = new PluginData();
            val.version = 1;
            val.data[nameof(StealLevel)] = StealLevel;
            return val;
        }
    }
}