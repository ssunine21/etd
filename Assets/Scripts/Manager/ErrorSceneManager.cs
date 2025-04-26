using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.Manager
{
    public class ErrorSceneManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text buttonTMP;

        private void Start()
        {
            GameManager.Instance.Pause();
            
            desc.text = GameManager.errorMessage;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(GameManager.errorAction);

            buttonTMP.text = Application.systemLanguage switch
            {
                SystemLanguage.Korean => "확인",
                SystemLanguage.Japanese => "確認",
                SystemLanguage.ChineseTraditional => "確認",
                SystemLanguage.ChineseSimplified => "确认",
                _ => "Confirm"
            };
        }
    }
}