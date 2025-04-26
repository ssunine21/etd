using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotGuildMember : ViewSlot<ViewSlotGuildMember>
    {
        public ActiveButton ExpelButton => expelButton;
        public ActiveButton NominateButton => nominateButton;
        
        [SerializeField] private Image markImage;
        [SerializeField] private GameObject masterTagGO;
        [SerializeField] private TMP_Text nickname;
        [SerializeField] private TMP_Text combatTMP;
        [SerializeField] private TMP_Text maxStageTMP;
        [SerializeField] private TMP_Text guildContributionTMP;
        [SerializeField] private TMP_Text lastLoginTimeTMP;
        [SerializeField] private ActiveButton expelButton;
        [SerializeField] private ActiveButton nominateButton;
        [SerializeField] private GameObject manageWrapGo;
        [SerializeField] private GameObject lastLoginWarp;
        [SerializeField] private GameObject pointWarp;

        public ViewSlotGuildMember SetMark(bool isMaster)
        {
            var memberMark = isMaster ? ResourcesManager.Instance.masterMark : ResourcesManager.Instance.memberMark;
            SetMark(memberMark);
            return this;
        }
        
        public ViewSlotGuildMember SetMark(Sprite sprite)
        {
            markImage.sprite = sprite;
            return this;
        }

        public ViewSlotGuildMember SetActiveManageWrap(bool flag)
        {
            manageWrapGo.SetActive(flag);
            return this;
        }

        public ViewSlotGuildMember SetActiveManageButton(bool flag)
        {
            expelButton.gameObject.SetActive(flag);
            nominateButton.gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotGuildMember SetActiveInfoWarp(bool flag)
        {
            lastLoginWarp.SetActive(flag);
            pointWarp.SetActive(flag);
            return this;
        }

        public ViewSlotGuildMember SetNickname(string text)
        {
            nickname.text = text;
            return this;
        }

        public ViewSlotGuildMember SetCombat(string text)
        {
            combatTMP.text = text;
            return this;
        }

        public ViewSlotGuildMember SetCurrStage(int level)
        {
            if (maxStageTMP)
                maxStageTMP.text = DataController.Instance.stage.GetStageLevelExpression(level);
            return this;
        }

        public ViewSlotGuildMember SetContribution(int point)
        {
            guildContributionTMP.text = point.ToString("N0");
            return this;
        }

        public ViewSlotGuildMember SetLastLoginTime(string text)
        {
            lastLoginTimeTMP.text = text;
            return this;
        }

        public ViewSlotGuildMember SetActiveMasterTag(bool flag)
        {
            masterTagGO.SetActive(flag);
            return this;
        }
    }
}