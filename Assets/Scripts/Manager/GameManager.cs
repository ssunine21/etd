using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID 
using Google.Play.Review;
#endif
namespace ETD.Scripts.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        
        public static int systemLanguageNumber;
        public static string errorMessage;
        public static UnityAction errorAction;
        
        public string appleAppId = "6503336669";
        public string googlePlayAppId = "net.themessage.etd";
        public SystemLanguage systemLanguage;
        
        public bool IsPlaying { get; private set; } = true;
        public bool LoadingComplete { get; private set; } = false;

        private static CancellationTokenSource _cts;
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            Init(new CancellationTokenSource());
            
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Update()
        {
            #if IS_TEST
            if (Input.GetKeyDown(KeyCode.K))
            {

                if (EnemyManager.Instance.TryOverlapCircleAll(Vector2.zero, 100, out var enemies))
                {
                    foreach (var enemy in enemies)
                    {
                        enemy.Damaged(double.MaxValue, true);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                ControllerCanvasTest.IsEnemyInvincibility = !ControllerCanvasTest.IsEnemyInvincibility;
            }
            #endif
        }

        public override void Init(CancellationTokenSource cts)
        {
            _cts = cts;
            LoadData().Forget();
        }

        private async UniTaskVoid LoadData()
        {
            Application.targetFrameRate = 60;
            systemLanguageNumber = Utility.GetSystemLanguageNumber();

            try
            {
                LoadingManager.Instance.Init(_cts);
                LoadingManager.Instance.SetMaxCount(49);
                await UniTask.WaitUntil(() => LoadingManager.Instance.IsInit, PlayerLoopTiming.Update, _cts.Token);
                await LoadingManager.Instance.Loading();
                BackendManager.Instance.Init(_cts);
                IAPManager.Instance.Init();
                
                await LoadingManager.Instance.Loading();
                await UniTask.WaitUntil(() => BackendManager.Instance.IsInit, PlayerLoopTiming.Update, _cts.Token);
                
                await LoadingManager.Instance.Loading();
                BackendManager.Login(2);
                BackendManager.CreateNickname();
                
                #region LoadData
                await LoadingManager.Instance.Loading();
                await DataController.Instance.LoadBackendData();
                
                await LoadingManager.Instance.Loading();
                UserData.CloudData.CloudData.Instance.Load();
                
                await LoadingManager.Instance.Loading();
                DataController.Instance.Init();   
                #endregion
                
                await LoadingManager.Instance.Loading();
                #region LoadManager
                ObjectPoolManager.Instance.Init(_cts);
                MainUnitManager.Instance.Init(_cts);
                EnemyManager.Instance.Init(_cts);
                StageManager.Instance.Init(_cts);
                TextManager.Instance.Init(_cts);
                GoodsEffectManager.Instance.Init(_cts);
                StageGoodEffectManager.Instance.Init(_cts);
                GoogleMobileAdsManager.Instance.Init(_cts);
                AudioManager.Instance.Init(_cts);
                #endregion

                VersionCheck().Forget();
                
                await LoadingManager.Instance.Loading();
                #region LoadController
                ControllerCanvas.Add(new ControllerCanvasTest(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasStage(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasBottomMenu(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasElemental(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasRune(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasSummon(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasMainMenu(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasHp(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasUpgrade(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasToastMessage(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasShop(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasSetting(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasDungeon(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasDungeonStage(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasClear(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasProfile(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasCombatPower(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasEnhance(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasDisassembly(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasMission(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasRanking(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasDefeat(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasPowerSaving(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasMail(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasProbability(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasFreeGifts(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasNewPackage(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasTutorial(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasOfflineReward(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasBuff(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasAttendance(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasFirstPurchase(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasPass(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasElementalInfo(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasLab(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasRelease(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasRaid(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasVip(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasGrowPass(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasGuild(_cts));await LoadingManager.Instance.Loading();
                ControllerCanvas.Add(new ControllerCanvasMyGuild(_cts));await LoadingManager.Instance.Loading();
                #endregion
                
                ServerTime.Init();           
                await LoadingManager.Instance.Loading();
                await UniTask.WaitUntil(() => ServerTime.IsInit);      
                LoadingManager.Instance.SetActive(false);
            }
            catch (Exception ex)
            {
                //TODO Go to Error Scene
                FirebaseManager.LogError(ex);
            }

            LoadingComplete = true;
        }

        public void RequestAppReview(UnityAction<bool> callback)
        {
            try
            {
#if UNITY_ANDROID
            RequestGooglePlayInAppReview(callback).Forget();
#elif UNITY_IOS
                RequestAppStoreInAppReview(callback);
#endif
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
                throw;
            }
        }
        
        #if UNITY_ANDROID
        private ReviewManager _reviewManager;
        private async UniTaskVoid RequestGooglePlayInAppReview(UnityAction<bool> callback)
        {
            _reviewManager = new ReviewManager();
            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            await requestFlowOperation;
            
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                callback?.Invoke(false);
                return;
            }
            var playReviewInfo = requestFlowOperation.GetResult();
            var launchFlowOperation = _reviewManager.LaunchReviewFlow(playReviewInfo);
            
            await launchFlowOperation;
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                callback?.Invoke(false);
                return;
            }
            
            callback?.Invoke(true);
        }
#elif UNITY_IOS
        private void RequestAppStoreInAppReview(UnityAction<bool> callback)
        {
            Device.RequestStoreReview();
            callback?.Invoke(true);
        }
#endif

        private async UniTaskVoid VersionCheck()
        {

#if UNITY_EDITOR
            return;
#endif
            BackendManager.VersionCheck(backendForceUpdate =>
            {
                Utility.LogWithColor($"Backend Version compare: {backendForceUpdate}", Color.green);
                switch (backendForceUpdate)
                {
                    case 1:
                        Utility.LogError("Update Notice");
                        ControllerCanvas.Get<ControllerCanvasToastMessage>()
                            .SetToastMessage(
                                LocalizeManager.GetText(LocalizedTextType.Warring),
                                LocalizeManager.GetText(LocalizedTextType.Warring_UpdateNotice),
                                LocalizeManager.GetText(LocalizedTextType.Cancel),
                                null,
                                LocalizeManager.GetText(LocalizedTextType.Confirm),
                                GoStore)
                            .ShowToastMessage();
                        break;
                    case 2:
                        Pause();
                        Utility.LogError("Update Force");
                        ControllerCanvas.Get<ControllerCanvasToastMessage>()
                            .SetToastMessage(
                                LocalizeManager.GetText(LocalizedTextType.Warring),
                                LocalizeManager.GetText(LocalizedTextType.Warring_UpdateForce),
                                LocalizeManager.GetText(LocalizedTextType.Confirm),
                                GoStore)
                            .ShowToastMessage();
                        break;
                }
            });
        }
        
        private void GoStore()
        {
#if UNITY_IOS
            OpenAppleAppStore();
#elif UNITY_ANDROID
        OpenGooglePlayStore();
#endif
        }
        private void OpenAppleAppStore()
        {
            var url = $"https://apps.apple.com/app/id{appleAppId}";
            Application.OpenURL(url);
        }

        private void OpenGooglePlayStore()
        {
            var url = $"https://play.google.com/store/apps/details?id={googlePlayAppId}";
            Application.OpenURL(url);
        }

        private void OnApplicationQuit()
        {
            try
            {
                _cts.Cancel();
            }
            catch (Exception e)
            {
                #if IS_TEST
                Debug.LogError(e.Message);
                #endif
            }
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Pause()
        {
            IsPlaying = false;
        }
    }
}

