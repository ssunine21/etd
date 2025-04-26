using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotAttribute : ViewSlot<ViewSlotAttribute>
    {
        [SerializeField] private TMP_Text index;
        [SerializeField] private TMP_Text value;
        
        public ViewSlotAttribute SetIndex(AttributeType type)
        {
            var text  = LocalizeManager.GetText(type);
            if (type is AttributeType.Projectile or AttributeType.Chain or AttributeType.Duration or AttributeType.Expansion)
                text = $"<color=orange>{text}</color>";

            index.text = text;
            return this;
        }

        public ViewSlotAttribute SetValue(AttributeType type, float value)
        {
            var text = type == AttributeType.RandomTag
                ? LocalizeManager.GetText((TagType)(int)value)
                : value.ToAttributeValueString(type);

            if (type is AttributeType.Projectile or AttributeType.Chain or AttributeType.Duration or AttributeType.Expansion)
                text = $"<color=orange>{text}</color>";

            this.value.text = text;
            
            return this;
        }

        public ViewSlotAttribute SetValueText(string text)
        {
            value.text = text;
            return this;
        }
    }
}