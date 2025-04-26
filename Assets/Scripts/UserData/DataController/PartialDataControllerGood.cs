using System;
using System.Collections.Generic;
using BackEnd;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataGood good;
    }

    [Serializable]
    public class DataGood
    {
        public UnityAction<GoodType> OnBindChangeGood;
        public List<double> goods;

        private Dictionary<GoodType, BGood> _cachce;

        public void Init()
        {
            Caching();
        }

        public void EarnReward(GoodItem goodItem)
        {
            EarnReward(goodItem.GoodType, goodItem.Value, goodItem.Param0);
        }
        
        public void EarnReward(List<GoodItem> goodItems)
        {
            foreach (var goodItem in goodItems)
            {
                EarnReward(goodItem.GoodType, goodItem.Value, goodItem.Param0);
            }
        }

        public void EarnReward(GoodType[] goodTypes, double[] values, int[] params0)
        {
            for (var i = 0; i < goodTypes.Length; ++i)
            {
                EarnReward(goodTypes[i], values[i], params0[i]);
            }
        }

        public void EarnReward(GoodType goodType, double value, int param0 = 0)
        {
            switch (goodType)
            {
                case GoodType.SummonElemental or GoodType.SummonRune or GoodType.SummonElementalS
                    or GoodType.SummonElementalSS or GoodType.SummonRuneS or GoodType.SummonRuneSS:
                {
                    var isResummon = (goodType is GoodType.SummonElemental or GoodType.SummonRune) && param0 == 0;
                    var summonController = ControllerCanvas.Get<ControllerCanvasSummon>();
                    summonController.Summon(goodType, (int)value, param0, isResummon).Forget();
                    break;
                }
                case GoodType.Protection:
                {
                    var addHour = DataController.Instance.shop.GetProtectionDurationTimeInHour() * (param0 + 1);
                    DataController.Instance.player.SetProtectionTime(addHour);
                    break;
                }
                default:
                    Earn(goodType, value);
                    break;
            }
        }

        public void Earn(GoodItem goodItem)
        {
            Earn(goodItem.GoodType, goodItem.Value, goodItem.Param0);
        }

        public void Earn(GoodType type, double value, int param0 = 0)
        {
            if (type == GoodType.DungeonTickets)
            {
                Earn(GoodType.EnhanceDungeonTicket, value);
                Earn(GoodType.GemDungeonTicket, value);
                Earn(GoodType.GoldDungeonTicket, value);
                return;
            }

            if (param0 > 0)
            {
                value = DataController.Instance.offlineReward.GetValueUntilTime(type, (float)value, 0);
            }
            
            goods[(int)type] = Math.Max(0, goods[(int)type] + value);
            OnBindChangeGood?.Invoke(type);
        }
        
        private void Consume(GoodType type, double value)
        {
            goods[(int)type] -= value;
            OnBindChangeGood?.Invoke(type);
            
            DataController.Instance.LocalSave();
        }

        public bool TryConsume(GoodItem goodItem, bool isShowMessage = true)
        {
            return TryConsume(goodItem.GoodType, goodItem.Value, isShowMessage);
        }
        
        public bool TryConsume(KeyValuePair<GoodType, double> keyValuePair, bool isShowMessage = true)
        {
            return TryConsume(keyValuePair.Key, keyValuePair.Value, isShowMessage);
        }

        public bool TryConsume(GoodType type, double value, bool isShowMessage = true)
        {
            if (!(goods[(int)type] - value >= 0))
            {
                if (isShowMessage)
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>()
                        .ShowTransientToastMessage(LocalizedTextType.NotEnoughGoods);
                }
                return false;
            }
            Consume(type, value);
            return true;

        }

        public void SetValue(GoodType type, double value)
        {
            if (type == GoodType.None) return;
            goods[(int)type] = value;
            OnBindChangeGood?.Invoke(type);
        }

        public double GetValue(GoodType type)
        {
            if (type == GoodType.None) return 0;
            return goods[(int)type];
        }
        
        public Sprite GetImage(GoodType type, int parma0 = 0)
        {
            return type == GoodType.SummonElemental 
                ? DataController.Instance.elemental.GetImage(parma0) : ResourcesManager.GoodSprites.GetValueOrDefault(type, null);
        }

        public LocalizedTextType GetGoodInfoTitle(GoodType goodType)
        {
            return _cachce.TryGetValue(goodType, out var bData) ? bData.titleLocalizeType : LocalizedTextType.ErrorMessage;
        }
        
        public LocalizedTextType GetGoodInfoDescription(GoodType goodType)
        {
            return _cachce.TryGetValue(goodType, out var bData) ? bData.descLocalizeType : LocalizedTextType.ErrorMessage;
        }
        
        public LocalizedTextType[] GetGoodInfoSources(GoodType goodType)
        {
            return _cachce.TryGetValue(goodType, out var bData) ? bData.sourceLocalizeTypes : new[] { LocalizedTextType.UpgradeTitle };
        }
        
        public LocalizedTextType[] GetGoodInfoUsages(GoodType goodType)
        {
            return _cachce.TryGetValue(goodType, out var bData) ? bData.usageLocalizeTypes : new[] { LocalizedTextType.UpgradeTitle };
        }

        public bool IsEnoughGood(GoodType type, double value)
        {
            if (type == GoodType.None) return false;
            return goods[(int)type] != 0 && goods[(int)type] >= value;
        }

        public bool IsGuildGoods(GoodType type)
        {
            return type is GoodType.GuildExp or GoodType.GuildGiftBoxPoint or GoodType.GuildGiftBoxExp;
        }

        public goodsType GetBackendGoodsTypeFromGoodType(GoodType goodType)
        {
            return goodType switch
            {
                GoodType.GuildExp => goodsType.goods1,
                GoodType.GuildGiftBoxPoint => goodsType.goods2,
                GoodType.GuildGiftBoxExp => goodsType.goods3,
                _ => goodsType.goods10
            };
        }
        
        public GoodType GetGoodTypeFromBackendGoodString(string backendGoodType)
        {
            return backendGoodType switch
            {
                "totalGoods1Amount" => GoodType.GuildExp,
                "totalGoods2Amount" => GoodType.GuildGiftBoxPoint,
                "totalGoods3Amount" => GoodType.GuildGiftBoxExp,
                "totalGoods4Amount" => GoodType.None,
                "totalGoods5Amount" => GoodType.None,
                _ => GoodType.None
            };
        }
        
        private void Caching()
        {
            goods ??= new List<double>();
            foreach (int goodType in Enum.GetValues(typeof(GoodType)))
            {
                if (goods.Count <= goodType)
                {
                    goods.Add(0);
                }
            }

            _cachce ??= new Dictionary<GoodType, BGood>();
            foreach (var bGood in CloudData.CloudData.Instance.bGoods)
            {
                _cachce[bGood.goodType] = bGood;
            }
        }
    }
}