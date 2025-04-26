using System;
using ETD.Scripts.Manager;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.Common
{
    public class LocalizeText : MonoBehaviour
    {
        [SerializeField] private string localizedTextTypeToString;
        [SerializeField] private TMP_Text text;

        private void Awake()
        {
            text ??= GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (text == null) return;
            text.text = Enum.TryParse<LocalizedTextType>(localizedTextTypeToString, out var type)
                ? LocalizeManager.GetText(type)
                : "";
        }
    }
}
