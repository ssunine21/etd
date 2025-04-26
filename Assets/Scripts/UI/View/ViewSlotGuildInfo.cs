using System;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotGuildInfo : MonoBehaviour
    {
        public Button JoinButton => showInfoButton;

        [SerializeField] private MarkInfo markInfo;
        [Space] [Space] [SerializeField] private TMP_Text nameTMP;

        [SerializeField] private TMP_Text combatTMP;
        [SerializeField] private TMP_Text levelTMP;
        [SerializeField] private TMP_Text expTMP;
        [SerializeField] private Image expImage;
        [SerializeField] private TMP_Text joinTypeTMP;
        [SerializeField] private TMP_Text memberCountTMP;
        [SerializeField] private TMP_Text neededStageTMP;
        [SerializeField] private TMP_Text descriptionTMP;
        [SerializeField] private Button showInfoButton;
        [SerializeField] private GameObject rankGo;
        [SerializeField] private TMP_Text rankTMP;
        [SerializeField] private Image topRankImage;

        public ViewSlotGuildInfo SetRanking(int rank)
        {
            rankTMP.text = rank.ToString();
            topRankImage.enabled = rank < 4;
            var sprite = ResourcesManager.Instance.GetRankSprite(Mathf.Max(0, rank - 1));
            if (sprite != null)
                topRankImage.sprite = sprite;
            return this;
        }

        public ViewSlotGuildInfo SetActiveRankSlot(bool flag)
        {
            rankGo.SetActive(flag);
            return this;
        }

    public ViewSlotGuildInfo SetMarkImage(MarkImageType imageType, int index)
        {
            var sprite = GetMarkSprites(imageType)[index];
            markInfo.SetMarkImage(imageType, sprite);
            return this;
        }
        
        public ViewSlotGuildInfo SetMarkImage(int index0, int index1, int index2)
        {
            SetMarkImage(MarkImageType.Background, index0);
            SetMarkImage(MarkImageType.MainSymbol, index1);
            SetMarkImage(MarkImageType.SubSymbol, index2);
            return this;
        }

        public ViewSlotGuildInfo SetName(string text)
        {
            nameTMP.text = text;
            return this;
        }

        public ViewSlotGuildInfo SetCombat(double combat)
        {
            combatTMP.text = combat.ToDamage();
            return this;
        }

        public ViewSlotGuildInfo SetLevel(int level)
        {
            if (levelTMP)
                levelTMP.text = LocalizeManager.GetText(LocalizedTextType.Lv, level);
            return this;
        }

        public ViewSlotGuildInfo SetExp(int currCount, int maxCount, bool isFillAmount = true)
        {
            if (expTMP)
                expTMP.text = $"{currCount}/{maxCount}";

            if (isFillAmount && expImage)
            {
                try
                {
                    expImage.fillAmount = currCount / (float)maxCount;
                }
                catch (Exception e)
                {
                    expImage.fillAmount = 0;
                }
            }    
            return this;
        }
        
        public ViewSlotGuildInfo SetJoinType(bool immediateRegistration)
        {
            joinTypeTMP.text = immediateRegistration
                ? LocalizeManager.GetText(LocalizedTextType.AutoJoin) : LocalizeManager.GetText(LocalizedTextType.ApproveJoin);
            return this;
        }

        public ViewSlotGuildInfo SetMemberCount(string text)
        {
            memberCountTMP.text = text;
            return this;
        }

        public ViewSlotGuildInfo SetMemberCount(int currCount, int maxCount)
        {
            memberCountTMP.text = $"({currCount}/{maxCount})";
            return this;
        }

        public ViewSlotGuildInfo SetNeededStage(int level)
        {
            if (neededStageTMP)
                neededStageTMP.text = DataController.Instance.stage.GetStageLevelExpression(level);
            return this;
        }

        public ViewSlotGuildInfo SetDescription(string text)
        {
            if (descriptionTMP)
                descriptionTMP.text = text;
            return this;
        }
        
        public ViewSlotGuildInfo SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
        
        private Sprite[] GetMarkSprites(MarkImageType imageType)
        {
            return imageType switch
            {
                MarkImageType.Background => ResourcesManager.Instance.guildMarkBackgroundSprites,
                MarkImageType.MainSymbol => ResourcesManager.Instance.guildMarkMainSymbolSprites,
                MarkImageType.SubSymbol => ResourcesManager.Instance.guildMarkSubSymbolSprites,
                _ => Array.Empty<Sprite>()
            };
        }
    }

    [Serializable]
    public class MarkInfo
    {
        [SerializeField] private Image markBackgroundImage;
        [SerializeField] private Image markMainImage;
        [SerializeField] private Image markSubImage;
        
        public void SetMarkImage(MarkImageType imageType, Sprite sprite)
        {
            var image = imageType switch
            {
                MarkImageType.Background => markBackgroundImage,
                MarkImageType.MainSymbol => markMainImage,
                MarkImageType.SubSymbol => markSubImage,
                _ => null
            };
            
            if (image)
            {
                image.sprite = sprite;
                image.color = 
                    sprite ? 
                        imageType == MarkImageType.SubSymbol 
                            ? new Color(0.8f, 0.8f, 0.8f, 1) 
                            : new Color(1, 1, 1, 1) 
                        : new Color(1, 1, 1, 0);
            }
        }
    }
    
    public enum MarkImageType
    {
        Background,
        MainSymbol,
        SubSymbol
    }
}