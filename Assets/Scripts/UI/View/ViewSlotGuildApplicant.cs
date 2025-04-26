using ETD.Scripts.Common;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotGuildApplicant : MonoBehaviour
    {
        public Button ApplyButton => applyButton;
        public Button RejectButton => rejectButton;

        [SerializeField] private TMP_Text nicknameTMP;
        [SerializeField] private TMP_Text combatTMP;
        [SerializeField] private TMP_Text stageTMP;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button rejectButton;
        
        [SerializeField] private TMP_Text applyTMP;
        [SerializeField] private TMP_Text rejectTMP;

        public ViewSlotGuildApplicant SetNickname(string text)
        {
            nicknameTMP.text = text;
            return this;
        }

        public ViewSlotGuildApplicant SetCombat(double combat)
        {
            combatTMP.text = combat.ToDamage();
            return this;
        }

        public ViewSlotGuildApplicant SetStage(int totalStage)
        {
            stageTMP.text = DataController.Instance.stage.GetStageLevelExpression(totalStage);
            return this;
        }

        public ViewSlotGuildApplicant SetApplyText(string text)
        {
            applyTMP.text = text;
            return this;
        }

        public ViewSlotGuildApplicant SetRejectText(string text)
        {
            rejectTMP.text = text;
            return this;
        }

        public ViewSlotGuildApplicant SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
    }
}