using System;
using ETD.Scripts.Common;
using ETD.Scripts.UI.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewTutorialButton : MonoBehaviour
    {
        [SerializeField] private TutorialType tutorialType;

        private Button _button;

        private void OnEnable()
        {
            if (!_button)
            {
                _button= GetComponent<Button>();
                _button.onClick.AddListener(() =>
                {
                    ControllerCanvas.Get<ControllerCanvasTutorial>().StartTutorial(tutorialType);
                });
            }
        }
    }
}