using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotStageLevel : MonoBehaviour
    {
        public RectTransform Content
        {
            get
            {
                if (_content == null) _content = GetComponent<RectTransform>();
                return _content;
            }
        }

        public Button ToLevelButton => button;
        
        public int StageLevel => _stageLevel;

        private RectTransform _content;

        private int _stageLevel;
        private Color _originColor = Color.white;
        
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text levelTMP;

        public ViewSlotStageLevel SetStageLevel(int level)
        {
            _stageLevel = level;
            levelTMP.text = (level + 1).ToString();
            return this;
        }

        public ViewSlotStageLevel SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotStageLevel SetFocus(bool flag)
        {
            if (_originColor == Color.white)
                _originColor = levelTMP.color;

            var text = (_stageLevel + 1).ToString();

            levelTMP.DOColor(flag ? _originColor : Color.gray, 0.2f);
            levelTMP.transform.DOScale(Vector3.one * (flag ? 1.6f : 1f), 0.2f);
            levelTMP.text = text;
            return this;
        }
    }
}