using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public double maxCombatPower;
        public DataPlayer player;
    }

    [Serializable]
    public class DataPlayer
    {
        public UnityAction<string> OnChangedNickname;
        public UnityAction<int> OnBindChangedElemental;
        public UnityAction<int> OnBindChangedRune;
        public UnityAction<double> OnBindChangeTotalCombat;
        public UnityAction OnBindChangedHp;
        public UnityAction<StageType> OnBindDie;
        public double CurrCombatPower => _currCombatPower;
        public double MaxCombatPower => DataController.Instance.maxCombatPower;

        public float TimeScale =>
            1
            + DataController.Instance.buff.GetValueIfBuffOn(AttributeType.GameSpeed)
            + (DataController.Instance.setting.isGameSpeedUp ? DataController.Instance.setting.GameSpeed : 0)
            + DataController.Instance.research.GetValue(ResearchType.IncreaseGameSpeed)
            + (float)DataController.Instance.good.GetValue(GoodType.IncreaseGameSpeed);

        public bool usedNicknameChangeChance;
        public bool usedGameSpeedFreeOnce;

        public int currPresetIndex;
        public List<PresetInfo> _presetInfos;
        public double[] dpsContainer = new double[9];
        public double totalDps;
        public string protectedEndTimeString;
        
        public double MaxHp
        {
            get => _maxHp;
            set => _maxHp = Math.Max(100, value);
        }

        public double CurrHp
        {
            get => _currHp;
            set => _currHp = Math.Clamp(value, 0, MaxHp);
        }

        private double _maxHp;
        private double _currHp;
        private double _currCombatPower;
        private List<EquippedProjectorInfo> _equippedInfos;

        private int _maxPresetIndex = 5;

        public void Init()
        {
            Caching();
            MainTask().Forget();

            UpdateHp();
            ResetHp();

            DataController.Instance.upgrade.onBindUpgrade[UpgradeType.IncreaseMaxHp] += (_) => UpdateHp();
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseBaseAtkPower] += UpdateAttributeAll;
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseAttackSpeed] += UpdateAttributeAll;
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseCriticalRate] += UpdateAttributeAll;
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseCriticalPower] += UpdateAttributeAll;
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseTotalAtkRate] += UpdateAttributeAll;
            
            DataController.Instance.research.OnBindResearch[ResearchType.IncreaseMaxHp] += UpdateHp;
            
            
            DataController.Instance.buff.OnBindUpdateBuff += UpdateAttributeAll;
            DataController.Instance.buff.OnBindGameSpeed += UpdateGameSpeed;

            StageManager.Instance.onBindChangeStageType += (_, _) => ResetHp();
        }

        #region Get

        public bool IsProtected()
        {
            return ServerTime.IsRemainingTimeUntilDisable(protectedEndTimeString);
        }

        public Color GetProjectorUnitColor(int projectorIndex)
        {
            var dataAttribute = DataController.Instance.attribute;
            if (dataAttribute.HasTag(TagType.Fire, projectorIndex)) return ResourcesManager.ElementalColor[ElementalType.Fire];
            if (dataAttribute.HasTag(TagType.Water, projectorIndex)) return ResourcesManager.ElementalColor[ElementalType.Water];
            if (dataAttribute.HasTag(TagType.Wind, projectorIndex)) return ResourcesManager.ElementalColor[ElementalType.Wind];
            if (dataAttribute.HasTag(TagType.Lighting, projectorIndex)) return ResourcesManager.ElementalColor[ElementalType.Lighting];
            if (dataAttribute.HasTag(TagType.Light, projectorIndex)) return ResourcesManager.ElementalColor[ElementalType.Light];
            return dataAttribute.HasTag(TagType.Darkness, projectorIndex) ? ResourcesManager.ElementalColor[ElementalType.Darkness] : Color.white;
        }

        public TimeSpan GetProtectRemainTimeSpan()
        {
            return ServerTime.RemainingTimeToTimeSpan(protectedEndTimeString);
        }
        
        public Elemental GetEquippedElemental(int projectorIndex, EquippedPositionType positionType)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].EquippedElementals[positionType]
                : null;
        }

        public Rune GetEquippedRune(int projectorIndex, EquippedPositionType positionType)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].EquippedRunes[positionType]
                : null;
        }

        public List<Elemental> GetEquippedElementals(int projectorIndex)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].EquippedElementals.Values.ToList()
                : null;
        }

        public List<Rune> GetEquippedRunes(int projectorIndex)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].EquippedRunes.Values.ToList()
                : null;
        }

        public string GetElementalKey(int projectorIndex, EquippedPositionType positionType)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].EquippedElementalKeys[positionType]
                : string.Empty;
        }

        public double GetAttackPower(int projectorIndex)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].AttackPower
                : 0;
        }

        public float GetCriticalRate(int projectorIndex)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].CriticalRate
                : 0;
        }

        public float GetCriticalDamage(int projectorIndex)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].CriticalDamage
                : 0;
        }

        public float GetAttackSpeed(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].AttackSpeed[equippedPositionType]
                : 0;
        }

        public float GetAttackCountForSecond(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].AttackCountForSeconds[equippedPositionType]
                : 0;
        }

        public float GetBulletDurationTime(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].DurationTimes[equippedPositionType]
                : 0;
        }

        public float GetBulletSize(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            return InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].Sizes[equippedPositionType]
                : 0;
        }

        #endregion

        public void SetProtectionTime(int addHour)
        {
            protectedEndTimeString = ServerTime.DateTimeToIsoString(ServerTime.Date.AddHours(addHour));
            var param = new Param { { RaidData.ProtectEndTimeToStringKey, protectedEndTimeString } };
            DataController.Instance.raid.TryUpdateMyRaidData(param);
        }

        #region Equippment

        public void EquipElemental(int projectorIndex, EquippedPositionType positionType, Elemental elemental)
        {
            UnEquipElemental(projectorIndex, positionType, false);
            elemental.equippedIndex = projectorIndex;
            elemental.equippedPositionType = positionType;

            _equippedInfos[projectorIndex].EquippedElementals[positionType] = elemental;

            DataController.Instance.quest.Count(QuestType.EquipElemental);
            if (projectorIndex == 1)
                DataController.Instance.quest.Count(QuestType.EquipElementalWithProjector);

            OnBindChangedElemental?.Invoke(projectorIndex);
        }

        public void UnEquipElemental(int projectorIndex, EquippedPositionType positionType, bool isCallback = true)
        {
            var elemental = GetEquippedElemental(projectorIndex, positionType);
            if (elemental == null) return;

            elemental.equippedIndex = -1;
            _equippedInfos[projectorIndex].EquippedElementals[positionType] = null;

            if (isCallback)
                OnBindChangedElemental?.Invoke(projectorIndex);
        }

        public void EquipRune(int projectorIndex, EquippedPositionType positionType, Rune rune)
        {
            UnEquipRune(projectorIndex, positionType, false);
            rune.equippedIndex = projectorIndex;
            rune.equippedPositionType = positionType;

            _equippedInfos[projectorIndex].EquippedRunes[positionType] = rune;

            DataController.Instance.quest.Count(QuestType.EquipRune);
            if (projectorIndex == 1)
                DataController.Instance.quest.Count(QuestType.EquipRuneWithProjector);

            OnBindChangedRune?.Invoke(projectorIndex);
        }

        public void UnEquipRune(int projectorIndex, EquippedPositionType positionType, bool isCallback = true)
        {
            var rune = GetEquippedRune(projectorIndex, positionType);
            if (rune == null) return;
            rune.equippedIndex = -1;
            _equippedInfos[projectorIndex].EquippedRunes[positionType] = null;

            if (isCallback)
                OnBindChangedRune?.Invoke(projectorIndex);
        }

        #endregion

        public void ChangePreset(int index)
        {
            index = Mathf.Clamp(index, 0, _maxPresetIndex);
            var presetInfo = _presetInfos[index];

            if (presetInfo.IsEmpty)
            {
                ControllerCanvas.Get<ControllerCanvasToastMessage>().SetToastMessage(
                    LocalizeManager.GetText(LocalizedTextType.PresetSaveTitle),
                    LocalizeManager.GetText(LocalizedTextType.PresetIsEmptyDesc),
                    LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                    LocalizeManager.GetText(LocalizedTextType.Confirm), () =>
                    {
                        SavePreset(currPresetIndex);
                        SavePreset(index);
                        LoadPreset(index);
                        currPresetIndex = index;
                    }).ShowToastMessage();
                return;
            }
            
            SavePreset(currPresetIndex);
            LoadPreset(index);
            currPresetIndex = index;
            
            UpdateTotalCombat();
        }

        private void SavePreset(int index)
        {
            var i = 0;
            foreach (var equippedProjectorInfo in _equippedInfos)
            {
                foreach (EquippedPositionType positionType in Enum.GetValues(typeof(EquippedPositionType)))
                {
                    _presetInfos[index].Add(equippedProjectorInfo.EquippedElementals.GetValueOrDefault(positionType, null),
                        equippedProjectorInfo.EquippedRunes.GetValueOrDefault(positionType, null), i, positionType);
                }
                ++i;
            }
        }

        private void LoadPreset(int index)
        {
            var presetInfo = _presetInfos[index];
            for (var i = 0; i < 9; ++i)
            {
                var elemental = presetInfo.elementals[i];
                var rune = presetInfo.runes[i];
                var projectorIndex = presetInfo.projectorIndexes[i];
                var equippedPositionType = presetInfo.equippedPositionTypes[i];
                var isEquipedElemental = presetInfo.isEquipedElemental[i];
                var isEquipedRune = presetInfo.isEquipedRune[i];
                
                if (!isEquipedElemental)
                    UnEquipElemental(projectorIndex, equippedPositionType);
                else 
                    EquipElemental(projectorIndex, equippedPositionType, elemental);

                if (!isEquipedRune)
                    UnEquipRune(projectorIndex, equippedPositionType);
                else 
                    EquipRune(projectorIndex, equippedPositionType, rune);
            }
        }

        #region Update

        private void UpdateAttributeCommon()
        {
            UpdateHp();
        }

        private void UpdateAttributeEach(int projectorIndex)
        {
            foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
            {
                _equippedInfos[projectorIndex].TotalAttribute[attributeType]
                    = DataController.Instance.attribute.TotalAttributes[projectorIndex][attributeType];
            }

            UpdateAttackPower(projectorIndex);
            UpdateCriticalDamage(projectorIndex);
            UpdateCriticalRate(projectorIndex);

            foreach (EquippedPositionType equippedPositionType in Enum.GetValues(typeof(EquippedPositionType)))
            {
                UpdateAttackCountForSecond(projectorIndex, equippedPositionType);
                UpdateBulletSize(projectorIndex, equippedPositionType);
                UpdateBulletDurationTime(projectorIndex, equippedPositionType);
                UpdateAttackSpeed(projectorIndex, equippedPositionType);
            }
        }

        public void UpdateAttribute(int projectorIndex)
        {
            UpdateAttributeEach(projectorIndex);
            UpdateAttributeCommon();
        }

        public void UpdateAttributeAll()
        {
            for (var i = 0; i < 3; ++i)
            {
                UpdateAttributeEach(i);
            }

            UpdateTotalCombat();
        }

        private void UpdateGameSpeed()
        {
            Time.timeScale = TimeScale;
        }

        public void UpdateEquippedElementalKey(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            var sb = string.Empty;
            var elemental = InEquippedInfoCount(projectorIndex)
                ? _equippedInfos[projectorIndex].EquippedElementals.GetValueOrDefault(equippedPositionType, null)
                : null;

            if (elemental == null)
            {
                _equippedInfos[projectorIndex].EquippedElementalKeys[equippedPositionType] = string.Empty;
                return;
            }

            sb = equippedPositionType switch
            {
                EquippedPositionType.Active => "A",
                EquippedPositionType.Link => "L",
                EquippedPositionType.Passive => "P",
                _ => ""
            } + elemental.type switch
            {
                ElementalType.Fire => "F",
                ElementalType.Water => "I",
                ElementalType.Wind => "W",
                ElementalType.Lighting => "Lg",
                ElementalType.Light => "L",
                ElementalType.Darkness => "D",
                _ => ""
            } + elemental.grade;

            _equippedInfos[projectorIndex].EquippedElementalKeys[equippedPositionType] = sb;
        }

        private void UpdateEquippedElementalKey(int projectorIndex)
        {
            var list = new List<int>();
            list.AddRange(
                from equippedElemental in GetEquippedElementals(projectorIndex)
                where equippedElemental != null
                select (int)equippedElemental.type);

            if (list.Count == 0)
                _equippedInfos[projectorIndex].EquippedElementalKeys[0] = string.Empty;

            list.Sort();
            var sb = new StringBuilder();

            foreach (var typeToInt in list)
            {
                switch (typeToInt)
                {
                    case 0:
                        sb.Append("F");
                        break;
                    case 1:
                        sb.Append("I");
                        break;
                    case 2:
                        sb.Append("W");
                        break;
                    case 3:
                        sb.Append("Lg");
                        break;
                    case 4:
                        sb.Append("L");
                        break;
                    case 5:
                        sb.Append("D");
                        break;
                    default:
                        sb.Append("");
                        break;
                }
            }

            _equippedInfos[projectorIndex].EquippedElementalKeys[0] = sb.ToString();
        }

        public void UpdateTotalCombat()
        {
            // **Notion : Damage Calculator
            var totalCombat = _equippedInfos.Sum(projectorInfo =>
                (projectorInfo.AttackPower * (1 - projectorInfo.CriticalRate)
                 + projectorInfo.AttackPower * projectorInfo.CriticalRate * projectorInfo.CriticalDamage)
                * Mathf.Max(projectorInfo.AttackSpeed[EquippedPositionType.Active], 1));

            OnBindChangeTotalCombat?.Invoke(totalCombat);
            _currCombatPower = totalCombat;
            DataController.Instance.maxCombatPower = Math.Max(_currCombatPower, DataController.Instance.maxCombatPower);
        }

        private void UpdateAttackPower(int projectorIndex)
        {
            var tags = DataController.Instance.attribute.GetHasTags(projectorIndex);
            var atk =
                DataController.Instance.attribute.TotalAttributes[projectorIndex][AttributeType.Atk]
                + DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseBaseAtkPower)
                + DataController.Instance.research.GetValue(ResearchType.IncreaseBaseAtkPower);

            var addAtk = 
                1 + DataController.Instance.attribute.TotalAttributes[projectorIndex][AttributeType.AddAtk]
                  + DataController.Instance.buff.GetValueIfBuffOn(AttributeType.AddAtk);

            var elementalAtk = tags
                .Select(tag => GetAttributeAtkTypeFromTag(tag, false))
                .Where(attributeType => attributeType != AttributeType.None)
                .Sum(attributeType =>
                    DataController.Instance.attribute.TotalAttributes[projectorIndex][attributeType]);

            var elementalAddAtk = 1 + tags
                .Select(tag => GetAttributeAtkTypeFromTag(tag, true))
                .Where(attributeType => attributeType != AttributeType.None)
                .Sum(attributeType =>
                    DataController.Instance.attribute.TotalAttributes[projectorIndex][attributeType]);

            var totalAddAtk = 
                1 + DataController.Instance.attribute.TotalAttributes[projectorIndex][AttributeType.Amplify] 
                  + DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseTotalAtkRate) 
                  + DataController.Instance.research.GetValue(ResearchType.IncreaseTotalAtkRate);

            var totalPower =
                (atk * addAtk + elementalAtk * elementalAddAtk) * totalAddAtk;

            _equippedInfos[projectorIndex].TotalAttribute[AttributeType.Atk] = atk;
            _equippedInfos[projectorIndex].TotalAttribute[AttributeType.AddAtk] = addAtk;
            _equippedInfos[projectorIndex].TotalAttribute[AttributeType.Amplify] = totalAddAtk;
            _equippedInfos[projectorIndex].AttackPower = totalPower;
        }

        private void UpdateCriticalRate(int projectorIndex)
        {
            var criticalRate =
                DataController.Instance.attribute.TotalAttributes[projectorIndex][AttributeType.CriticalRate]
                + DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseCriticalRate)
                + DataController.Instance.research.GetValue(ResearchType.IncreaseCriticalRate);
            
            _equippedInfos[projectorIndex].TotalAttribute[AttributeType.CriticalRate] = criticalRate;
            _equippedInfos[projectorIndex].CriticalRate = (float)criticalRate;
        }

        private void UpdateCriticalDamage(int projectorIndex)
        {
            var criticalDamage = 
                1 
                + DataController.Instance.attribute.TotalAttributes[projectorIndex][AttributeType.CriticalDamage] 
                + DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseCriticalPower)
                + DataController.Instance.research.GetValue(ResearchType.IncreaseCriticalPower);
            
            _equippedInfos[projectorIndex].TotalAttribute[AttributeType.CriticalDamage] = criticalDamage;
            _equippedInfos[projectorIndex].CriticalDamage = (float)criticalDamage;
        }

        private void UpdateAttackSpeed(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            var key = _equippedInfos[projectorIndex].EquippedElementalKeys[equippedPositionType];

            var attackSpeed = DataController.Instance.elementalCombine.GetAttackSpeed(key);
            if (equippedPositionType == EquippedPositionType.Active)
            {
                var defaultSpeed = 1 
                                   + DataController.Instance.attribute.TotalAttributes[projectorIndex][AttributeType.AttackSpeed] 
                                   + DataController.Instance.research.GetValue(ResearchType.IncreaseAttackSpeed);
                
                attackSpeed *= defaultSpeed;
                _equippedInfos[projectorIndex].TotalAttribute[AttributeType.AttackSpeed] = defaultSpeed;
            }

            _equippedInfos[projectorIndex].AttackSpeed[equippedPositionType] = attackSpeed;
        }

        private void UpdateAttackCountForSecond(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            var key = _equippedInfos[projectorIndex].EquippedElementalKeys[equippedPositionType];
            var attackCountParSecond = DataController.Instance.elementalCombine.GetAttackCountPerSecond(key);

            _equippedInfos[projectorIndex].AttackCountForSeconds[equippedPositionType] = attackCountParSecond;
        }

        private void UpdateBulletSize(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            var size = DataController.Instance.elementalCombine.GetSize(GetElementalKey(projectorIndex, equippedPositionType));
            size *= 1 + DataController.Instance.attribute.GetTagValueOrDefault(TagType.Expansion, projectorIndex, 0);

            _equippedInfos[projectorIndex].Sizes[equippedPositionType] = size;
        }

        private void UpdateBulletDurationTime(int projectorIndex, EquippedPositionType equippedPositionType)
        {
            var durationTime = DataController.Instance.elementalCombine.GetDuration(GetElementalKey(projectorIndex, equippedPositionType));
            durationTime *= 1 + DataController.Instance.attribute.GetTagValueOrDefault(TagType.Duration, projectorIndex, 0);

            _equippedInfos[projectorIndex].DurationTimes[equippedPositionType] = durationTime;
        }

        private void UpdateHp()
        {
            var hp = 
                100 + DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseMaxHp)
                    + DataController.Instance.research.GetValue(ResearchType.IncreaseMaxHp);
            
            var addHp = 1 + DataController.Instance.buff.GetValueIfBuffOn(AttributeType.AddMaxHp);

            hp *= addHp;
            MaxHp = hp;

            foreach (var equippedProjectorInfo in _equippedInfos)
            {
                equippedProjectorInfo.TotalAttribute[AttributeType.AddMaxHp] = MaxHp;
            }
            
            OnBindChangedHp?.Invoke();
        }

        #endregion

        #region Util

        public void ResetHp()
        {
            CurrHp = MaxHp;
            OnBindChangedHp?.Invoke();
        }

        private bool InEquippedInfoCount(int count)
        {
            if (_equippedInfos == null) return false;
            return count < _equippedInfos.Count;
        }

        private AttributeType GetAttributeAtkTypeFromTag(TagType elementalType, bool isAddAtk)
        {
            if (isAddAtk)
            {
                return elementalType switch
                {
                    TagType.Fire => AttributeType.AddFAtk,
                    TagType.Water => AttributeType.AddIAtk,
                    TagType.Wind => AttributeType.AddWAtk,
                    TagType.Lighting => AttributeType.AddLgAtk,
                    TagType.Light => AttributeType.AddLAtk,
                    TagType.Darkness => AttributeType.AddDAtk,
                    _ => AttributeType.None
                };
            }

            return elementalType switch
            {
                TagType.Fire => AttributeType.FAtk,
                TagType.Water => AttributeType.IAtk,
                TagType.Wind => AttributeType.WAtk,
                TagType.Lighting => AttributeType.LgAtk,
                TagType.Light => AttributeType.LAtk,
                TagType.Darkness => AttributeType.DAtk,
                _ => AttributeType.None
            };
        }

        private AttributeType GetAddAtkTypeFromElementalType(ElementalType elementalType)
        {
            return elementalType switch
            {
                ElementalType.Fire => AttributeType.AddFAtk,
                ElementalType.Water => AttributeType.AddIAtk,
                ElementalType.Wind => AttributeType.AddWAtk,
                ElementalType.Lighting => AttributeType.AddLgAtk,
                ElementalType.Light => AttributeType.AddLAtk,
                ElementalType.Darkness => AttributeType.AddDAtk,
                _ => AttributeType.FAtk
            };
        }

        private async UniTaskVoid MainTask()
        {
            await UniTask.Yield();
            UpdateAttributeAll();

            foreach (var equippedInfo in _equippedInfos)
            {
                foreach (var key in equippedInfo.EquippedElementalKeys.Values)
                {
                    ObjectPoolManager.Instance.CreateBullet(key);
                }
            }
        }

        private void Caching()
        {
            _equippedInfos ??= new List<EquippedProjectorInfo>();
            _presetInfos ??= new List<PresetInfo>();

            for (var i = _presetInfos.Count; i < _maxPresetIndex; ++i)
            {
                _presetInfos.Add(new PresetInfo());
            }
            
            for (var i = 0; i < 3; ++i)
            {
                _equippedInfos.Add(new EquippedProjectorInfo());
            }

            foreach (var elemental in DataController.Instance.elemental.Gets()
                         .Where(elemental => elemental.equippedIndex > -1))
            {
                EquipElemental(elemental.equippedIndex, elemental.equippedPositionType, elemental);
            }

            if (GetEquippedElemental(0, EquippedPositionType.Active) == null)
            {
                var elemental = DataController.Instance.elemental.Get(0);
                EquipElemental(0, EquippedPositionType.Active, elemental);
            }

            foreach (var rune in DataController.Instance.rune.runes.Where(elemental => elemental.equippedIndex > -1))
            {
                EquipRune(rune.equippedIndex, rune.equippedPositionType, rune);
            }
        }

        #endregion

        public double GetTotalAttributeValue(AttributeType type, int projectorIndex)
        {
            return _equippedInfos[projectorIndex].TotalAttribute[type];
        }
        
        private class EquippedProjectorInfo
        {
            public double AttackPower { get; set; }
            public float CriticalRate { get; set; }
            public float CriticalDamage { get; set; }

            public readonly Dictionary<AttributeType, double> TotalAttribute;

            public Dictionary<EquippedPositionType, float> Sizes { get; }
            public Dictionary<EquippedPositionType, float> DurationTimes { get; }
            public Dictionary<EquippedPositionType, float> AttackSpeed { get; }
            public Dictionary<EquippedPositionType, float> AttackCountForSeconds { get; }


            public Dictionary<EquippedPositionType, string> EquippedElementalKeys { get; }
            public Dictionary<EquippedPositionType, Elemental> EquippedElementals { get; }
            public Dictionary<EquippedPositionType, Rune> EquippedRunes { get; }

            public EquippedProjectorInfo()
            {
                EquippedElementalKeys = new Dictionary<EquippedPositionType, string>();
                EquippedElementals = new Dictionary<EquippedPositionType, Elemental>();
                EquippedRunes = new Dictionary<EquippedPositionType, Rune>();
                Sizes = new Dictionary<EquippedPositionType, float>();
                DurationTimes = new Dictionary<EquippedPositionType, float>();
                AttackCountForSeconds = new Dictionary<EquippedPositionType, float>();
                AttackSpeed = new Dictionary<EquippedPositionType, float>();

                TotalAttribute = new Dictionary<AttributeType, double>();

                foreach (var posType in Enum.GetValues(typeof(EquippedPositionType)))
                {
                    EquippedElementals.Add((EquippedPositionType)posType, null);
                    EquippedRunes.Add((EquippedPositionType)posType, null);
                    EquippedElementalKeys.Add((EquippedPositionType)posType, null);
                    Sizes.Add((EquippedPositionType)posType, 0);
                    DurationTimes.Add((EquippedPositionType)posType, 0);
                    AttackCountForSeconds.Add((EquippedPositionType)posType, 0);
                    AttackSpeed.Add((EquippedPositionType)posType, 0);
                }
                
                foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
                {
                    TotalAttribute.Add(attributeType, 0);
                }
            }
        }
    }

    [Serializable]
    public class PresetInfo
    {
        public bool IsEmpty => !isEquipedElemental.Find(x => x) && !isEquipedRune.Find(x => x);
        
        public List<Elemental> elementals = new() { null };
        public List<Rune> runes = new() { null };
        public List<int> projectorIndexes = new();
        public List<EquippedPositionType> equippedPositionTypes = new();
        public List<bool> isEquipedElemental = new();
        public List<bool> isEquipedRune = new();
        
        public PresetInfo()
        {
            
        }

        public void Add(Elemental elemental, Rune rune, int projectorIndex, EquippedPositionType equippedPositionType)
        {
            var index = projectorIndex * 3 + (int)equippedPositionType;
            
            Add(elementals, elemental, index, null);
            Add(runes, rune, index, null);
            Add(projectorIndexes, projectorIndex, index, 0);
            Add(equippedPositionTypes, equippedPositionType, index, EquippedPositionType.Active);
            Add(isEquipedElemental, elemental != null, index, false);
            Add(isEquipedRune, rune != null, index, false);
        }

        private void Add<T>(List<T> list, T element, int index, T defaultElement)
        {
            for (var i = list.Count; i <= index; ++i)
            {
                list.Add(defaultElement);
            }

            list[index] = element;
        }
    }
}