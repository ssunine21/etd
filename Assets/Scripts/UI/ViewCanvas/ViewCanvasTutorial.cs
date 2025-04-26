using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasTutorial : ViewCanvas
    {
        public ViewCanvasPopup ScriptViewCanvasPopupWrap => scriptWrap;
        public GameObject GuideArrowPrefab => guideArrowPrefab;
        public Button NextScriptButton => nextScriptButton;
        public TypewriterByCharacter Typewriter => typewriterByCharacter;
        public Image ScriptArrow => scriptArrow;
        public Image Background => background;
        
        [SerializeField] private ViewCanvasPopup scriptWrap;
        [SerializeField] private Image background;
        
        [Space][Space][Header("Guide")]
        [SerializeField] private GameObject guideArrowPrefab;
        
        [Space][Space][Header("Scripts")]
        [SerializeField] private TypewriterByCharacter typewriterByCharacter;
        [SerializeField] private Button nextScriptButton;
        [SerializeField] private Image scriptArrow;
    }
}