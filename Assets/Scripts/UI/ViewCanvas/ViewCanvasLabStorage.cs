using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasLab
    {
        public Button StorageSave => storageSave;
        public GameObject StorageTutorialPanel0 => tutorialPanel0;
        public GameObject StorageTutorialPanel1 => tutorialPanel1;
        
        [Space] [Space] [Header("Storage")]
        [SerializeField] private TMP_Text createTMP;
        [SerializeField] private TMP_Text darkDiaPerSecTMP;
        [SerializeField] private TMP_Text storageTMP;
        [SerializeField] private TMP_Text saveTimeTMP;
        [SerializeField] private TMP_Text protectedTMP;
        [SerializeField] private TMP_Text storageDesc;
        [SerializeField] private GameObject saveButtonLockPanel;
        [SerializeField] private Button storageSave;
        
        [Space] [Space] [Header("Tutorial")]
        [SerializeField] private GameObject tutorialPanel0;
        [SerializeField] private GameObject tutorialPanel1;

        public ViewCanvasLab SetDarkDiaCreateText(string text)
        {
            createTMP.text = text;
            return this;
        }

        public ViewCanvasLab SetProtectedText(string text)
        {
            protectedTMP.text = text;
            return this;
        }

        public ViewCanvasLab SetStorageDescText(string text)
        {
            storageDesc.text = text;
            return this;
        }

        public ViewCanvasLab SetStorageSaveText(string text)
        {
            storageTMP.text = text;
            return this;
        }

        public ViewCanvasLab SetDarkDiaPerSecText(string text)
        {
            darkDiaPerSecTMP.text = text;
            return this;
        }

        public ViewCanvasLab SetLockSaveButton(bool flag)
        {
            saveButtonLockPanel.SetActive(flag);
            return this;
        }

        public ViewCanvasLab SetSaveTimeText(string text)
        {
            saveTimeTMP.text = text;
            return this;
        }
    }
}