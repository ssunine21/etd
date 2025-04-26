using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using UnityEngine;
using ETD.Scripts.UserData.CloudData;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace ETD.Scripts.UserData.DataController
{ 
    public partial class DataController
    {
        public DataRune rune;
    }

    [Serializable]
    public class DataRune
    {
        public UnityAction OnBindChangeSummonCount;
        
        public int summonCount;
        public List<Rune> runes;
        public int MaxRuneCount = 300;
        public bool IsMaxRune => runes.Count >= MaxRuneCount;

        private BRune[] BData => CloudData.CloudData.Instance.bRunes;
        private Dictionary<RuneType, Dictionary<GradeType, BRune>> _bDataCache;

        public void Init()
        {
            Caching();
        }
        
        public void AddSummonCount(int count)
        {
            summonCount += count;
            OnBindChangeSummonCount?.Invoke();
        }

        public BRune GetBData(RuneType runeType, GradeType gradeType)
        {
            var bRune = _bDataCache[runeType][gradeType];
            return bRune;
        }
        
        public BRune GetBData(int index)
        {
            var bRune = BData[index];
            return bRune;
        }

        public async UniTask<List<Rune>> GetRandomRuneType(int count)
        {
            var items = await DataController.Instance.probability.GetRandomProbabilitys(ProbabilityType.SummonRune, count);
            if (items == null)
            {
                ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                return null;
            }

            var types = new List<Rune>();
            foreach (var item in items)
            {
                var randomRuneType = Utility.GetRandomEnumValue<RuneType>(null);
                var randomGradeType = item.gradeType;

                var rune = GetRune(randomRuneType, randomGradeType);
                types.Add(rune);
            }

            return types;
        }
        
        public List<Rune> GetRuneType(GradeType gradeType, int count = 1)
        {
            var types = new List<Rune>();
            for (var i = 0; i < count; ++i)
            {
                var randomRuneType = Utility.GetRandomEnumValue<RuneType>(null);
                var rune = GetRune(randomRuneType, gradeType);
                types.Add(rune);
            }

            return types;
        }

        public List<Rune> GetRuneType(int param0, int count = 1)
        {
            var types = new List<Rune>();
            for (var i = 0; i < count; ++i)
            {
                var rune = GetRune(Get(param0).type, Get(param0).grade);
                types.Add(rune);
            }

            return types;
        }

        private Rune GetRune(RuneType runeType, GradeType gradeType)
        {
            var rune = new Rune(runeType, gradeType);
            var bRune = DataController.Instance.rune.GetBData(rune.type, rune.grade);
            for (var j = 0; j < bRune.obtainableAttrTypes.Length; ++j)
            {
                var attribute = new Attribute(bRune.obtainableAttrTypes[j],
                    bRune.obtainableAttrValues[j]);

                if (attribute.type == AttributeType.RandomTag)
                {
                    var tagType = (TagType)Random.Range((int)bRune.obtainableAttrValues[j],
                        (int)bRune.dynamicAttrValueRange[j]);
                    rune.tags.Add(tagType);
                    attribute.value = (int)tagType;
                }
                else
                    attribute.value += Random.Range(0, bRune.dynamicAttrValueRange[j]);
                    
                rune.equippingAttr.Add(attribute);
            }

            return rune;
        }

        public void Remove(Rune rune)
        {
            runes.Remove(rune);
        }

        public void Earn(Rune rune)
        {
            runes.Add(rune);
        }

        public List<Rune> Gets()
        {
            return runes;
        }
        
        public Rune Get(int index)
        {
            return runes.Count <= index ? null : runes[index];
        }

        public Sprite GetImage(RuneType type, GradeType gradeType)
        {
            var grade = (int)gradeType;
            return type switch
            {
                RuneType.Projectile => ResourcesManager.Instance.projectileRunes[
                    Mathf.Clamp(grade, 0, ResourcesManager.Instance.projectileRunes.Length - 1)],
                RuneType.Expansion => ResourcesManager.Instance.expansionRunes[
                    Mathf.Clamp(grade, 0, ResourcesManager.Instance.expansionRunes.Length - 1)],
                RuneType.Chain => ResourcesManager.Instance.chainRunes[
                    Mathf.Clamp(grade, 0, ResourcesManager.Instance.chainRunes.Length - 1)],
                RuneType.Amplify => ResourcesManager.Instance.amplifyRunes[
                    Mathf.Clamp(grade, 0, ResourcesManager.Instance.amplifyRunes.Length - 1)],
                RuneType.AttackSpeed => ResourcesManager.Instance.attackSpeedRunes[
                    Mathf.Clamp(grade, 0, ResourcesManager.Instance.attackSpeedRunes.Length - 1)],
                RuneType.Duration => ResourcesManager.Instance.durationRunes[
                    Mathf.Clamp(grade, 0, ResourcesManager.Instance.durationRunes.Length - 1)],
                RuneType.RandomTag => ResourcesManager.Instance.randomTag[
                    Mathf.Clamp(grade, 0, ResourcesManager.Instance.randomTag.Length - 1)],
                _ => null
            };
        }
        
        public Sprite GetBackCardImage(RuneType type)
        {
            return ResourcesManager.Instance.backRuneCardImages[Mathf.Clamp((int)type, 0, ResourcesManager.Instance.backRuneCardImages.Length - 1)];
        }

        private void Caching()
        {
            runes ??= new List<Rune>();
            _bDataCache ??= new Dictionary<RuneType, Dictionary<GradeType, BRune>>();
            
            foreach (var bRune in BData)
            {
                _bDataCache.TryAdd(bRune.runeType, new Dictionary<GradeType, BRune>());
                _bDataCache[bRune.runeType].TryAdd(bRune.grade, bRune);
            }
        }
    }

    [Serializable]
    public class Rune : IEnhanceable
    {
        public int Index { get; set; }
        public EnhancementType EnhancementType => enhancementType;
        public bool IsEquipped => equippedIndex != -1;
        
        //interface member
        public int EnhancementLevel => enhancementLevel;
        public int EquippedIndex => equippedIndex;
        public GradeType GradeType => grade;
        public Sprite IconSprite => DataController.Instance.rune.GetImage(type, grade);
        //----------------
        
        public RuneType type;
        public GradeType grade;
        public EquippedPositionType equippedPositionType;
        public EnhancementType enhancementType;
        
        public int enhancementLevel;
        public int equippedIndex = -1;

        public List<Attribute> equippingAttr;
        public List<TagType> tags;
        
        public Rune(){}
        
        public Rune(RuneType type, GradeType grade)
        {
            this.type = type;
            this.grade = grade;
            equippingAttr = new List<Attribute>();
            tags = new List<TagType>();
            
            enhancementType = EnhancementType.Rune;
        }
        
        public void Enhance()
        {
            enhancementLevel++;
            var bData = DataController.Instance.rune.GetBData(type, grade);
            
            for (var i = 0; i < bData.enhanceEquipAttrIncreaseValues.Length; ++i)
            {
                if (equippingAttr.Count == i) break;
                equippingAttr[i].value += bData.enhanceEquipAttrIncreaseValues[i];
            }
        }
    }
}