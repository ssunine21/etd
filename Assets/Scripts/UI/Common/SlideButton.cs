using System;
using System.Collections;
using DG.Tweening;
using ETD.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    public class SlideButton : MonoBehaviour
    {
        public int SelectedIndex { get; private set; }
        public Button[] Buttons => _contents;
        
        private RectTransform _rectTransform;
        private Button[] _contents;
        private TMP_Text[] _contentTexts;

        [SerializeField] private RectTransform handle;
        [SerializeField] private Color enableColor;
        [SerializeField] private Color disableColor;
        [SerializeField] private Image[] contentImages;

        public void AddListener(UnityAction<int> action)
        {
            if(_contents == null) InitButton();
            
            var i = 0;
            foreach (var button in _contents)
            {
                var index = i;
                button.onClick.AddListener(() => action?.Invoke(index));
                ++i;
            }
        }

        public void OnClick(int index)
        {
            if (index >= _contents.Length) return;
            _contents[index].onClick?.Invoke();
        }
        
        private void Awake()
        {
            InitButton();
        }
        
        private void InitButton()
        {
            _rectTransform = GetComponent<RectTransform>();
            _contents = transform.GetComponentsInChildren<Button>();
            _contentTexts = transform.GetComponentsInChildren<TMP_Text>();

            if (handle == null)
                handle = transform.Find("Handle").GetComponent<RectTransform>();

            for (var i = 0; i < _contents.Length; ++i)
            {
                var index = i;
                _contents[index].onClick.AddListener(() =>
                {
                    MoveHandle(index);
                    ChangeColor(index);      
                    SelectedIndex = index;
                });
            }

            AsyncNormailzedSize();
        }

        public void AsyncNormailzedSize()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(NormailzedSize());
        }

        private IEnumerator NormailzedSize()
        {
            var width = _rectTransform.rect.width;
            while (_rectTransform.rect.width <= 0)
            {
                yield return null;
            }
            
            handle.sizeDelta = new Vector2(_rectTransform.rect.width / _contents.Length, handle.sizeDelta.y);
        }

        private void MoveHandle(int index)
        {
            handle
                .DOAnchorPosX(index * handle.sizeDelta.x, 0.1f)
                .SetUpdate(true);
        }

        private void ChangeColor(int index)
        {
            for (var i = 0; i < _contentTexts.Length; ++i)
            {
                _contentTexts[i].color = i == index ? enableColor : disableColor;
            }

            if (contentImages != null)
            {
                for (var i = 0; i < contentImages.Length; ++i)
                {
                    contentImages[i].material = i == index ? null : ResourcesManager.Instance.grayScaleMaterial;
                }
            }
        }
    }
}