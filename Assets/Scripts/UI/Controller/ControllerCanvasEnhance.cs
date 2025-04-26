using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasEnhance : ControllerCanvas
    {
        private ViewCanvasEnhance View => ViewCanvas as ViewCanvasEnhance;
        private IEnhanceable _enhanceable;

        public ControllerCanvasEnhance(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasEnhance>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.EnhanceButton.OnClick.AddListener(() =>
            {
                var cost = DataController.Instance.enhancement.GetEnhanceCostValue(_enhanceable);
                var toastMessage = Get<ControllerCanvasToastMessage>();

                if (DataController.Instance.enhancement.IsMaxLevel(_enhanceable))
                {
                    toastMessage.ShowTransientToastMessage(LocalizedTextType.EnhanceNoMore);
                    return;
                }

                if (DataController.Instance.good.TryConsume(cost.Key, cost.Value))
                {
                    var isSuccess = DataController.Instance.enhancement.TryEnhance(_enhanceable);
                    Effect(isSuccess);
                }
            });

             DataController.Instance.enhancement.OnBindEnhanced += UpdatePrice;
             DataController.Instance.enhancement.OnBindEnhanced += UpdateProbability;
             DataController.Instance.enhancement.OnBindEnhanced += UpdateEnhancementLevel;
             DataController.Instance.enhancement.OnBindEnhanced += UpdateEnhanceButtonView;
        }

        public void Open(IEnhanceable enhanceable)
        {
            if (enhanceable == null) return;
            base.Open();
            View.ViewSlotUI.EffectFront.enabled = false;
            UpdateEnhancementView(enhanceable);
            UpdateEnhanceButtonView(enhanceable);
        }
        
        private void UpdateEnhancementView(IEnhanceable enhanceable)
        {
            _enhanceable = enhanceable;
            View.ViewSlotUI
                .SetUnitSprite(enhanceable.IconSprite)
                .SetGradeText(enhanceable.GradeType)
                .SetActiveEquipBorder(enhanceable.EquippedIndex);

            UpdatePrice(enhanceable);
            UpdateProbability(enhanceable);
            UpdateEnhancementLevel(enhanceable);
        }

        private void UpdatePrice(IEnhanceable enhanceable)
        {
            var cost = DataController.Instance.enhancement.GetEnhanceCostValue(enhanceable);
            View.ViewGood
                .SetInit(cost.Key)
                .SetValue(cost.Value);
        }
        
        private void UpdateProbability(IEnhanceable enhanceable)
        {
            var probability = DataController.Instance.enhancement.GetProbability(enhanceable);
            View.SetEnhanceProbabilityText(LocalizeManager.GetText(LocalizedTextType.EnhanceProbability, probability));
        }
        
        private void UpdateEnhancementLevel(IEnhanceable enhanceable)
        {
            View.ViewSlotUI
                .SetActiveEnhancementBorder(enhanceable.EnhancementLevel > 0)
                .SetEnhancementLevel(enhanceable.EnhancementLevel);
        }
        
        private void UpdateEnhanceButtonView(IEnhanceable enhanceable)
        {
            View.EnhanceButton.Selected(!DataController.Instance.enhancement.IsMaxLevel(enhanceable));
        }

        private void Effect(bool isSuccess)
        {
            View.ViewSlotUI.EffectFront.DOKill();
            
            var color = View.GetEffectColor(isSuccess);
            color.a = 0;
            
            View.ViewSlotUI.EffectFront.enabled = true;   
            View.ViewSlotUI.EffectFront.color = color;
            View.ViewSlotUI.EffectFront.DOFade(1, 0.25f).SetLoops(2, LoopType.Yoyo);
        }
    }
}