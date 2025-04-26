using System;
using System.Collections.Generic;
using ETD.Scripts.Common;
using UnityEngine;
using System.Threading;
using BackEnd.Functions;
using Cysharp.Threading.Tasks;
using ETD.Scripts.InGame.Controller.ControllerBullet;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.CloudData;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataElemental elemental;
    }

    [Serializable]
    public class DataElemental
    {
        public const int MaxLevel = 200;
        public UnityAction OnBindChangeSummonCount;
        
        public int summonCount;
        public UnityAction<Elemental> OnBindLevelUp;
        public UnityAction<Elemental> OnBindEarn;
        public List<Elemental> elementals;
        public int[] ExpCache { get; private set; }

        private BElemental[] BData => CloudData.CloudData.Instance.bElementals;
        private Dictionary<ElementalType, Dictionary<GradeType, Elemental>> _cache;
        private Dictionary<ElementalType, Dictionary<GradeType, BElemental>> _bDataCache;

        public void Init()
        {
            Caching();
        }

        public void AddSummonCount(int count)
        {
            summonCount += count;
            OnBindChangeSummonCount?.Invoke();
        }

        public async UniTask<List<KeyValuePair<ElementalType, GradeType>>> GetRandomElementalType(int count)
        {
            var items = await DataController.Instance.probability.GetRandomProbabilitys(ProbabilityType.SummonElemental, count);
            if (items == null)
            {
                ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                return null;
            }
            
            var types = new List<KeyValuePair<ElementalType, GradeType>>();
            foreach (var item in items)
            {
                var randomElementalType = Utility.GetRandomEnumValue<ElementalType>(null);
                var randomGradeType = item.gradeType;

                types.Add(new KeyValuePair<ElementalType, GradeType>(randomElementalType, randomGradeType));
            }
            
            return types;
        }
        
        public List<KeyValuePair<ElementalType, GradeType>> GetElementalType(GradeType gradeType, int count = 1)
        {
            var types = new List<KeyValuePair<ElementalType, GradeType>>();
            
            for (var i = 0; i < count; ++i)
            {
                var randomElementalType = Utility.GetRandomEnumValue<ElementalType>(null);
                types.Add(new KeyValuePair<ElementalType, GradeType>(randomElementalType, gradeType));
            }
            return types;
        }
        
        public List<KeyValuePair<ElementalType, GradeType>> GetElementalType(int index, int count = 1)
        {
            var types = new List<KeyValuePair<ElementalType, GradeType>>();

            for (var i = 0; i < count; ++i)
            {
                types.Add(new KeyValuePair<ElementalType, GradeType>(Get(index).type, Get(index).grade));
            }

            return types;
        }

        public void Earn(KeyValuePair<ElementalType, GradeType> keyValuePair)
        {
            var elemental = Get(keyValuePair);

            if (elemental == null) return;
            
            elemental.exp++;
            if (elemental.exp == 1 && elemental.Level == 0)
            {
                elemental.Level++;
            }
            OnBindEarn?.Invoke(elemental);
        }

        public void AllLevelUp(Elemental elemental)
        {
            while (TryLevelUp(elemental)) { }
        }

        public bool TryLevelUp(Elemental elemental)
        {
            DataController.Instance.elemental.GetExpData(elemental, out var currExp, out var neededNextExp);
            if (currExp >= neededNextExp)
            {
                elemental.LevelUp();
                OnBindLevelUp?.Invoke(elemental);
                return true;
            }

            return false;
        }

        public List<Elemental> Gets()
        {
            return elementals;
        }

        public Elemental Get(int index)
        {
            return elementals.Count <= index ? null : elementals[index];
        }
        
        public BElemental GetBData(ElementalType type, GradeType gradeType)
        {
            var bElemental = _bDataCache[type][gradeType];
            return bElemental;
        }
        
        public BElemental GetBData(int index)
        {
            return BData[index];
        }

        public Elemental Get(KeyValuePair<ElementalType, GradeType> keyValuePair)
        {
            return _cache[keyValuePair.Key][keyValuePair.Value];
        }

        public Sprite GetImage(ElementalType type, GradeType gradeType)
        {
            var grade = (int)gradeType;
            if (ResourcesManager.ElementalSprites.TryGetValue(type, out var sprites))
            {
                var clampedGrade = Mathf.Clamp(grade, 0, sprites.Length - 1);
                return sprites[clampedGrade];
            }

            return null;
        }

        public Sprite GetImage(int index)
        {
            var elemental = Get(index);
            return GetImage(elemental.type, elemental.grade);
        }

        public Sprite GetBackCardImage(ElementalType type)
        {
            return ResourcesManager.Instance.backCardImages[Mathf.Clamp((int)type, 0, ResourcesManager.Instance.backCardImages.Length - 1)];
        }
        
        public void GetExpData(Elemental elemental, out int currExp, out int neededNextExp)
        {
            var exp = elemental.exp;
            var level = elemental.Level;
            
            if (ExpCache[level] <= exp)
                exp -= ExpCache[level];

            currExp = exp;
            neededNextExp = level;
        }

        public List<TagType> GetDefaultTags(string key)
        {
            List<TagType> tagTypes = new();
            foreach (var bElementalCombine in CloudData.CloudData.Instance.bElementalCombines)
            {
                if (bElementalCombine.key != key) continue;
                tagTypes.AddRange(bElementalCombine.tags);
                break;
            }

            return tagTypes;
        }
        
        public float GetPowerOfLevelAttrValues(Elemental elemental, AttributeType attrType)
        {
            var bDataForElemental = _bDataCache[elemental.type][elemental.grade];

            var i = 0;
            foreach (var attr in bDataForElemental.possessionEffAttrTypes)
            {
                if (attr == attrType)
                {
                    return Mathf.Pow(bDataForElemental.powerOfLevelAttrValues[i], elemental.Level) * 0.01f;
                }
                ++i;
            }

            return 0;
        }

        public ControllerBullet GetBullet(string key, CancellationTokenSource cts, Transform parent)
        {
            if (string.IsNullOrEmpty(key)) return new ControllerBulletAFC(cts, parent);

            return key switch
            {
                "AFC" => new ControllerBulletAFC(cts, parent),
                "AFB" => new ControllerBulletAFB(cts, parent),
                "AFA" => new ControllerBulletAFA(cts, parent),
                "AFS" => new ControllerBulletAFS(cts, parent),
                "AFSS" => new ControllerBulletAFSS(cts, parent),
                
                "AIC" => new ControllerBulletAIC(cts, parent),
                "AIB" => new ControllerBulletAIB(cts, parent),
                "AIA" => new ControllerBulletAIA(cts, parent),
                "AIS" => new ControllerBulletAIS(cts, parent),
                "AISS" => new ControllerBulletAISS(cts, parent),
                
                "AWC" => new ControllerBulletAWC(cts, parent),
                "AWB" => new ControllerBulletAWB(cts, parent),
                "AWA" => new ControllerBulletAWA(cts, parent),
                "AWS" => new ControllerBulletAWS(cts, parent),
                "AWSS" => new ControllerBulletAWSS(cts, parent),
                
                "ALgC" => new ControllerBulletALgC(cts, parent),
                "ALgB" => new ControllerBulletALgB(cts, parent),
                "ALgA" => new ControllerBulletALgA(cts, parent),
                "ALgS" => new ControllerBulletALgS(cts, parent),
                "ALgSS" => new ControllerBulletALgSS(cts, parent),
                
                "ALC" => new ControllerBulletALC(cts, parent),
                "ALB" => new ControllerBulletALB(cts, parent),
                "ALA" => new ControllerBulletALA(cts, parent),
                "ALS" => new ControllerBulletALS(cts, parent),
                "ALSS" => new ControllerBulletALSS(cts, parent),
                
                "ADC" => new ControllerBulletADC(cts, parent),
                "ADB" => new ControllerBulletADB(cts, parent),
                "ADA" => new ControllerBulletADA(cts, parent),
                "ADS" => new ControllerBulletADS(cts, parent),
                "ADSS" => new ControllerBulletADSS(cts, parent),
                
                "LFC" => new ControllerBulletLFC(cts, parent),
                "LFB" => new ControllerBulletLFB(cts, parent),
                "LFA" => new ControllerBulletLFA(cts, parent),
                "LFS" => new ControllerBulletLFS(cts, parent),
                "LFSS" => new ControllerBulletLFSS(cts, parent),
                
                "LIC" => new ControllerBulletLIC(cts, parent),
                "LIB" => new ControllerBulletLIB(cts, parent),
                "LIA" => new ControllerBulletLIA(cts, parent),
                "LIS" => new ControllerBulletLIS(cts, parent),
                "LISS" => new ControllerBulletLISS(cts, parent),
                
                "LWC" => new ControllerBulletLWC(cts, parent),
                "LWB" => new ControllerBulletLWB(cts, parent),
                "LWA" => new ControllerBulletLWA(cts, parent),
                "LWS" => new ControllerBulletLWS(cts, parent),
                "LWSS" => new ControllerBulletLWSS(cts, parent),
                
                "LLgC" => new ControllerBulletLLgC(cts, parent),
                "LLgB" => new ControllerBulletLLgB(cts, parent),
                "LLgA" => new ControllerBulletLLgA(cts, parent),
                "LLgS" => new ControllerBulletLLgS(cts, parent),
                "LLgSS" => new ControllerBulletLLgSS(cts, parent),
                
                "LLC" => new ControllerBulletLLC(cts, parent),
                "LLB" => new ControllerBulletLLB(cts, parent),
                "LLA" => new ControllerBulletLLA(cts, parent),
                "LLS" => new ControllerBulletLLS(cts, parent),
                "LLSS" => new ControllerBulletLLSS(cts, parent),
                
                "LDC" => new ControllerBulletLDC(cts, parent),
                "LDB" => new ControllerBulletLDB(cts, parent),
                "LDA" => new ControllerBulletLDA(cts, parent),
                "LDS" => new ControllerBulletLDS(cts, parent),
                "LDSS" => new ControllerBulletLDSS(cts, parent),
                
                "PFC" => new ControllerBulletPFC(cts, parent),
                "PFB" => new ControllerBulletPFB(cts, parent),
                "PFA" => new ControllerBulletPFA(cts, parent),
                "PFS" => new ControllerBulletPFS(cts, parent),
                "PFSS" => new ControllerBulletPFSS(cts, parent),
                
                "PIC" => new ControllerBulletPIC(cts, parent),
                "PIB" => new ControllerBulletPIB(cts, parent),
                "PIA" => new ControllerBulletPIA(cts, parent),
                "PIS" => new ControllerBulletPIS(cts, parent),
                "PISS" => new ControllerBulletPISS(cts, parent),
                
                "PWC" => new ControllerBulletPWC(cts, parent),
                "PWB" => new ControllerBulletPWB(cts, parent),
                "PWA" => new ControllerBulletPWA(cts, parent),
                "PWS" => new ControllerBulletPWS(cts, parent),
                "PWSS" => new ControllerBulletPWSS(cts, parent),
                
                "PLgC" => new ControllerBulletPLgC(cts, parent),
                "PLgB" => new ControllerBulletPLgB(cts, parent),
                "PLgA" => new ControllerBulletPLgA(cts, parent),
                "PLgS" => new ControllerBulletPLgS(cts, parent),
                "PLgSS" => new ControllerBulletPLgSS(cts, parent),
                
                "PLC" => new ControllerBulletPLC(cts, parent),
                "PLB" => new ControllerBulletPLB(cts, parent),
                "PLA" => new ControllerBulletPLA(cts, parent),
                "PLS" => new ControllerBulletPLS(cts, parent),
                "PLSS" => new ControllerBulletPLSS(cts, parent),
                
                "PDC" => new ControllerBulletPDC(cts, parent),
                "PDB" => new ControllerBulletPDB(cts, parent),
                "PDA" => new ControllerBulletPDA(cts, parent),
                "PDS" => new ControllerBulletPDS(cts, parent),
                "PDSS" => new ControllerBulletPDSS(cts, parent),
                
                "Rocket" => new ControllerBulletRocket(cts, parent),
                "DropWater" => new ControllerBulletDropWater(cts, parent),
                "DropFire" => new ControllerBulletDropFire(cts, parent),
                "TonadoW" => new ControllerBulletTonadoW(cts, parent),
                "TonadoWWW" => new ControllerBulletTonadoWWW(cts, parent),
                "LightRocket" => new ControllerBulletLightRocket(cts, parent),
                "DarkCircle" => new ControllerBulletDarkCircle(cts, parent),
                "LightingBall" => new ControllerBulletLightingBall(cts, parent),
                
                _ => new ControllerBulletAFC(cts, parent)
            };
        }

        private void Caching()
        {
            elementals ??= new List<Elemental>();
            _cache ??= new Dictionary<ElementalType, Dictionary<GradeType, Elemental>>();
            _bDataCache ??= new Dictionary<ElementalType, Dictionary<GradeType, BElemental>>();

            foreach (var bElemental in BData)
            {
                _bDataCache.TryAdd(bElemental.elementalType, new Dictionary<GradeType, BElemental>());
                _bDataCache[bElemental.elementalType].TryAdd(bElemental.grade, bElemental);
            }

            for (var i = elementals.Count; i < BData.Length; ++i)
            {
                var bElemental = BData[i];
                elementals.Add(new Elemental(bElemental.elementalType, bElemental.grade));
            }

            foreach (var elemental in elementals)
            {
                _cache.TryAdd(elemental.type, new Dictionary<GradeType, Elemental>());
                _cache[elemental.type].TryAdd(elemental.grade, elemental);
                
                elemental.InitAttribute();
            }

            InitExp();

            var firstElementalKeyValue = new KeyValuePair<ElementalType, GradeType>(ElementalType.Fire, GradeType.C);
            if (Get(firstElementalKeyValue).exp == 0)
                Earn(firstElementalKeyValue);
        }

        private void InitExp()
        {
            ExpCache = new int[MaxLevel + 1];
            ExpCache[0] = 0;
            ExpCache[1] = 1;
            var addValue = 1;
            for (var i = 2; i < ExpCache.Length; ++i)
            {
                ExpCache[i] = ExpCache[i - 1] + addValue++;
            }
        }

        public int[] GetAttrLevel(ElementalType elementalType, GradeType gradeType)
        {
            return _bDataCache[elementalType][gradeType].attrLevels;
        }
        public AttributeType[] GetLevelAttrTypes(ElementalType elementalType, GradeType gradeType)
        {
            return _bDataCache[elementalType][gradeType].levelAttrTypes;
        }
        public float[] GetLevelAttrValues(ElementalType elementalType, GradeType gradeType)
        {
            return _bDataCache[elementalType][gradeType].levelAttrValue;
        }

        public Dictionary<AttributeType, List<Attribute>> GetLevelAttribute(Elemental elemental)
        {
            var attributeDict = elemental.levelAttr;
            
            Dictionary<AttributeType, List<Attribute>> valueDict = new();
            foreach (var dict in attributeDict)
            {
                if (!valueDict.TryGetValue(dict.Value.type, out var list))
                {
                    list = new List<Attribute>();
                    valueDict[dict.Value.type] = list;
                }
                list.Add(new Attribute(dict.Value));
            }

            return valueDict;
        }
    }

    [Serializable]
    public class Elemental : IEnhanceable
    {
        public EnhancementType EnhancementType => enhancementType;
        public bool IsEquipped => equippedIndex != -1;
        public bool IsHave => exp > 0;

        public int Level
        {
            get => Mathf.Min(level, DataElemental.MaxLevel);
            set => level = Mathf.Min(value, DataElemental.MaxLevel);
        }

        //interface member
        public GradeType GradeType => grade;
        public int EnhancementLevel => enhancementLevel;
        public int EquippedIndex => equippedIndex;
        public Sprite IconSprite => DataController.Instance.elemental.GetImage(type, grade);
        //----------------

        public ElementalType type;
        public GradeType grade;
        public EquippedPositionType equippedPositionType;
        public EnhancementType enhancementType;

        public int exp;
        public int level;
        public int enhancementLevel;
        public int equippedIndex = -1;

        public List<Attribute> equippingAttr;
        public List<Attribute> possessionAttr;
        //public Dictionary<AttributeType, Dictionary<int, List<Attribute>>> levelAttr;
        public Dictionary<int, Attribute> levelAttr;

        public Elemental() { }

        public Elemental(ElementalType type, GradeType grade)
        {
            this.type = type;
            this.grade = grade;

            enhancementType = EnhancementType.Elemental;
        }
        
        public void Enhance()
        {
            enhancementLevel++;
            var bData = DataController.Instance.elemental.GetBData(type, grade);

            try
            {
                for (var i = 0; i < bData.enhanceEquipAttrIncreaseValues.Length; ++i)
                {
                    equippingAttr[i].value += bData.enhanceEquipAttrIncreaseValues[i];
                }

                for (var i = 0; i < bData.enhancePossessionAttrIncreaseValues.Length; ++i)
                {
                    possessionAttr[i].value += bData.enhancePossessionAttrIncreaseValues[i];
                }
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
            }
        }

        public void LevelUp()
        {
            Level++;
            var bData = DataController.Instance.elemental.GetBData(type, grade);
            try
            {
                for (var i = 0; i < bData.enhanceEquipAttrIncreaseValues.Length; ++i)
                {
                    equippingAttr[i].value = bData.equippingEffAttrValues[i];
                    equippingAttr[i].value += bData.enhanceEquipAttrIncreaseValues[i] * enhancementLevel;
                    equippingAttr[i].value *= 1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(this, equippingAttr[i].type);
                }

                for (var i = 0; i < bData.enhancePossessionAttrIncreaseValues.Length; ++i)
                {
                    possessionAttr[i].value = bData.possessionEffAttrValues[i];
                    possessionAttr[i].value += bData.enhancePossessionAttrIncreaseValues[i] * enhancementLevel;
                    possessionAttr[i].value *= 1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(this, equippingAttr[i].type);
                }
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
            }
        }

        public void InitAttribute()
        {
            equippingAttr = new List<Attribute>();
            possessionAttr = new List<Attribute>();
            levelAttr = new Dictionary<int, Attribute>();
            
            var bDataForElemental = DataController.Instance.elemental.GetBData(type, grade);
            for (var i = 0; i < bDataForElemental.equippingEffAttrTypes.Length; ++i)
            {
                var attribute = new Attribute(bDataForElemental.equippingEffAttrTypes[i], bDataForElemental.equippingEffAttrValues[i]);
                attribute.value += bDataForElemental.enhanceEquipAttrIncreaseValues[i] * enhancementLevel;
                attribute.value *= 1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(this, attribute.type);
                equippingAttr.Add(attribute);
            }
            
            for (var i = 0; i < bDataForElemental.possessionEffAttrTypes.Length; ++i)
            {
                var attribute = new Attribute(bDataForElemental.possessionEffAttrTypes[i], bDataForElemental.possessionEffAttrValues[i]);
                attribute.value += bDataForElemental.enhancePossessionAttrIncreaseValues[i] * enhancementLevel;
                attribute.value *= 1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(this, attribute.type);
                possessionAttr.Add(attribute);
            }

            for (var i = 0; i < bDataForElemental.attrLevels.Length; ++i)
            {
                var attrLevel = bDataForElemental.attrLevels[i];
                var attrType = bDataForElemental.levelAttrTypes[i];
                var attrValue = bDataForElemental.levelAttrValue[i];

                levelAttr[attrLevel] = new Attribute(attrType, attrValue);

                // if (!levelAttr.TryGetValue(attrType, out var typeDict))
                // {
                //     typeDict = new Dictionary<int, List<Attribute>>();
                //     levelAttr[attrType] = typeDict;
                // }
                //
                // if (!typeDict.TryGetValue(attrLevel, out var list))
                // {
                //     list = new List<Attribute>();
                //     typeDict[attrLevel] = list;
                // }
                //
                // list.Add(new Attribute(attrType, attrValue));
            }
        }
    }
}