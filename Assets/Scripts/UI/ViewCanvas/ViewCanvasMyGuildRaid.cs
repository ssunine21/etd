using System.Collections.Generic;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMyGuild
    {
        public ViewSlotGuildRaidRanking ViewSlotGuildRaidRankingPrefab => viewSlotGuildRaidRanking;
        public Transform ViewSlotGuildRankingParent => viewSlotGuildRankingParent;
        public ViewSlotGuildRaidRanking MyViewSlotRanking => myViewSlotRanking;
        public ActiveButton RaidEnterButton => raidEnterButton;
        public Transform ViewBossInner => viewBossInner; 
        public Transform ViewBossBackground => viewBossBackground;
        public CanvasGroup ViewBossCanvasGroup => viewBossCanvasGroup;
        public Transform RaidLogTr => raidLogTr;
        public ActiveButton ShowRaidBoxButton => showRiadBoxButton;
        public ViewCanvasPopup RaidBoxViewCanvasPopup => raidBoxViewCanvasPopup;
        public List<ViewSlotRaidBox> ViewSlotRaidBoxes => viewSlotRaidBoxes;
        public TMP_Text LogPrefab => logPrefab;
        public Transform LogParent => raidLogTr;
        public Button RaidDescButton => raidDescButton;
        public Button RaidBoxDescButton => raidBoxDescButton;
        
        
        [Space][Space][Header("Raid")]
        [SerializeField] private TMP_Text nextRaidBossTimeTMP;
        [SerializeField] private TMP_Text nextTicketTimeTMP;
        [SerializeField] private TMP_Text ticketCountTMP;
        [SerializeField] private GameObject ticketTimeGo;
        [SerializeField] private ViewSlotGuildRaidRanking myViewSlotRanking;
        [SerializeField] private ViewSlotGuildRaidRanking viewSlotGuildRaidRanking;
        [SerializeField] private Transform viewSlotGuildRankingParent;
        [SerializeField] private ActiveButton raidEnterButton;
        [SerializeField] private CanvasGroup viewBossCanvasGroup;
        [SerializeField] private Transform viewBossInner;
        [SerializeField] private Transform viewBossBackground;
        [SerializeField] private ActiveButton showRiadBoxButton;

        [FormerlySerializedAs("raidBoxViewPopup")]
        [Space] [Header("RaidBox")] 
        [SerializeField] private ViewCanvasPopup raidBoxViewCanvasPopup;
        [SerializeField] private List<ViewSlotRaidBox> viewSlotRaidBoxes;
        [SerializeField] private Transform raidLogTr;
        [SerializeField] private TMP_Text logPrefab;

        [Space] [Header("Description")]
        [SerializeField] private Button raidDescButton;
        [SerializeField] private Button raidBoxDescButton;

        public ViewCanvasMyGuild SetNextBossTime(string text)
        {
            nextRaidBossTimeTMP.text = text;
            return this;
        }

        public ViewCanvasMyGuild SetTicketTime(string text)
        {
            nextTicketTimeTMP.text = text;
            return this;
        }

        public ViewCanvasMyGuild SetTicketCount(int curr, int max)
        {
            ticketCountTMP.text = $"{curr}/{max}";
            return this;
        }

        public ViewCanvasMyGuild SetActiveTicketTime(bool flag)
        {
            ticketTimeGo.SetActive(flag);
            return this;
        }
    }
}