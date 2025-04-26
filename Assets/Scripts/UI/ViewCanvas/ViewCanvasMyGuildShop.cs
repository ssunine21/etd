using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMyGuild
    {
        public ViewSlotProduct ViewSlotProductPrefab => viewSlotProductPrefab; 
        public Transform[] ViewSlotProductParent => viewSlotProductParent;
        public ViewGood ShopViewGood => shopViewGood;
        
        [Space] [Space] [Header("Shop")] 
        [SerializeField] private TMP_Text weeklyResetTimeTMP;
        [SerializeField] private ViewSlotProduct viewSlotProductPrefab;
        [SerializeField] private Transform[] viewSlotProductParent;
        [SerializeField] private ViewGood shopViewGood;

        public ViewCanvasMyGuild SetWeeklyResetTime(string text)
        {
            weeklyResetTimeTMP.text = text;
            return this;
        }
    }
}