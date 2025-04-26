using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataEnhancement enhancement;
    }

    [Serializable]
    public class DataEnhancement
    {
        public UnityAction<IEnhanceable> OnBindEnhanced;

        private BEnhancement[] BDatas => CloudData.CloudData.Instance.bEnhancements;
        private Dictionary<EnhancementType, Dictionary<GradeType, BEnhancement>> _bDataCache;

        public void Init()
        {
            Caching();
        }

        public bool IsMaxLevel(IEnhanceable enhanceable)
        {
            return enhanceable.EnhancementLevel >= GetMaxLevel(enhanceable);
        }

        public int GetMaxLevel(IEnhanceable enhanceable)
        {
            return GetBData(enhanceable).maxLevel;
        }

        public double GetDisassembleMaterialValue(IEnhanceable enhanceable)
        {
            double value = 0;
            value += GetBData(enhanceable).disassemblyMaterialValue;
            value += GetEnhanceCostToLevel(enhanceable) * 0.9f;
            return value;
        }

        public double GetEnhanceCostToLevel(IEnhanceable enhanceable)
        {
            double cost = 0;
            var bEnhancement = GetBData(enhanceable);
            for (var i = 0; i < enhanceable.EnhancementLevel; ++i)
            {
                cost += bEnhancement.baseGoodValue + i * bEnhancement.increaseGoodValue;
            }

            return cost;
        }
        
        public KeyValuePair<GoodType, double> GetEnhanceCostValue(IEnhanceable enhanceable)
        {
            var bEnhancement = GetBData(enhanceable);
            var cost = bEnhancement.baseGoodValue + enhanceable.EnhancementLevel * bEnhancement.increaseGoodValue;
            
            var reduceValue = DataController.Instance.research.GetValue(ResearchType.DecreaseEnhancementCost);
            cost *= 1 - reduceValue;
            
            return new KeyValuePair<GoodType, double>(bEnhancement.goodType, cost);
        }
        
        public float GetProbability(IEnhanceable enhanceable)
        {
            var bEnhancement = GetBData(enhanceable);
            var range = Mathf.CeilToInt((float)bEnhancement.maxLevel / bEnhancement.probability.Length);
            var level = enhanceable.EnhancementLevel;

            var probabilityIndex = 0;
            while (level > range)
            {
                level -= range;
                probabilityIndex++;
            }

            var probability = bEnhancement.probability[probabilityIndex];
            probability += DataController.Instance.research.GetValue(ResearchType.IncreaseEnhancementProbability);

            return Mathf.Clamp01(probability);
        }

        public bool TryEnhance(IEnhanceable enhanceable, bool isShowMessage = true)
        {
            var randomValue = Random.Range(0f, 1f);
            var isSuccess = randomValue <= GetProbability(enhanceable);
            
            LocalizedTextType messageKey;

            if (isSuccess)
            {
                messageKey = LocalizedTextType.EnhanceSuccess;
                enhanceable.Enhance();
                DataController.Instance.mission.Count(MissionType.Enhance);

                DataController.Instance.quest.Count(
                    enhanceable.EnhancementType == EnhancementType.Elemental
                        ? QuestType.EnhanceElemental
                        : QuestType.EnhanceRune);
                
                OnBindEnhanced?.Invoke(enhanceable);
            }
            else
                messageKey = LocalizedTextType.EnhanceFail;
            
            if (isShowMessage)
            {
                var toastMessage = ControllerCanvas.Get<ControllerCanvasToastMessage>();
                toastMessage.ShowTransientToastMessage(messageKey);
            }
            
            return isSuccess;
        }

        public BEnhancement GetBData(IEnhanceable enhanceable)
        {
            return _bDataCache[enhanceable.EnhancementType][enhanceable.GradeType];
        }

        public void Caching()
        {
            _bDataCache ??= new Dictionary<EnhancementType, Dictionary<GradeType, BEnhancement>>();
            foreach (var bData in BDatas)
            {
                _bDataCache.TryAdd(bData.enhancementType, new Dictionary<GradeType, BEnhancement>());
                _bDataCache[bData.enhancementType].TryAdd(bData.gradeType, bData);
            }
        }
    }
}