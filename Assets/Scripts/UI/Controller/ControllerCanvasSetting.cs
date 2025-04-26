using System.Threading;
using BackEnd;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Networking;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasSetting : ControllerCanvas
    {
        private const string PrivacyPolicyURL = "https://storage.thebackend.io/ced287481af620f62fb6f79668d80e92dfafd6154b5bafd025dcfb5827f889ed/privacy.html";
        private const string TermsOfUseURL = "https://storage.thebackend.io/ced287481af620f62fb6f79668d80e92dfafd6154b5bafd025dcfb5827f889ed/privacy.html";
        private ViewCanvasSetting View => ViewCanvas as ViewCanvasSetting;

        public ControllerCanvasSetting(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasSetting>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);

            View.IsBGMSwitchButton.OnValueChanged +=
                switchEnabled =>
                {
                    SetSwitchEnable(switchEnabled, ref DataController.Instance.setting.isBGM);
                    DataController.Instance.setting.onBindBGM?.Invoke(switchEnabled);
                };
            View.IsSFXSwitchButton.OnValueChanged +=
                switchEnabled =>
                {
                    SetSwitchEnable(switchEnabled, ref DataController.Instance.setting.isSFX);
                    DataController.Instance.setting.onBindSFX?.Invoke(switchEnabled);
                };
            View.IsCameraShakeSwitchButton.OnValueChanged +=
                switchEnabled => SetSwitchEnable(switchEnabled, ref DataController.Instance.setting.isCameraShake);
            View.IsAutoSleepSwitchButton.OnValueChanged +=
                switchEnabled => SetSwitchEnable(switchEnabled, ref DataController.Instance.setting.isAutoSleep);
            
            View.Frame30CheckBox.Toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    View.Frame30CheckBox.Toggle.enabled = false;
                    
                    View.Frame60CheckBox.Toggle.enabled = true;
                    View.Frame60CheckBox.Toggle.isOn = false;
                    Application.targetFrameRate = 30;
                }
                
                SetSwitchEnable(isOn, ref DataController.Instance.setting.is30fps);
            });
            View.Frame60CheckBox.Toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    View.Frame60CheckBox.Toggle.enabled = false;
                    
                    View.Frame30CheckBox.Toggle.enabled = true;
                    View.Frame30CheckBox.Toggle.isOn = false;
                    Application.targetFrameRate = 60;
                }
                SetSwitchEnable(isOn, ref DataController.Instance.setting.is60fps);
            });
            
            View.IsBGMSwitchButton.SetToggle(DataController.Instance.setting.isBGM);
            View.IsSFXSwitchButton.SetToggle(DataController.Instance.setting.isSFX);
            View.IsCameraShakeSwitchButton.SetToggle(DataController.Instance.setting.isCameraShake);
            View.IsAutoSleepSwitchButton.SetToggle(DataController.Instance.setting.isAutoSleep);
            
            View.Frame30CheckBox.Toggle.isOn = DataController.Instance.setting.is30fps;
            View.Frame60CheckBox.Toggle.isOn = DataController.Instance.setting.is60fps;
            View.Frame30CheckBox.Toggle.enabled = !DataController.Instance.setting.is30fps;
            View.Frame60CheckBox.Toggle.enabled = !DataController.Instance.setting.is60fps;

            Application.targetFrameRate = View.Frame30CheckBox.IsChecked ? 30 : 60;

            View.SlideButton.AddListener(ChangeMenu);
            View.SendMailButton.onClick.AddListener(SendMail);
            View.GoogleLogin.onClick.AddListener(BackendManager.Instance.StartGoogleLogin);
            View.AppleLogin.onClick.AddListener(BackendManager.Instance.StartAppleLogin);
            View.CloudSaveButton.onClick.AddListener(() =>
            {
                var toastController = Get<ControllerCanvasToastMessage>();
                toastController.ShowLoading();
                DataController.Instance.SaveBackendData(isSuccess =>
                {
                    toastController.ShowTransientToastMessage(isSuccess ? LocalizedTextType.UploadMessage : LocalizedTextType.ErrorMessage);
                    toastController.CloseLoading();
                });
            });
            View.PrivacyPolicyButton.onClick.AddListener(() => OpenWebPage(PrivacyPolicyURL));
            View.TermsOfUseButton.onClick.AddListener(() => OpenWebPage(TermsOfUseURL));
            View.OslButton.onClick.AddListener(() => View.SetActiveOSLPanel(true));
            View.OslCloseButton.onClick.AddListener(() => View.SetActiveOSLPanel(false));
            
            View.LogoutButton.onClick.AddListener(() =>
            {
                Get<ControllerCanvasToastMessage>().SetToastMessage(
                    LocalizeManager.GetText(LocalizedTextType.Warring),
                    LocalizeManager.GetText(LocalizedTextType.Warring_Logout),
                    LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                    LocalizeManager.GetText(LocalizedTextType.Confirm),
                    () =>
                    {
                        BackendManager.Logout(isSuccess =>
                        {
                            if (isSuccess)
                            {
                                GameManager.Instance.Pause();
                                Backend.BMember.DeleteGuestInfo();
                                DataController.DeleteFederationLoginInfo();
                                PlayerPrefs.DeleteAll();
                                
                                Get<ControllerCanvasToastMessage>().SetToastMessage(
                                        LocalizeManager.GetText(LocalizedTextType.Warring),
                                        LocalizeManager.GetText(LocalizedTextType.Warring_QuitApplication))
                                    .ShowToastMessage();
                                
                                Utility.ApplicationQuit(2000).Forget();
                            }
                        });
                    })
                    .ShowToastMessage();
            });
            View.AccountDeleteButton.onClick.AddListener(() =>
            {
                Get<ControllerCanvasToastMessage>().SetToastMessage(
                        LocalizeManager.GetText(LocalizedTextType.Warring),
                        LocalizeManager.GetText(LocalizedTextType.Warring_DeleteAccount),
                        LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                        LocalizeManager.GetText(LocalizedTextType.Confirm),
                        () =>
                        {
                            BackendManager.WithdrawAccount(isSuccess =>
                            {
                                if (isSuccess)
                                {
                                    GameManager.Instance.Pause();
                                    Backend.BMember.DeleteGuestInfo();
                                    DataController.DeleteFederationLoginInfo();
                                    PlayerPrefs.DeleteAll();
                                
                                    Get<ControllerCanvasToastMessage>().SetToastMessage(
                                            LocalizeManager.GetText(LocalizedTextType.Warring),
                                            LocalizeManager.GetText(LocalizedTextType.Warring_QuitApplication))
                                        .ShowToastMessage();
                                
                                    Utility.ApplicationQuit(2000).Forget();
                                }
                            });
                        })
                    .ShowToastMessage();
            });
            View.CopyUuidButton.onClick.AddListener(() => {CopyTextToClipboard(BackendManager.Uuid);});
            View.UuidTMP.text = $"UUID : {BackendManager.Uuid}";
            
            BackendManager.onBindAuthorizedFederation += UpdateFederationView;
            
            #if UNITY_IOS
            #elif UNITY_ANDROID
            View.AppleLogin.gameObject.SetActive(false);
            #endif
        }
        
        public override void Open()
        {
            base.Open();
            
            View.SlideButton.OnClick(0);
            View.SetActiveOSLPanel(false);
            var federationLoginType = DataController.GetFederationLoginType();
            if (federationLoginType.HasValue)
                UpdateFederationView(federationLoginType.Value);
        }

        private void ChangeMenu(int index)
        {
            View.SetChangePanel(index);
        }
        
        private void SendMail()
        {
            var email = "contact@themessage.net";
            var subject = EscapeURL("");
            var body = EscapeURL($"ETD\nUUID: {BackendManager.Uuid}");

            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        private void OpenWebPage(string siteURL)
        {
            Application.OpenURL(siteURL);
        }

        private string EscapeURL(string url)
        {
            return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
        }

        private void UpdateFederationView(FederationType type)
        {
            View.UpdateLoginButtonView(type);
        }

        private void SetSwitchEnable(bool switchEnable, ref bool data)
        {
            data = switchEnable;
            DataController.Instance.LocalSave();
        }
        
        private void CopyTextToClipboard(string text)
        {
            GUIUtility.systemCopyBuffer = text;
            Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.CopySuccess);
        }
    }
}