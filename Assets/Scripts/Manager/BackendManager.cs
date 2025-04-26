using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BackEnd;
using ETD.Scripts.Common;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ETD.Scripts.Manager
{
    public class BackendManager : Singleton<BackendManager>
    {
        public bool IsInit { get; private set; }

        public static string Uuid
        {
            get
            {
                var userInfo = Backend.BMember.GetUserInfo();
                var userInfoJson = userInfo.GetReturnValuetoJSON()["row"];
                return userInfoJson["gamerId"].ToString();
            }
        }
        public static UnityAction<FederationType> onBindAuthorizedFederation;

        private delegate void BackendFunction(int maxRepeatCount);

        public override void Init(CancellationTokenSource cts)
        {
            var bro = Backend.Initialize();
            if (bro.IsSuccess())
            {
                IsInit = true;
                Utility.LogWithColor($"Initialize Success : {bro.GetMessage()}", Color.yellow);

                if (Backend.IsInitialized)
                {
                    Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = OnOtherDeviceLoginDetectedError;
                    Backend.ErrorHandler.OnMaintenanceError = OnMaintenanceError;
                    Backend.ErrorHandler.OnDeviceBlockError = () => OnDeviceBlockError(Uuid);

                    Utility.LogWithColor($"Google hash key: {Backend.Utils.GetGoogleHash()}", Color.yellow);
                }
            }
            else
            {
                Utility.LogError($"Initialize Fail : {bro}");
            }
        }

        public static void Login(int maxRepeatCount)
        {
            if (maxRepeatCount <= 0)
                OnError();

            Utility.LogWithColor($"Login Start", Color.yellow);
            var bro = AccessTokenLogin();
            if (bro != null)
            {
                if (bro.IsSuccess())
                {
                    Utility.LogWithColor($"Access Token Login... Success!!", Color.green);
                    var userInfo = Backend.BMember.GetUserInfo();
                    var userInfoJson = userInfo.GetReturnValuetoJSON()["row"];
                    Utility.LogWithColor($"Guest Login Success({bro.GetStatusCode()}): {bro}\n" +
                                         $"User NickName: {userInfoJson["nickname"]}\n" +
                                         $"User InDate: {userInfoJson["inDate"]}\n" +
                                         $"User UUID: {userInfoJson["gamerId"]}", Color.green);
                }
                else
                {
                    Utility.LogWithColor($"Access Token Login... Fails!!", Color.red);
                    Utility.LogWithColor($"{bro.GetStatusCode()}\n{bro.GetMessage()}", Color.yellow);
                    if (IsCommonError(bro))
                    {
                        Login(maxRepeatCount - 1);
                    }
                    else
                    {
                        var getMessage = bro.GetMessage();
                        switch (bro.GetStatusCode())
                        {
                            case "400":
                                if (getMessage.Contains("accessToken not exist")
                                    || getMessage.Contains("undefined refresh_token"))
                                    GuestLogin(3);
                                break;
                            case "401":
                                if(getMessage.Contains("bad refreshToken"))
                                {
                                    if (DataController.GetFederationLoginType().HasValue
                                        && !string.IsNullOrEmpty(DataController.GetFederationLoginToken()))
                                    {
                                        var key = DataController.GetFederationLoginType().GetValueOrDefault(FederationType.Google);
                                        var token = DataController.GetFederationLoginToken();
                                        if (CheckUserInBackend(token, key))
                                        {
                                            AuthorizeFederation(token, key);
                                        }
                                    }
                                    GuestLogin(3);
                                }
                                break;
                            case "403":
                                if(getMessage.Contains("blocked user"))
                                    OnDeviceBlockError(bro.GetErrorData()["uuid"].ToString());
                                break;
                            default:
                                OnError();
                                break;
                        }
                    }
                }
            }
        }

        private static BackendReturnObject CustomLogin(string id)
        {
            var bro = Backend.BMember.CustomLogin(id, "");
            return bro;
        }

        private static BackendReturnObject AccessTokenLogin()
        {
            var bro = Backend.BMember.LoginWithTheBackendToken();
            return bro;
        }

        private static void GuestLogin(int maxRepeatCount)
        {
            if (maxRepeatCount <= 0)
            {
                OnError();
                return;
            }
            
            var bro = Backend.BMember.GuestLogin("Guest Loging");
            if (bro.IsSuccess())
            {
                Utility.LogWithColor($"Guest Login Success({bro.GetStatusCode()}): {bro}", Color.yellow);
                Utility.LogWithColor($"User NickName: {Backend.UserNickName}", Color.yellow);
                Utility.LogWithColor($"User Update: {Backend.UserInDate}", Color.yellow);
                Utility.LogWithColor($"User UID: {Backend.UID}", Color.yellow);
            }
            else
            {
                if (IsCommonError(bro))
                {
                    
                }
                else
                {
                    Utility.LogError($"Guest Login Fail: {bro.GetStatusCode()}-{bro.GetMessage()}");
                    switch (bro.GetStatusCode())
                    {
                        case "401":
                            if (bro.GetMessage().Contains("bad customId"))
                                Utility.LogWithColor("Delete Guest Info...", Color.yellow);

                            Backend.BMember.DeleteGuestInfo();
                            GuestLogin(maxRepeatCount - 1);
                            return;
                        case "403":
                            if (bro.GetMessage().Contains("Forbidden blocked user")
                                || bro.GetMessage().Contains("Forbidden blocked device"))
                                OnDeviceBlockError(bro.GetErrorData()["uuid"].ToString());
                            return;
                    }

                    Utility.LogError("Guest Login Error");
                    OnError();
                }
            }
        }

        public void StartGoogleLogin()
        {
            StartGoogleLoginTask();
        }
 
        public void StartAppleLogin()
        {
            StartAppleLoginTask();
        }

        private void StartGoogleLoginTask()
        {
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowLoading();
            
#if UNITY_ANDROID
            TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(true, (success, message, token) =>
            {
                StartCoroutine(GoogleLoginCallback(success, message, token));
                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
            });
#elif UNITY_IOS
            TheBackend.ToolKit.GoogleLogin.iOS.GoogleLogin((success, message, token) =>
            {
                StartCoroutine(GoogleLoginCallback(success, message, token));
                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
            });
#endif
        }

        private void StartAppleLoginTask()
        {
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowLoading();
            AppleLoginManager.Instance.SigninWithApple((isSuccess, message, token) =>
            {
                StartCoroutine(AppleLoginCallback(isSuccess, message, token));
                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
            });
        }

        private readonly WaitForSeconds _wfs = new (0.3f);
        private IEnumerator GoogleLoginCallback(bool isSuccess, string errorMessage, string token)
        {
            yield return _wfs;
            if (!isSuccess)
            {
                ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Error_GoogleLoginFails);
                Utility.LogError($"Google Login is Fail: {errorMessage}");
                yield break;
            }

            MigrationFederation(token, FederationType.Google);

        }

        private IEnumerator AppleLoginCallback(bool isSuccess, string errorMessage, string token)
        {
            yield return _wfs;
            if (!isSuccess)
            {
                ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Error_AppleLoginFails);
                Utility.LogError($"Google Login is Fail: {errorMessage}");
                yield break;
            }

            MigrationFederation(token, FederationType.Apple);
        }

        private void MigrationFederation(string token, FederationType type)
        {
            try
            {
                Utility.LogWithColor($"Login Token: {token}", Color.yellow);
                if (CheckUserInBackend(token, type))
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>()
                        .SetToastMessage(
                            LocalizeManager.GetText(LocalizedTextType.Warring),
                            LocalizeManager.GetText(LocalizedTextType.hasFederationDataMessage),
                            LocalizeManager.GetText(LocalizedTextType.Cancel), () => { },
                            LocalizeManager.GetText(LocalizedTextType.Load),
                            () =>
                            {
                                AuthorizeFederation(token, type, 1, (isSuccess) =>
                                {
                                    if(isSuccess)
                                    {
                                        GameManager.Instance.Pause();
                                        PlayerPrefs.DeleteAll();
                                        
                                        ControllerCanvas.Get<ControllerCanvasToastMessage>().SetToastMessage(
                                                LocalizeManager.GetText(LocalizedTextType.Warring),
                                                LocalizeManager.GetText(LocalizedTextType.Warring_QuitApplication))
                                            .ShowToastMessage();
                                        
                                        onBindAuthorizedFederation?.Invoke(type);
                                        DataController.SaveFederationLoginInfo(token, type);
                                        
                                        Utility.ApplicationQuit(2000).Forget();
                                    }
                                });
                            })
                        .ShowToastMessage();
                }
                else
                {
                    ChangeCustomToFederation(token, type);
                }
            }
            catch (NullReferenceException e)
            {
                Utility.LogWithColor(e.Message, Color.red);
                ControllerCanvas.Get<ControllerCanvasToastMessage>()
                    .ShowTransientToastMessage(LocalizedTextType.Error_LoginFails);
            }
        }



        public static void CreateNickname(int count = 0, UnityAction callBack = null)
        {
            if (!string.IsNullOrEmpty(Backend.UserNickName))
                return;

            var split = Backend.BMember.GetGuestID().Split("-");
            var nickname = split[0] + split[1] + count;
            nickname = nickname.Replace(" ", "");

            SendQueue.Enqueue(Backend.BMember.CreateNickname, nickname, (bro) =>
            {
                if (bro.IsSuccess())
                {
                    Utility.LogWithColor($"User NickName : {Backend.UserNickName}", Color.yellow);
                    callBack?.Invoke();
                }
                else
                {
                    switch (bro.GetStatusCode())
                    {
                        case "400":
                            break;
                        case "409": //닉네임 중복
                            CreateNickname(count + 1, callBack);
                            break;
                    }
                }
            });
        }

        private static bool CheckUserInBackend(string token, FederationType federationType, int maxRepeatCount = 2)
        {
            Utility.LogWithColor($"CheckUserInBackend... {maxRepeatCount}", Color.red);
            if (maxRepeatCount <= 0)
            {
                Utility.LogWithColor("CheckUserInBackend... Fails!!", Color.red);
                return false;
            }
            
            Utility.Log("CheckUserInBackend...");
            var bro = Backend.BMember.CheckUserInBackend(token, federationType);
            var getMessage = bro.GetMessage();

            if(bro.IsSuccess())
            {
                Utility.LogWithColor("CheckUserInBackend... Success!!", Color.green);
                switch (bro.GetStatusCode())
                {
                    //not enjoy user
                    case "204":
                        return false;
                    //enjoy user
                    case "200":
                        return true;
                }
            }

            if (IsCommonError(bro))
            {
                CheckUserInBackend(token, federationType, maxRepeatCount - 1);
            }

            Utility.LogWithColor("CheckUserInBackend... Fails!!", Color.red);
            return false;
        }

        private static void AuthorizeFederation(string token, FederationType federationType, int maxRepeatCount = 2, UnityAction<bool> callback = null)
        {
            Utility.LogWithColor($"AuthorizeFederation... {maxRepeatCount}", Color.red);
            if (maxRepeatCount <= 0)
            {
                Utility.LogWithColor("AuthorizeFederation... Fails!!", Color.red);
                OnError();
                callback?.Invoke(false);
                return;
            }
            
            var bro = Backend.BMember.AuthorizeFederation(token, federationType);
            if (bro.IsSuccess())
            {
                Utility.LogWithColor("AuthorizeFederation... Success!!", Color.green);
                Utility.LogWithColor(bro.GetStatusCode(), Color.green);
            }
            else
            {
                if (IsCommonError(bro))
                {
                    AuthorizeFederation(token, federationType, maxRepeatCount - 1);
                }
                else
                {
                    var getMessage = bro.GetMessage();
                    Utility.LogWithColor("AuthorizeFederation... Fails!!", Color.red);
                    Utility.LogWithColor($"{bro.GetStatusCode()}: {bro.GetMessage()}", Color.red);
                    switch (bro.GetStatusCode())
                    {
                        case "403":
                            if(getMessage.Contains("Forbidden blocked device"))
                                OnDeviceBlockError(bro.GetErrorData()["uuid"].ToString());
                            break;
                        default:
                            OnError();
                            break;
                    }
                }
            }
            callback?.Invoke(bro.IsSuccess());
        }

        private static void ChangeCustomToFederation(string token, FederationType federationType)
        {
            Backend.BMember.ChangeCustomToFederation(token, federationType, callback =>
            {
                if (callback.IsSuccess())
                {
                    Utility.LogWithColor($"Change Custom To Federation: {federationType}", Color.yellow);
                    ControllerCanvas.Get<ControllerCanvasToastMessage>()
                        .ShowTransientToastMessage(LocalizedTextType.FederationLoginSuccess);
                    onBindAuthorizedFederation?.Invoke(federationType);
                    DataController.SaveFederationLoginInfo(token, federationType);
                }
                else
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>()
                        .ShowTransientToastMessage(LocalizedTextType.Error_LoginFails);
                }

                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
            });
        }

        public static void CheckNicknameDuplication(string nickname, UnityAction<BackendReturnObject> returnCallback)
        {
            Backend.BMember.CheckNicknameDuplication(nickname, callback => { returnCallback?.Invoke(callback); });
        }

        public static void UpdateNickname(string nickname, UnityAction<BackendReturnObject> returnCallback)
        {
            Backend.BMember.UpdateNickname(nickname, (callback) => { returnCallback?.Invoke(callback); });
        }

        public static void UpdateMyRanking(string rankUuid, KeyValuePair<string, double> param, UnityAction<bool> updateCallBack = null)
        {
            Utility.LogWithColor("Data Searching", Color.cyan);
            Backend.GameData.GetMyData(DataController.UserDataTableKeyString, new Where(), callback =>
            {
                var rowInDate = string.Empty;
                if (!callback.IsSuccess())
                {
                    Utility.LogError("Data Searching - Error");
                    updateCallBack?.Invoke(false);
                    return;
                }

                Utility.LogWithColor("Data Searching - Compleate", Color.cyan);
                if (callback.FlattenRows().Count > 0)
                    rowInDate = callback.FlattenRows()[0]["inDate"].ToString();
                else
                {
                    Utility.LogWithColor("No Data!!", Color.cyan);
                    Backend.GameData.Insert(DataController.UserDataTableKeyString, insertCallback =>
                    {
                        if (!insertCallback.IsSuccess())
                        {
                            Utility.LogError($"Data Insert - Error: {insertCallback.GetStatusCode()}{insertCallback.GetMessage()}");
                            updateCallBack?.Invoke(false);
                            return;
                        }

                        Utility.LogWithColor("Data Insert - Compleate", Color.cyan);
                        rowInDate = insertCallback.GetInDate();
                    });
                }

                Utility.LogWithColor($"My inDate: {rowInDate}", Color.cyan);

                var rankParam = new Param
                {
                    { param.Key, param.Value },
                };

                Utility.LogWithColor($"Ranking Update...: {rowInDate}", Color.cyan);
                Backend.URank.User.UpdateUserScore(rankUuid, DataController.UserDataTableKeyString, rowInDate, rankParam,
                    rankCallback =>
                    {
                        if (!rankCallback.IsSuccess())
                        {
                            Utility.LogError($"Ranking Update - Error: {rankCallback.GetStatusCode()}{rankCallback.GetMessage()}");
                            updateCallBack?.Invoke(false);
                            return;
                        }

                        Utility.LogWithColor("Ranking Update - Compleate", Color.cyan);
                        updateCallBack?.Invoke(true);
                    });
            });
        }

        public static void GetRankList(string rankUuid, int limit, UnityAction<bool, JsonData> action)
        {
            SendQueue.Enqueue(Backend.URank.User.GetRankList, rankUuid, limit, callback=> {
                action?.Invoke(callback.IsSuccess(), callback.GetFlattenJSON());
            });
        }

        public static void GetMyRank(string rankUuid, UnityAction<bool, JsonData> action)
        {
            SendQueue.Enqueue(Backend.URank.User.GetMyRank, rankUuid, callback =>
            {
                action?.Invoke(callback.IsSuccess(), callback.GetFlattenJSON());
            });
        }

        public static void Logout(UnityAction<bool> callback = null)
        {
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowLoading();
            Backend.BMember.Logout(logoutCallback =>
            {
                Utility.LogWithColor("Logout...", Color.yellow);
                if (!logoutCallback.IsSuccess())
                {
                    Utility.LogError($"Logout Fails: {logoutCallback.GetStatusCode()}-{logoutCallback.GetMessage()}");
                }

                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
                callback?.Invoke(logoutCallback.IsSuccess());
            });
        }
        
        public static void WithdrawAccount(UnityAction<bool> callback = null)
        {
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowLoading();
            Backend.BMember.WithdrawAccount(withdrawCallback =>
            {
                Utility.LogWithColor("WithdrawCallback...", Color.yellow);
                if (!withdrawCallback.IsSuccess())
                {
                    Utility.LogError($"WithdrawCallback Fails: {withdrawCallback.GetStatusCode()}-{withdrawCallback.GetMessage()}");
                }

                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
                callback?.Invoke(withdrawCallback.IsSuccess());
            });
        }

        public static void VersionCheck(UnityAction<int> callback)
        {
             Backend.Utils.GetLatestVersion(bro =>
             {
                 if (!bro.IsSuccess())
                 {
                     Utility.Log("GetLatestVersion Fails...");
                     callback?.Invoke(-1);
                 }
                 else
                 {
                     var versionStr = bro.GetReturnValuetoJSON()["version"].ToString();
                     Utility.LogWithColor($"Client Version: {Application.version}", Color.green);

                     if (int.TryParse(versionStr.Replace(".", ""), out var broVersion)
                         && int.TryParse(Application.version.Replace(".", ""), out var appVersion))
                     {
                         if (broVersion <= appVersion)
                             callback?.Invoke(0);
                         else
                         {
                             var forceUpdate = int.Parse(bro.GetReturnValuetoJSON()["type"].ToString());
                             callback?.Invoke(forceUpdate);
                         }
                     }
                 }
             });
        }

        private static void OnError()
        {
            GameManager.Instance.Pause();
            Utility.LogError("OnError");
            GameManager.errorMessage = Application.systemLanguage switch
            {
                SystemLanguage.Korean => "로그인 실패",
                SystemLanguage.Japanese => "ログイン失敗",
                SystemLanguage.ChineseTraditional => "登錄失敗",
                SystemLanguage.ChineseSimplified => "登录失败",
                _ => "Login Failed"
            };
            GameManager.errorAction += Application.Quit;
            SceneManager.LoadScene("Error");
        }

        private static void OnOtherDeviceLoginDetectedError()
        {
            GameManager.Instance.Pause();
            Utility.LogError("Other Device Login");
            GameManager.errorMessage = Application.systemLanguage switch
            {
                SystemLanguage.Korean => "다른 기기에서 로그인이 감지되었습니다.",
                SystemLanguage.Japanese => "別のデバイスからログインが検出されました。",
                SystemLanguage.ChineseTraditional => "檢測到其他設備登錄。",
                SystemLanguage.ChineseSimplified => "检测到其他设备登录。",
                _ => "Login detected from another device."
            }; 
            
            GameManager.errorAction += Application.Quit;
            SceneManager.LoadScene("Error");
        }

        private static void OnMaintenanceError()
        {
            GameManager.Instance.Pause();
            Utility.LogError("On Maintenance Error");
            GameManager.errorMessage = Application.systemLanguage switch
            {
                SystemLanguage.Korean => "점검 중입니다.",
                SystemLanguage.Japanese => "メンテナンス中です。",
                SystemLanguage.ChineseTraditional => "維護中。",
                SystemLanguage.ChineseSimplified => "维护中。",
                _ => "Under Maintenance"
            }; 
            
            GameManager.errorAction += Application.Quit;
            SceneManager.LoadScene("Error");
        }

        private static void OnDeviceBlockError(string uuid)
        {
            GameManager.Instance.Pause();
            Utility.LogError("On Device Block Error");
            
            var message = Application.systemLanguage switch
            {
                SystemLanguage.Korean => "차단된 아이디 입니다.",
                SystemLanguage.Japanese => "このアカウントはブロックされています。",
                SystemLanguage.ChineseTraditional => "此帳戶已被封禁。",
                SystemLanguage.ChineseSimplified => "此账户已被封禁。",
                _ => "This account has been banned."
            };
            GameManager.errorMessage = message + $"\n\nUUID : {uuid}";
            GameManager.errorAction += Application.Quit;

            SceneManager.LoadScene("Error");
        }

        
        private static bool IsCommonError(BackendReturnObject bro)
        {
            if (!bro.IsSuccess())
            {
                var sb = new StringBuilder();
                sb.Append($"Code: {bro.GetErrorCode()}\n").Append($"Message: {bro.GetMessage()}\n");
                
                if (bro.IsClientRequestFailError())
                {
                    sb.Append($"IsClientRequestFailError");
                    return true;
                }
                if (bro.IsServerError())
                {
                    sb.Append($"IsServerError");
                    return true;
                }

                if (bro.IsMaintenanceError())
                {
                    OnMaintenanceError();
                    sb.Append($"IsMaintenanceError");
                    return true;
                }

                if (bro.IsTooManyRequestError())
                {
                    sb.Append($"IsTooManyRequestError");
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Warring_WaitForSeconds);
                    return true;
                }

                if (bro.IsBadAccessTokenError())
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowLoading();
                    var isRefreshSuccess = RefreshTheBackendToken(3);
                    if (isRefreshSuccess)
                    {
                        Utility.LogWithColor($"Refresh The Backend Token... Success!!", Color.green);
                    }
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
                }
                Utility.LogError(sb.ToString());
            }
            return false;
        }

        public static void GetPostList(UnityAction<List<UPostItem>> callback)
        {
            const PostType postType = PostType.Admin;

            Backend.UPost.GetPostList(postType, 10, bro =>
            {
                if (!bro.IsSuccess())
                {
                    IsCommonError(bro);
                    ControllerCanvas.Get<ControllerCanvasToastMessage>()
                        .ShowTransientToastMessage(LocalizedTextType.Warring_WaitForSeconds);
                    return;
                }

                var postItemList = new List<UPostItem>();
                var json = bro.GetReturnValuetoJSON()["postList"];
                for (var i = 0; i < json.Count; i++)
                {
                    var postItem = new UPostItem
                    {
                        title = (LocalizedTextType)Enum.Parse(typeof(LocalizedTextType), json[i]["title"].ToString()),
                        content = (LocalizedTextType)Enum.Parse(typeof(LocalizedTextType),
                            json[i]["content"].ToString()),
                        inDate = json[i]["inDate"].ToString(),
                        expirationDate = DateTime.Parse(json[i]["expirationDate"].ToString()),
                        sentDate = DateTime.Parse(json[i]["sentDate"].ToString()),
                    };

                    if (json[i]["items"].Count > 0)
                    {
                        postItem.goodType = (GoodType)Enum.Parse(typeof(GoodType),
                            json[i]["items"][0]["item"]["goodType"].ToString());
                        postItem.goodValue = double.Parse(json[i]["items"][0]["itemCount"].ToString());
                    }

                    postItemList.Add(postItem);
                }
                
                callback?.Invoke(postItemList);
            });
        }

        public static void ReceivePostItem(string inDate, UnityAction<KeyValuePair<GoodType, double>> callback)
        {
            Backend.UPost.ReceivePostItem(PostType.Admin, inDate, receiveBro =>
            {
                if (!receiveBro.IsSuccess())
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Warring_WaitForSeconds);
                    Utility.LogError($"{receiveBro.GetStatusCode()}: {receiveBro.GetMessage()}");
                    return;
                }

                var receivePostItemJson = receiveBro.GetReturnValuetoJSON()["postItems"];
                if (receivePostItemJson.Count > 0)
                {
                    if (receivePostItemJson[0]["item"].ContainsKey("goodType"))
                    {
                        callback?.Invoke(new KeyValuePair<GoodType, double>(
                            Enum.Parse<GoodType>(receivePostItemJson[0]["item"]["goodType"].ToString()),
                            double.Parse(receivePostItemJson[0]["itemCount"].ToString())));
                    }
                }
            });
        }

        public static void ReceivePostItemAll(UnityAction<List<KeyValuePair<GoodType, double>>> callback)
        {
            Backend.UPost.ReceivePostItemAll(PostType.Admin, receiveBro =>
            {
                var goodKeyValueList = new List<KeyValuePair<GoodType, double>>();
                if (receiveBro.IsSuccess() == false)
                {
                    Utility.LogError($"{receiveBro.GetStatusCode()}: {receiveBro.GetMessage()}");
                    callback?.Invoke(null);
                    return;
                }

                foreach (JsonData postItemJson in receiveBro.GetReturnValuetoJSON()["postItems"])
                {
                    for (var j = 0; j < postItemJson.Count; j++)
                    {
                        if (!postItemJson[j].ContainsKey("item"))
                        {
                            continue;
                        }

                        if (postItemJson[j]["item"].ContainsKey("goodType"))
                        {
                            goodKeyValueList.Add(new KeyValuePair<GoodType, double>(
                                Enum.Parse<GoodType>(postItemJson[0]["item"]["goodType"].ToString()),
                                double.Parse(postItemJson[0]["itemCount"].ToString())));
                        }
                    }
                }
                callback?.Invoke(goodKeyValueList);
            });
        }

        private static bool RefreshTheBackendToken(int maxRepeatCount)
        {
            while (true)
            {
                Utility.LogWithColor($"Refresh The Backend Token...{maxRepeatCount}", Color.red);
                
                if (maxRepeatCount <= 0) 
                    return false;

                var callback = Backend.BMember.RefreshTheBackendToken();
                
                if (callback.IsSuccess()) 
                    return true;

                if (callback.IsClientRequestFailError()
                    || callback.IsServerError())
                {
                    maxRepeatCount -= 1;
                    continue;
                }

                if (callback.IsMaintenanceError())
                {
                    OnMaintenanceError();
                    return false;
                }

                if (callback.IsTooManyRequestError())
                    return false;

                OnOtherDeviceLoginDetectedError();
                return false;
            }
        }
    }
}