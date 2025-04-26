using System;
using System.Collections.Generic;
using DG.Tweening;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewBuffIconParts : MonoBehaviour
    {
        [SerializeField] private Image[] icons;
        [SerializeField] private Color[] enabledColors;
        [SerializeField] private Color disabledColor;

        private List<Tween> _tweens;

        private void Start()
        {
            if (name.Contains("Clone")) return;
            
            _tweens ??= new List<Tween>();
            foreach (var icon in icons)
            {
                _tweens.Add(icon.transform.DORotate(new Vector3(0, 0, -360), 10f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(int.MaxValue)
                    .SetUpdate(true));

                _tweens[^1].Pause();
            }

            UpdateView();
            DataController.Instance.buff.OnBindUpdateBuff += UpdateView;
        }

        private void UpdateView()
        {
            for (var i = 0; i < icons.Length; ++i)
            {
                if (DataController.Instance.buff.IsBuffOn(i))
                {
                    icons[i].color = enabledColors[i];
                    _tweens[i].Play();
                }
                else
                {
                    icons[i].color = disabledColor;
                    _tweens[i].Pause();
                    icons[i].transform.rotation = Quaternion.Euler(Vector3.zero);
                }
            }
        }
    }
}
