using System;
using System.Collections.Generic;
using System.Threading;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using Unity.Collections.LowLevel.Unsafe;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasProbability : ControllerCanvas
    {
        private ViewCanvasProbability View => ViewCanvas as ViewCanvasProbability;
        private readonly List<ViewSlotProbability> _slotProbabilities = new();

        private const string ViewSlotProbabilityName = nameof(ViewSlotProbability);
        
        public ControllerCanvasProbability(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasProbability>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
        }

        public void AddOptions(Dictionary<string, float> options)
        {
            var viewSlots = _slotProbabilities.GetViewSlots(ViewSlotProbabilityName, View.SlotParent, options.Count);
            var i = 0;
            foreach (var option in options)
            {
                var slot = viewSlots[i];
                slot.SetTitle(option.Key)
                    .SetProbability(option.Value)
                    .SetTitleColor(ResourcesManager.GradeColor[(GradeType)i])
                    .SetValueColor(ResourcesManager.GradeColor[(GradeType)i])
                    .SetActiveLine(i < options.Count - 1)
                    .SetActive(true);
                ++i;
            }
        }
    }
}