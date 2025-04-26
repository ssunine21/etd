using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;

namespace ETD.Scripts.UI.Controller
{
    public abstract class ControllerCanvas
    {
        public bool ActiveSelf => ViewCanvas && ViewCanvas.Canvas && ViewCanvas.Canvas.enabled;
        protected const float ViewDoDurationTime = 0.2f;
        
        protected CancellationTokenSource Cts;

        private static readonly Dictionary<string, ControllerCanvas> Controllers = new();
        private static readonly Dictionary<int, Queue<ControllerCanvas>> OpenViewDic = new();

        protected readonly ViewCanvas.ViewCanvas ViewCanvas;
        
        public static void Add<T>(T controller) where T : ControllerCanvas
        {
            Controllers.Add(typeof(T).Name, controller);
        }

        public static T Get<T>() where T : ControllerCanvas
        {
            var controllerName = typeof(T).Name;
            if (!Controllers.TryGetValue(controllerName, out var controller)) return null;
        
            return controller as T;
        }

        protected static void EnqueueOpenView(ControllerCanvas controller, int openQuestLevel)
        {
            if (!OpenViewDic.ContainsKey(openQuestLevel))
                OpenViewDic.Add(openQuestLevel, new Queue<ControllerCanvas>());
            
            OpenViewDic[openQuestLevel].Enqueue(controller);
            controller.ViewCanvas.OnBindClose += () => OpenViewQueue(openQuestLevel);
        }

        protected static void OpenViewQueue(int questLevel)
        {
            if (OpenViewDic.TryGetValue(questLevel, out var queue))
            {
                if (queue.Count > 0)
                {
                    var controller = queue.Dequeue();
                    controller.Open();
                }
            }
        }

        protected ControllerCanvas(CancellationTokenSource cts, ViewCanvas.ViewCanvas viewCanvas)
        {
            Cts = cts;
            ViewCanvas = viewCanvas;
            ViewCanvas.SetActive(false);
            foreach (var closeButton in ViewCanvas.CloseButtons)
            {
                if (closeButton != null)
                    closeButton.onClick.AddListener(Close);
            }
        }
        
        protected void SetViewAnimation(ViewAnimationType animationType)
        {
            ViewCanvas.SetViewAnimation(animationType);
        }

        public virtual void Open()
        {
            ViewCanvas.Open();
        }

        public virtual void Close()
        {
            ViewCanvas.Close();
        }

        public void SetActive(bool flag)
        {
            ViewCanvas.SetActive(flag);
        }
    }
}