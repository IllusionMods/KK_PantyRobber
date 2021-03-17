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
            if (ChaControl.sex == 0)
                ExtendedSave.SetExtendedDataById(ChaFileControl, PantyRobber.GUID, Data.Save());
            else
                ExtendedSave.SetExtendedDataById(ChaFileControl, PantyRobber.GUID, null);
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            SaveData();
        }

        protected override void OnReload(GameMode currentGameMode)
        {
            //IL_0000: Unknown result type (might be due to invalid IL or missing references)
            //IL_0002: Invalid comparison between Unknown and I4
            if ((int) currentGameMode == 3 && ChaControl.sex == 0) ReadData();
        }
    }
}