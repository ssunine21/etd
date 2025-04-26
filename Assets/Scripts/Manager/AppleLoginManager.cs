using System.Text;
using System.Threading;
using AppleAuth;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using ETD.Scripts.Common;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.Manager
{
    public class AppleLoginManager : Singleton<AppleLoginManager>
    {
        private AppleAuthManager _appleAuthManager;

        private void Start()
        {
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }

        private void Update()
        {
            _appleAuthManager?.Update();
        }

        public delegate void AppleLoginCallback(bool isSuccess, string message, string token);
        public void SigninWithApple(AppleLoginCallback appleLoginCallback)
        {
            var loginArgs = new AppleAuthLoginArgs(AppleAuth.Enums.LoginOptions.IncludeEmail | AppleAuth.Enums.LoginOptions.IncludeFullName);
            _appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    if (credential is IAppleIDCredential appleIdCredential)
                    {
                        //var userId = appleIdCredential.User;
                        //var email = appleIdCredential.Email;
                        //var fullName = appleIdCredential.FullName;
                        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                        //var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);

                        appleLoginCallback(true, appleIdCredential.ToString(), identityToken);
                    }
                },
                error =>
                {
                    Utility.LogError($"Apple Login Error: {error}");
                    appleLoginCallback(false, $"Apple Login Error: {error}", string.Empty);
                });
        }

        public override void Init(CancellationTokenSource cts)
        {
            
        }
    }
}
