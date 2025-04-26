using BackEnd;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasSetting : ViewCanvas
    {
        public SlideButton SlideButton => slideButton;
        public Button GoogleLogin => googleLoginButton;
        public Button AppleLogin => appleLoginButton;
        public Button CloudSaveButton => cloudSaveButton;
        public Button LogoutButton => logoutButton;
        public Button AccountDeleteButton => accountDeleteButton;
        public Button PrivacyPolicyButton => privacyPolicyButton;
        public Button TermsOfUseButton => termsOfUseButton;
        public Button SendMailButton => sendMailButton;
        public Button OslButton => oslButton;
        public Button OslCloseButton => oslCloseButton;
        
        public SwitchButton IsBGMSwitchButton => isBGMSwitchButton;
        public SwitchButton IsSFXSwitchButton => isSFXSwitchButton;
        public SwitchButton IsCameraShakeSwitchButton => isCameraShakeSwitchButton;
        public SwitchButton IsAutoSleepSwitchButton => isAutoSleepSwitchButton;
        public Button CopyUuidButton => copyUuidButton;
        public TMP_Text UuidTMP => uuidTMP;
        public CheckBox Frame30CheckBox => frame30CheckBox;
        public CheckBox Frame60CheckBox => frame60CheckBox;
        
        
        [SerializeField] private GameObject[] panels;
        [SerializeField] private SlideButton slideButton;
        [SerializeField] private GameObject oslPaenl;

        [Space] [Space] [Header("DefaultSetting")]
        [SerializeField] private SwitchButton isBGMSwitchButton;
        [SerializeField] private SwitchButton isSFXSwitchButton;
        [SerializeField] private SwitchButton isCameraShakeSwitchButton;
        [SerializeField] private SwitchButton isAutoSleepSwitchButton;
        [SerializeField] private CheckBox frame30CheckBox;
        [SerializeField] private CheckBox frame60CheckBox;
        
        [Space] [Space] [Header("Account")] 
        [SerializeField] private Button googleLoginButton;
        [SerializeField] private Button appleLoginButton;
        [SerializeField] private Button cloudSaveButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button accountDeleteButton;
        [SerializeField] private Button sendMailButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button termsOfUseButton;
        [SerializeField] private Button oslButton;
        [SerializeField] private Button oslCloseButton;
        [SerializeField] private Image googleLoginCheckImage;
        [SerializeField] private Image appleLoginCheckImage;
        [SerializeField] private TMP_Text uuidTMP;
        [SerializeField] private Button copyUuidButton;
        
        public ViewCanvasSetting SetChangePanel(int index)
        {
            panels[0].SetActive(index == 0);
            panels[1].SetActive(index == 1);
            return this;
        }

        public ViewCanvasSetting SetActiveOSLPanel(bool flag)
        {
            oslPaenl.SetActive(flag);
            return this;
        }

        public ViewCanvasSetting UpdateLoginButtonView(FederationType federationType)
        {
            SetGoogleLogin(federationType == FederationType.Google);
            SetAppleLogin(federationType == FederationType.Apple);
            return this;
        }

        public ViewCanvasSetting SetGoogleLogin(bool isLogin)
        {
            googleLoginCheckImage.enabled = isLogin;
            googleLoginButton.gameObject.SetActive(isLogin);
            return this;
        }

        public ViewCanvasSetting SetAppleLogin(bool isLogin)
        {
            appleLoginCheckImage.enabled = isLogin;
            appleLoginButton.gameObject.SetActive(isLogin);
            return this;
        }
    }
}