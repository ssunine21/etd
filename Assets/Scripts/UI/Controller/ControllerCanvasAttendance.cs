using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasAttendance : ControllerCanvas
    {
        private ViewCanvasAttendance View => ViewCanvas as ViewCanvasAttendance;
        private readonly int attendanceUnlockQuestLevel = DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.Attendance);
        public ControllerCanvasAttendance(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasAttendance>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            
            View.ReceiveButton.onClick.AddListener(TryReceiveReward);
            if (DataController.Instance.quest.currQuestLevel < attendanceUnlockQuestLevel)
                EnqueueOpenView(this, attendanceUnlockQuestLevel);
            
            ServerTime.onBindNextDay += OnNextDay;
            DataController.Instance.quest.OnBindClear += OnBindQuestClear;
            
            UpdateViewAll();
            TimeTask().Forget();
        }
        
        private async UniTaskVoid TimeTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);

            if (!DataController.Instance.attendance.isOpenViewToday && DataController.Instance.contentUnlock.IsUnLock(UnlockType.Attendance))
            {
                if (DataController.Instance.attendance.CanReceiveAny())
                {
                    Open();
                    DataController.Instance.attendance.SetOpenViewToday(true);
                }
            }

            while (!Cts.IsCancellationRequested)
            {
                View.ViewSlotTime.SetTimeText(Utility.GetTimeStringToFromTotalSecond(ServerTime.RemainingTimeUntilNextDay));
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            }
        }

        private void OnBindQuestClear(int clearLevel)
        {
            if(clearLevel == attendanceUnlockQuestLevel)
                OpenViewQueue(attendanceUnlockQuestLevel);
        }

        private void TryReceiveReward()
        {
            for (var i = 0; i < DataController.Instance.attendance.Length; ++i)
            {
                if (DataController.Instance.attendance.CanReceiveReward(i))
                {
                    GetReward(i);
                    DataController.Instance.attendance.ReceiveRewardToday(i);
                    UpdateView(i);
                    return;
                }
            }
        }

        private void GetReward(int index)
        {
            var goodType = DataController.Instance.attendance.GetGoodType(index);
            var value = DataController.Instance.attendance.GetValue(index);
            var param0 = DataController.Instance.attendance.GetParam0(index);
            var goodItem = new GoodItem(goodType, value, param0);
            
            DataController.Instance.good.EarnReward(goodItem);
            Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(goodItem, LocalizeManager.GetText(LocalizedTextType.Claimed));
        }

        private void UpdateViewAll()
        {
            for (var i = 0; i < DataController.Instance.attendance.Length; ++i)
            {
                UpdateView(i);
            }
        }

        private void UpdateView(int index)
        {
            var bData = DataController.Instance.attendance;
            var goodType = bData.GetGoodType(index);
            var rewardValue = bData.GetValue(index);
            var param0 = bData.GetParam0(index);
            var x = goodType == GoodType.SummonElemental ? "" : "x";
            
            View.ViewSlotAttendances[index]
                .SetDay(index + 1)
                .SetIcon(DataController.Instance.good.GetImage(bData.GetGoodType(index), bData.GetParam0(index)))
                .SetGrade($"{x}{rewardValue.ToGoodString(goodType, param0)}")
                .SetActiveEarnPanel(bData.HasReceivedReward(index));
            
            View.ViewSlotAttendances[index].ReddotView.ShowReddot(bData.CanReceiveReward(index));
        }
        
        private void OnNextDay()
        {
            DataController.Instance.attendance.isOpenViewToday = false;
            DataController.Instance.attendance.hasReceivedTodayReward = false;
            
            UpdateViewAll();
        }
    }
}