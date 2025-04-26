using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasMail : ControllerCanvas
    {
        private readonly List<ViewSlotMail> _viewSlotMailList = new();
        private const string ViewSlotMailName = nameof(ViewSlotMail);

        private ViewCanvasMail View => ViewCanvas as ViewCanvasMail;

        public ControllerCanvasMail(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasMail>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.GetAllRewardButton.OnClick.AddListener(ReceivePostItemAll);
            AsyncMail().Forget();
        }
        
        private void GetMailList(bool isShowLoading)
        {
            if(isShowLoading) Get<ControllerCanvasToastMessage>().ShowLoading();
            TryGetMailList();
        }

        private void TryGetMailList()
        {
            BackendManager.GetPostList(postList =>
            {
                var hasMail = postList is { Count: > 0 };
                if (hasMail)
                {
                    var postItems = _viewSlotMailList.GetViewSlots(ViewSlotMailName, View.SlotParent, postList.Count);
                    var index = 0;
                    foreach (var postData in postList)
                    {
                        var postItem = postItems[index];
                        postItem.GetRewardButton.onClick.RemoveListener(postItem.ReceivePostItem);
                        postItem.GetRewardButton.onClick.AddListener(postItem.ReceivePostItem);
                        
                        postItem
                            .SetTitle(LocalizeManager.GetText(postData.title))
                            .SetDescription(LocalizeManager.GetText(postData.content))
                            .SetRemainingTime(LocalizeManager.GetText(LocalizedTextType.Mail_RemainingDays,
                                postData.GetRemainingDay()))
                            .SetInDate(postData.inDate)
                            .SetActive(true);
                        postItem.ViewGood
                            .SetInit(postData.goodType)
                            .SetValue(postData.goodValue);
                        ++index;
                    }

                    for (; index < postList.Count; ++index)
                    {
                        _viewSlotMailList[index].SetActive(false);
                    }
                    View.SetActiveEmptyTextPanel(false);
                }
                View.SetActiveEmptyTextPanel(!hasMail);
                View.GetAllRewardButton.OnOffView(hasMail);
                
                ControllerCanvasMainMenu.onBindReddot?.Invoke(ReddotType.ReceiveableMail, hasMail);
                Get<ControllerCanvasToastMessage>().CloseLoading();
                
            });
        }

        private void ReceivePostItemAll()
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            BackendManager.ReceivePostItemAll(rewardList =>
            {
                if(rewardList != null)
                {
                    var goodItems = new List<GoodItem>();
                    foreach (var (goodType, goodValue) in rewardList)
                    {
                        goodItems.Add(new GoodItem(goodType, goodValue));
                    }

                    Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(goodItems, LocalizeManager.GetText(LocalizedTextType.Claimed)).Forget();
                    DataController.Instance.good.EarnReward(goodItems);

                    foreach (var slotMail in _viewSlotMailList)
                    {
                        slotMail.SetActive(false);
                    }
                    
                    GetMailList(true);
                    DataController.Instance.LocalSave();
                }
                Get<ControllerCanvasToastMessage>().CloseLoading();
            });
        }

        private async UniTaskVoid AsyncMail()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            
            while (!Cts.IsCancellationRequested)
            {
                GetMailList(false);
                await UniTask.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
}