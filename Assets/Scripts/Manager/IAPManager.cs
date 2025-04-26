using System;
using BackEnd;
using ETD.Scripts.Common;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using ProductType = UnityEngine.Purchasing.ProductType;

namespace ETD.Scripts.Manager
{
    public class IAPManager : IStoreListener
    {
        public bool IsInitialized { get; private set; }
        
        private IStoreController _controller;
        private IExtensionProvider _extensions;

        private UnityAction<bool> purchasedCallback;

        public void Init()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            
            foreach (var iapType in Enum.GetValues(typeof(IAPType)))
            {
                if((IAPType)iapType == IAPType.net_themessage_etc_removeadspackage) continue;
                
                var iapTypeToString = ((IAPType)iapType).ToString();
                builder.AddProduct(iapTypeToString, ProductType.Consumable, new IDs
                {
                    {iapTypeToString, GooglePlay.Name},
                    {iapTypeToString, AppleAppStore.Name}
                });
            }
            
            UnityPurchasing.Initialize(this, builder);
        }
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Utility.LogError("OnInitializeFailed");
            Utility.LogError(error.ToString());
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Utility.LogError("OnInitializeFailed");
            Utility.LogError(message);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var receiptJson = purchaseEvent.purchasedProduct.receipt;
            var iapPrice = purchaseEvent.purchasedProduct.metadata.localizedPrice;
            var iapCurrency = purchaseEvent.purchasedProduct.metadata.isoCurrencyCode;
            
#if UNITY_ANDROID
            var validation =
                Backend.Receipt.IsValidateGooglePurchase(receiptJson, "", iapPrice, iapCurrency);
#elif UNITY_IOS
            var validation =
                Backend.Receipt.IsValidateApplePurchase(receiptJson, "", iapPrice, iapCurrency);
#endif
            
            if (validation.IsSuccess())
            {
                Utility.LogWithColor($"Purchase Success: {purchaseEvent.purchasedProduct.definition.id}", Color.green);
                purchasedCallback?.Invoke(true);
            }
            else
            {
                Utility.LogWithColor($"Backend Receipt Validation Fails: {purchaseEvent.purchasedProduct.definition.id}\n" +
                                     $"status: {validation.GetStatusCode()}\n" +
                                     $"error: {validation.GetErrorCode()}", Color.green);
                purchasedCallback?.Invoke(false);
            }
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Utility.LogWithColor($"Purchase Fails : {failureReason} / Product Id : {product.definition.id}", Color.green);
            purchasedCallback?.Invoke(false);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            
            Utility.Log("IAP Init Success");

            IsInitialized = true;
        }
        
        public void Purchase(string productId, UnityAction<bool> callback)
        {
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowLoading();
            purchasedCallback = callback;
            purchasedCallback += (success) =>
            {
                if(success)
                {
                    DataController.Instance.shop.SetIsFirstPurchased(true);
                }
                
                ControllerCanvas.Get<ControllerCanvasToastMessage>().CloseLoading();
            };
            
#if UNITY_EDITOR
            purchasedCallback?.Invoke(true);
#else
            
            var product = _controller.products.WithID(productId);
            if (product is { availableToPurchase: true })
            {
                _controller.InitiatePurchase(product);
            }
            else
            {
                Utility.LogWithColor($"product available is false : {productId}", Color.green);
                purchasedCallback?.Invoke(false);
            }
#endif
        }

        public string GetPrice(string productId)
        {
            var product = _controller.products.WithID(productId);

            if (product is { availableToPurchase: true })
            {
                // Get the localized price
                var localizedPrice = product.metadata.localizedPriceString;
                //var localizedPriceSymbol = product.metadata.isoCurrencyCode;

                return $"{localizedPrice}";
            }

            return string.Empty;
        }

        public string GetMultiplePriceString(string productId, int multiplePrice)
        {
            var product = _controller.products.WithID(productId);
            if (product is { availableToPurchase: true })
            {
                // 기존 가격 가져오기
                var originalPrice = product.metadata.localizedPrice;
                var triplePrice = originalPrice * multiplePrice;

                // 기존 통화 기호를 포함한 가격 문자열을 조작
                var currencySymbol = product.metadata.localizedPriceString.Remove(1);
                return currencySymbol + triplePrice.ToString("0.##");
            }

            return string.Empty;
        }

        private static IAPManager _instance;
        public static IAPManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new IAPManager();
                return _instance;
            }
        }
    }
}