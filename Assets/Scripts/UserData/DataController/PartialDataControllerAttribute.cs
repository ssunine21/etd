using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataAttribute attribute;
    }

    [Serializable]
    public class DataAttribute
    {
        public Dictionary<AttributeType, float>[] TotalAttributes { get; private set; }
        private Dictionary<TagType, bool>[] _hasTags;

        public void Init()
        {
            Caching();
            MainTask().Forget();
            
            DataController.Instance.player.OnBindChangedElemental += UpdateAttributes;
            DataController.Instance.player.OnBindChangedRune += UpdateAttributes;
            DataController.Instance.enhancement.OnBindEnhanced += _ => UpdateAllAttribute();
            DataController.Instance.elemental.OnBindLevelUp += _ => UpdateAllAttribute();
        }

        public float GetTagValueOrDefault(TagType tagType, int projectorIndex, float defaultValue = 0)
        {
            var attrType = Utility.TagTypeToAttributeType(tagType);
            return HasTag(tagType, projectorIndex) ? TotalAttributes[projectorIndex][attrType] : defaultValue;
        }

        public bool HasTag(TagType tagType, int projectorIndex)
        {
            return _hasTags[projectorIndex].GetValueOrDefault(tagType, false);
        }

        public List<TagType> GetHasTags(int projectorIndex)
        {
            return _hasTags[projectorIndex].Where(x => x.Value).Select(x => x.Key).ToList();
        }

        private void UpdateAllAttribute()
        {
            for (var i = 0; i < 3; ++i)
            {
                UpdateAttributes(i);
            }
            DataController.Instance.player.UpdateTotalCombat();
        }

        public float GetValue(AttributeType type, int projectorIndex)
        {
            return TotalAttributes[projectorIndex].GetValueOrDefault(type, 0);
        }

        private void UpdateAttributes(int projectorIndex)
        {
            foreach (EquippedPositionType euqippedPositionType in Enum.GetValues(typeof(EquippedPositionType)))
                DataController.Instance.player.UpdateEquippedElementalKey(projectorIndex, euqippedPositionType);
            
            foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
                TotalAttributes[projectorIndex][attributeType] = 0;

            foreach (TagType tagType in Enum.GetValues(typeof(TagType)))
                _hasTags[projectorIndex][tagType] = false;
            
            AddEquippedElementalAttr(projectorIndex);
            AddEquippedRuneAttr(projectorIndex);
            AddPossessionAttr();

            DataController.Instance.player.UpdateAttribute(projectorIndex);
        }

        private void AddEquippedElementalAttr(int projectorIndex)
        {
            foreach (var equippedElemental in DataController.Instance.player.GetEquippedElementals(projectorIndex))
            {
                if (equippedElemental == null) continue;
                foreach (var equippingAttr in equippedElemental.equippingAttr)
                {
                    TotalAttributes[projectorIndex][equippingAttr.type] +=
                        equippingAttr.value * (1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(equippedElemental, equippingAttr.type));
                }
            }
            
            var key = DataController.Instance.player.GetElementalKey(projectorIndex, EquippedPositionType.Active);
            foreach (var tag in DataController.Instance.elemental.GetDefaultTags(key))
            {
                _hasTags[projectorIndex][tag] = true;
            }
        }

        private void AddEquippedRuneAttr(int projectorIndex)
        {
            foreach (var equippedRune in DataController.Instance.player.GetEquippedRunes(projectorIndex))
            {
                if (equippedRune == null) continue;
                foreach (var equippingAttr in equippedRune.equippingAttr)
                {
                    TotalAttributes[projectorIndex][equippingAttr.type] += equippingAttr.value;
                }

                foreach (var tag in equippedRune.tags)
                {
                    _hasTags[projectorIndex][tag] = true;
                }
            }
        }

        private void AddPossessionAttr()
        {
            foreach (var elemental in DataController.Instance.elemental.Gets())
            {
                if (!elemental.IsHave) continue;
                foreach (var possessionAttr in elemental.possessionAttr)
                {
                    foreach (var totalAttribute in TotalAttributes)
                    {
                        totalAttribute[possessionAttr.type] +=
                            possessionAttr.value * (1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(elemental, possessionAttr.type));   
                    }
                }
            }
        }

        // private void UpdatePossessionAttributes()
        // {
        //     foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
        //     {
        //         PossessionAttributes[attributeType] = 0;
        //     }
        //
        //     foreach (var elemental in DataController.Instance.elemental.Gets())
        //     {
        //         if (!elemental.IsHave) continue;
        //         foreach (var possessionAttr in elemental.possessionAttr)
        //         {
        //             PossessionAttributes[possessionAttr.type] +=
        //                 possessionAttr.value * (1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(elemental, possessionAttr.type));
        //         }
        //     }
        // }
        // private void UpdateEquippingAttribute(int projectorIndex)
        // {
        //     foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
        //     {
        //         EquippingAttributes[projectorIndex][attributeType] = 0;
        //     }
        //     
        //     foreach (var equippedElemental in DataController.Instance.player.GetEquippedElementals(projectorIndex))
        //     {
        //         if (equippedElemental == null) continue;
        //         foreach (var equippingAttr in equippedElemental.equippingAttr)
        //         {
        //             EquippingAttributes[projectorIndex][equippingAttr.type] +=
        //                 equippingAttr.value * (1 + DataController.Instance.elemental.GetPowerOfLevelAttrValues(equippedElemental, equippingAttr.type));
        //         }
        //     }
        //
        //     foreach (var equippedRune in DataController.Instance.player.GetEquippedRunes(projectorIndex))
        //     {
        //         if (equippedRune != null)
        //         {
        //             foreach (var equippingAttr in equippedRune.equippingAttr)
        //             {
        //                 EquippingAttributes[projectorIndex][equippingAttr.type] += equippingAttr.value;
        //             }
        //         }
        //     }
        // }

        private void Caching()
        {
            TotalAttributes = new Dictionary<AttributeType, float>[3];
            for (var i = 0; i < TotalAttributes.Length; ++i)
            {
                var dict = new Dictionary<AttributeType, float>();
                foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType)))
                {
                    dict[attributeType] = 0;
                }

                TotalAttributes[i] = dict;
            }

            _hasTags = new Dictionary<TagType, bool>[3];
            for (var i = 0; i < _hasTags.Length; ++i)
            {
                var dict = new Dictionary<TagType, bool>();
                foreach (TagType attributeType in Enum.GetValues(typeof(TagType)))
                {
                    dict[attributeType] = false;
                }

                _hasTags[i] = dict;
            }
        }

        private async UniTaskVoid MainTask()
        {
            await UniTask.Yield();
            for (var i = 0; i < 3; ++i)
            {
                UpdateAttributes(i);
            }
        }
    }
    

    [Serializable]
    public class Attribute
    {
        public AttributeType type;
        public float value;
        
        public Attribute(){}

        public Attribute(Attribute attribute)
        {
            type = attribute.type;
            value = attribute.value;
        }

        public Attribute(AttributeType type, float val)
        {
            this.type = type;
            value = val;
        }
    }
}