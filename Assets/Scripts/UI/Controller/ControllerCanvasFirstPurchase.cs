using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasFirstPurchase : ControllerCanvas
    {
        private ViewCanvasFirstPurchase View => ViewCanvas as ViewCanvasFirstPurchase;
        private readonly Reddot _reddot;

        public ControllerCanvasFirstPurchase(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasFirstPurchase>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            _reddot = new Reddot(ReddotType.FirstPurchased);
            View.GetRewardActiveButton.Button.onClick.AddListener(TryGetReward);

            DataController.Instance.shop.onBindPurchased += OnBindPurchase;

            var unlockLevel = DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.FirstPurchase);
            if (DataController.Instance.quest.currQuestLevel < unlockLevel)
                EnqueueOpenView(this, unlockLevel);
            
            UpdateView();
        }
        
        private void OnBindPurchase(ProductType type)
        {
            if (DataController.Instance.shop.IsFirstPurchased()
                && DataController.Instance.shop.CanReceiveFirstPurchaseReward())
                UpdateView();
        }

        private void TryGetReward()
        {
            if (DataController.Instance.shop.IsFirstPurchased()
                && DataController.Instance.shop.CanReceiveFirstPurchaseReward())
            {
                var rewardGoodType = DataController.Instance.shop.GetRewardGoodTypes(ProductType.FirstPurchaseReward)[0];
                var rewardValue = DataController.Instance.shop.GetRewardValues(ProductType.FirstPurchaseReward)[0];
                var param0 = DataController.Instance.shop.GetRewardParam0(ProductType.FirstPurchaseReward)[0];
                DataController.Instance.good.EarnReward(rewardGoodType, rewardValue, param0);
                DataController.Instance.shop.SetHasFirstPurchaseReward(true);

                UpdateView();
            }
            else
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.FirstPurchaseReward_Description);
            }
        }

        private void UpdateReddot()
        {
            _reddot.IsOn =
                DataController.Instance.shop.IsFirstPurchased()
                && DataController.Instance.shop.CanReceiveFirstPurchaseReward();
            _reddot.OnBindShowReddot();
        }

        private void UpdateView()
        {
            var rewardGoodType = DataController.Instance.shop.GetRewardGoodTypes(ProductType.FirstPurchaseReward)[0];
            var param0 = DataController.Instance.shop.GetRewardParam0(ProductType.FirstPurchaseReward)[0];
            
            var gradeType = DataController.Instance.elemental.GetBData(param0).grade;
            var htmlColor = ColorUtility.ToHtmlStringRGBA(ResourcesManager.GradeColor[gradeType]);
            var gradeText = $"<color=#{htmlColor}>{gradeType.ToString()}</color>";
            
            View
                .SetRewardSprite(DataController.Instance.good.GetImage(rewardGoodType, param0))
                .SetValueText(gradeText)
                .SetEnable(DataController.Instance.shop.CanReceiveFirstPurchaseReward());
            
            UpdateReddot();
        }
    }
}