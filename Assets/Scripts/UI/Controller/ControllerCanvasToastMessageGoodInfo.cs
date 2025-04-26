using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasToastMessage
    {
        public ControllerCanvasToastMessage ShowGoodInfo(GoodType goodType, int param0)
        {
            View.ViewGoodInfo.SetActiveBackground(false);
            View.ViewGoodInfo.Button.enabled = false;
            
            var title = DataController.Instance.good.GetGoodInfoTitle(goodType);
            var desc = DataController.Instance.good.GetGoodInfoDescription(goodType);
            var sources = DataController.Instance.good.GetGoodInfoSources(goodType);
            var uasges = DataController.Instance.good.GetGoodInfoUsages(goodType);

            var sourcesText = "";
            if(sources[0] != LocalizedTextType.UpgradeTitle)
                sourcesText +=  $"- {LocalizeManager.GetText(LocalizedTextType.Source)} : {LocalizeManager.GetLocalizedTextConcat(sources, " / ")}";
            if(uasges[0] != LocalizedTextType.UpgradeTitle)
                sourcesText += $"\n- {LocalizeManager.GetText(LocalizedTextType.Usage)} : {LocalizeManager.GetLocalizedTextConcat(uasges, " / ")}";

            View
                .SetGoodInfoTitle(LocalizeManager.GetText(title))
                .SetGoodInfoDescription(LocalizeManager.GetText(desc))
                .SetGoodInfoSource(sourcesText);

            View.ViewGoodInfo.SetInit(goodType, param0).SetValue(DataController.Instance.good.GetValue(goodType), param0);

            SetActiveView(ToastType.GoodInfo, true);
            return this;
        }
    }
}