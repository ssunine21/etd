using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasCombatPower : ViewCanvas
    {
        public ViewCombatPowerSlot ViewCombatPowerSlotPrefab => viewCombatPowerSlotPrefab;

        [SerializeField] private ViewCombatPowerSlot viewCombatPowerSlotPrefab;
    }
}