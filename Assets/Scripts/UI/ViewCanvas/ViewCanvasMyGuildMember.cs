using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMyGuild
    {
        public Transform ViewSlotGuildMemberParent => viewSlotGuildMemberParent;
        
        [Space] [Space] [Header("Member")] 
        [SerializeField] private Transform viewSlotGuildMemberParent;
        [SerializeField] private TMP_Text memberCountTMP;
        [SerializeField] private GameObject manageTMPGo;

        public ViewCanvasMyGuild SetMemberCount(int curr, int max)
        {
            memberCountTMP.text = $"{curr}/{max}";
            return this;
        }

        public ViewCanvasMyGuild SetActiveManageButton(bool isMaster)
        {
            manageTMPGo.SetActive(isMaster);
            return this;
        }
    }
}