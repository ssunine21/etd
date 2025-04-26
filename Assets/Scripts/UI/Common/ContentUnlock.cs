using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.Common
{
    public class ContentUnlock : MonoBehaviour
    {
        public UnlockType unlockType;
        public GameObject goLockPanel;
        public GameObject go;
        public TMP_Text lockDescTMP;

        private void Awake()
        {
            Init().Forget();
        }

        private async UniTaskVoid Init()
        { 
            await UniTask.WaitUntil(() => DataController.IsInit);
            
            DataController.Instance.contentUnlock.AddListenerUnlock(unlockType, UpdateUnlock);
            DataController.Instance.contentUnlock.AddListenerUnlock(unlockType, UpdateObject);
            
            SetLockText(LocalizeManager.GetText(LocalizedTextType.UnlockCondition, DataController.Instance.contentUnlock.GetUnlockQuestLevel(unlockType)))
                .UpdateUnlock(DataController.Instance.contentUnlock.IsUnLock(unlockType));
        }

        private ContentUnlock SetLockText(string text)
        {
            if (lockDescTMP)
                lockDescTMP.text = text;
            return this;
        }

        private void UpdateUnlock(bool isUnlock)
        {
            if (goLockPanel)
                goLockPanel.SetActive(!isUnlock);
        }

        private void UpdateObject(bool isUnlock)
        {
            if (go)
                go.SetActive(isUnlock);
        }
    }
}