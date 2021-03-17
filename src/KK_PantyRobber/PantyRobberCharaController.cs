using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;

namespace KK_PantyRobber
{
    public class PantyRobberCharaController : CharaCustomFunctionController
    {
        public PantyRobberCharaController()
        {
            Data = new PantyData();
        }

        public PantyData Data { get; private set; }

        public void ReadData()
        {
            var extendedDataById = ExtendedSave.GetExtendedDataById(ChaFileControl, PantyRobber.GUID);
            Data = PantyData.Load(extendedDataById);
        }

        public void SaveData()
        {
            ExtendedSave.SetExtendedDataById(ChaFileControl, PantyRobber.GUID, ChaControl.sex == 0 ? Data.Save() : null);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            SaveData();
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            if (currentGameMode == GameMode.MainGame && ChaControl.sex == 0) ReadData();
        }
    }
}