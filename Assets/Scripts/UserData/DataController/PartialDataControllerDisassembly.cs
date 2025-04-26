using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.UI.Common;
using UnityEngine.Events;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataDisassembly disassembly;
    }

    [Serializable]
    public class DataDisassembly
    {
        public UnityAction onBindDisassembly;
        
        public List<bool> typeCheckBoxes;
        public List<bool> gradeCheckBoxes;

        public bool GetTypeCheckBoxOn(int index)
        {
            typeCheckBoxes ??= new List<bool>();
            for (var i = typeCheckBoxes.Count - 1; i < index; ++i)
            {
                typeCheckBoxes.Add(false);
            }

            return typeCheckBoxes[index];
        }
        
        public bool GetGradeCheckBoxIsOn(int index)
        {
            gradeCheckBoxes ??= new List<bool>();
            for (var i = gradeCheckBoxes.Count - 1; i < index; ++i)
            {
                gradeCheckBoxes.Add(false);
            }

            return gradeCheckBoxes[index];
        }

        public void SaveCheckBoxesOn(CheckBox[] types, CheckBox[] grades)
        {
            for (var i = 0; i < typeCheckBoxes.Count; ++i)
            {
                typeCheckBoxes[i] = types[i].Toggle.isOn;
            }
            
            for (var i = 0; i < gradeCheckBoxes.Count; ++i)
            {
                gradeCheckBoxes[i] = grades[i].Toggle.isOn;
            }
        }
    }
}