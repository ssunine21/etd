using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewDamageText : MonoBehaviour
    {
        public bool Enabled => _defaultText.enabled;
        public Text TMPText => _defaultText;
        public string Text
        {
            get => _defaultText.text;
            set => _defaultText.text = value;
        }
        
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Text _defaultText;

        public ViewDamageText SetActive(bool flag)
        {
            _defaultText.enabled = flag;
            //gameObject.SetActive(flag);
            return this;
        }

        public ViewDamageText SetPosition(Vector2 position)
        {
            transform.position = position;
            return this;
        }
    }
}