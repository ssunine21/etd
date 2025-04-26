using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.CloudData;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ETD.Scripts.Common
{
    public static class Utility
    {
        private static Camera Camera
        {
            get
            {
                _camera ??= Camera.main;
                return _camera;
            }
        }
        private static bool IsCamera => Camera != null;
        private static Camera _camera;
        
        public static T GetRandomEnumValue<T>(float[] probability)
        {
            var enumValues = Enum.GetValues(typeof(T));
            if (probability == null)
                return (T)enumValues.GetValue(Random.Range(0, enumValues.Length));

            var totalProbability = 0f;
            var randomValue = Random.Range(0f, 1f);
            for (var i = 0; i < enumValues.Length; ++i)
            {
                totalProbability += probability[i];
                if (randomValue <= totalProbability)
                    return (T)enumValues.GetValue(i);
            }

            return (T)enumValues.GetValue(0);
        }

        public static bool IsProbabilityTrue(float probability)
        {
            if (probability == 0) return false;
            
            var randomValue = Random.Range(0f, 1f);
            return randomValue <= probability;
        }
        
        public static void Log(string message)
        {
            LogWithColor(message, Color.white);
        }

        public static void LogError(string message)
        {
#if IS_TEST
            Debug.LogError(message);
#endif
        }

        public static void LogError(Exception e)
        {
#if IS_TEST
            Debug.LogError(e.Message + "\n" + e.StackTrace);
#endif
        }
        
        public static void LogWithColor(string message, Color color)
        {
#if IS_TEST
            var colorCode = ColorUtility.ToHtmlStringRGB(color);
            Debug.Log($"<color=#{colorCode}> {message} </color>");
#endif
        }
        
        public static Vector2 RandomPositionInView()
        {
            if (!IsCamera) return Vector2.zero;
            
            var randomX = Random.Range(0f, 1f);
            var randomY = Random.Range(0.1f, 1f);
            return Camera.ViewportToWorldPoint(new Vector3(randomX, randomY));
        }

        public static bool IsInView(Vector3 objectPosition)
        {
            if (!IsCamera) return false;
            var viewportPoint = Camera.WorldToViewportPoint(objectPosition);
            return (viewportPoint.x is >= 0 and <= 1 && viewportPoint.y is >= 0.1f and <= 1);
        }

        public static AttributeType TagTypeToAttributeType(TagType tagType)
        {
            return tagType switch
            {
                TagType.Projectile => AttributeType.Projectile,
                TagType.Expansion => AttributeType.Expansion,
                TagType.Chain => AttributeType.Chain,
                TagType.Duration => AttributeType.Duration,
                _ => AttributeType.None
            };
        }
        
        public static async UniTaskVoid ApplicationQuit(int millisecondsDelay)
        {
            await UniTask.Delay(millisecondsDelay);
            Application.Quit();
        }
        
        public static int GetSystemLanguageNumber()
        {
            var systemLanguage = Application.systemLanguage;
#if IS_TEST
            systemLanguage = GameManager.Instance.systemLanguage;
#endif
            return systemLanguage switch
            {
                SystemLanguage.Korean => 0,
                SystemLanguage.Japanese => 2,
                SystemLanguage.ChineseTraditional => 3,
                SystemLanguage.ChineseSimplified => 4,
                _ => 1
            };
        }

        public static string GetTimeStringToFromTotalSecond(TimeSpan timeSpan)
        {
            return GetTimeStringToFromTotalSecond(timeSpan.TotalSeconds);
        }

        public static string GetTimeStringToFromTotalSecond(double totalSecond)
        {
            if (totalSecond < 0) return "00:00:00";
            
            var time = TimeSpan.FromSeconds(totalSecond);
            return time.Days > 0
                ? $"{time.Days}D {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}"
                : $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        public static string GetRedColorToHex()
        {
            return $"#{195:X2}{58:X2}{59:X2}";
        }

        private static bool _autoScrolling;
        private static bool _isEnter;

        public static void ScrollKill()
        {
            if (_isEnter) _isEnter = false;
        }
        
        public static async UniTaskVoid ScrollToTarget(ScrollRect scrollRect, int sibilingIndex, float speed)
        {
            await UniTask.WaitUntil(() => !_autoScrolling);
            scrollRect.enabled = false;
            _isEnter = true;
            
            while (_isEnter)
            {
                _autoScrolling = true;
                var targetPos = 1 - sibilingIndex / (float)(scrollRect.content.childCount - 1);
                var newHorizontal = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetPos, Time.unscaledDeltaTime * speed);
                scrollRect.verticalNormalizedPosition = newHorizontal;

                if (Mathf.Abs(scrollRect.verticalNormalizedPosition - targetPos) < 0.001f) break;

                await UniTask.Yield();
            }

            _autoScrolling = false;
            _isEnter = false;
            scrollRect.enabled = true;
        }
        
        public static void NormalizeGridLayoutSize(GridLayoutGroup gridLayoutGroup, int constraintCount)
        {
            if (gridLayoutGroup.TryGetComponent<RectTransform>(out var rectTransform))
            {
                var width = rectTransform.rect.width - gridLayoutGroup.spacing.x * (constraintCount + 1);
                var cellSize = width / constraintCount;
                gridLayoutGroup.cellSize = Vector2.one * cellSize;
            }
        }
        public static void NormalizeGridLayoutXSize(GridLayoutGroup gridLayoutGroup, int constraintCount)
        {
            if (gridLayoutGroup.TryGetComponent<RectTransform>(out var rectTransform))
            {
                var width = rectTransform.rect.width - gridLayoutGroup.spacing.x * (constraintCount + 1);
                var cellSize = width / constraintCount;
                gridLayoutGroup.cellSize = new Vector2(cellSize, gridLayoutGroup.cellSize.y);
            }
        }
    }
    
    public static class ExtensionMethod
    {
        private static string[] formatArray =
        {
            "", "A", "B", "C", "D", "E", "F", "G", 
            "H", "I", "J", "K", "L", "M", "N", 
            "O", "P", "Q", "R", "S", "T", "U", 
            "V", "W", "X", "Y", "Z",
            "AA", "AB", "AC", "AD", "AE", "AF", "AG", 
            "AH", "AI", "AJ", "AK", "AL", "AM", "AN", 
            "AO", "AP", "AQ", "AR", "AS", "AT", "AU", 
            "AV", "AW", "AX", "AY", "AZ",
        };
        
        public static string ToDamage(this double value)
        {
            if (value < 0)
                return "0";

            value = Math.Round(value, 0);
            
            if (value < 1000)
                return value.ToString();

            var exponent = (int)Math.Floor(Math.Log10(value) / 3);
            
            var normalizedValue = Math.Round(value / Mathf.Pow(1000, exponent), 2);
            
            string result;
            try
            {
                result = $"{normalizedValue}{formatArray[exponent]}";
            }
            catch (Exception e)
            {
                result = "MAX";
            }
            
            return result;
        }

        public static string ToGoodString(this double value, GoodType goodType, int param0 = 0, bool isX = false)
        {
            var result = Math.Clamp(Math.Round(value, 0), 0, double.MaxValue);
            string resultText;
            switch (goodType)
            {
                case GoodType.SummonElemental when param0 > 0:
                case GoodType.SummonRune when param0 > 0:
                case GoodType.SummonElementalS or GoodType.SummonElementalSS or GoodType.SummonRuneS or GoodType.SummonRuneSS:
                {
                    var gradeType = goodType switch
                    {
                        GoodType.SummonElemental => DataController.Instance.elemental.GetBData(param0).grade,
                        GoodType.SummonRune => DataController.Instance.rune.GetBData(param0).grade,
                        GoodType.SummonElementalS => GradeType.S, GoodType.SummonElementalSS => GradeType.SS,
                        GoodType.SummonRuneS => GradeType.S, GoodType.SummonRuneSS => GradeType.SS
                    };

                    var htmlColor = ColorUtility.ToHtmlStringRGBA(ResourcesManager.GradeColor[gradeType]);
                    return $"<color=#{htmlColor}>{gradeType.ToString()}</color>";
                }
                case GoodType.Protection:
                    return $"{DataController.Instance.shop.GetProtectionDurationTimeInHour() * (param0 + 1)}H";
                default:
                {
                    if (goodType == GoodType.Dia || result < 1000)
                    {
                        resultText = param0 > 0
                            ? DataController.Instance.offlineReward.GetValueUntilTime(goodType, (float)value, 0).ToGoodString(goodType, 0)
                            : result.ToString("N0");
                    }
                    else
                    {
                        var exponent = (int)Math.Floor(Math.Log10(result) / 3);
                        var normalizedValue = result / Mathf.Pow(1000, exponent);
                        resultText = $"{normalizedValue:N0}{formatArray[exponent]}+";
                    }

                    break;
                }
            }

            resultText = (isX ? "X" : "") + resultText;
            return resultText;
        }

        public static string ToGoodString(this float value, GoodType goodType, int param0 = 0)
        {
            var result = Math.Clamp(Math.Round(value, 0), 0, double.MaxValue);
            return result.ToGoodString(goodType, param0);
        }

        public static string ToAttributeValueString(this float value, AttributeType type)
        {
            if (type is AttributeType.GameSpeed)
                value += 1;

            var attribute = CloudData.Instance.bAttributes.FirstOrDefault(x => x.type == type);
            if (attribute == null)
                return string.Empty;
            
            var expression = attribute.expression;
            return (type == AttributeType.GameSpeed ? "x" : "+") + string.Format(expression, Math.Round(value, 2)).Replace(" ", "");
        }
        
        public static void Open(this ViewCanvas viewCanvas) 
        {
            const float duration = 0.2f;
            const float slideYOffset = -100f;

            var wrapBackground = viewCanvas.WrapBackground;
            var originBackgroundColor = viewCanvas.OriginBackgroundColor;
            
            var sequence = viewCanvas.ViewAnimationType switch
            {
                ViewAnimationType.Popup => DOTween.Sequence().SetAutoKill(false)
                    .OnPlay(() =>
                    {
                        viewCanvas.OnBindOpen?.Invoke();
                        viewCanvas.WrapTr.localScale = Vector3.one * 0.85f;
                        viewCanvas.SetActive(true);
                        viewCanvas.WrapCanvasGroup.alpha = 0;
                        wrapBackground.color = new Color(originBackgroundColor.r, originBackgroundColor.g, originBackgroundColor.b, 0);
                    })
                    .Append(viewCanvas.WrapTr.DOScale(Vector3.one, duration).SetEase(Ease.OutBack))
                    .Join(wrapBackground.DOFade(0.8f, 0.4f))
                    .Join(viewCanvas.WrapCanvasGroup.DOFade(1, duration))
                    .SetUpdate(true),

                ViewAnimationType.SlideUp => DOTween.Sequence().SetAutoKill(false)
                    .OnPlay(() =>
                    {
                        viewCanvas.OnBindOpen?.Invoke();
                        viewCanvas.WrapTr.localPosition = new Vector3(0, slideYOffset, 0);
                        viewCanvas.SetActive(true);
                        viewCanvas.WrapCanvasGroup.alpha = 0;
                        wrapBackground.color = new Color(originBackgroundColor.r, originBackgroundColor.g, originBackgroundColor.b, 0);
                    })
                    .Append(viewCanvas.WrapTr.DOLocalMove(Vector3.zero, duration))
                    .Join(wrapBackground.DOFade(originBackgroundColor.a, duration))
                    .Join(viewCanvas.WrapCanvasGroup.DOFade(1, duration))
                    .SetUpdate(true),

                _ => DOTween.Sequence().SetAutoKill(false)
                    .OnPlay(() =>
                    {
                        viewCanvas.OnBindOpen?.Invoke();
                        viewCanvas.SetActive(true); 
                    })
            };
        }

        public static void Close(this ViewCanvas viewCanvas)
        {
            const float duration = 0.2f;
            const float slideYOffset = -200f;
            
            var wrapBackground = viewCanvas.WrapBackground;
            var originBackgroundColor = wrapBackground.color;
            
            _ = viewCanvas.ViewAnimationType switch
            {
                ViewAnimationType.Popup => DOTween.Sequence().SetAutoKill(false)
                    .Append(viewCanvas.WrapTr.DOScale(Vector3.one * 0.6f, duration))
                    .Join(viewCanvas.WrapCanvasGroup.DOFade(0, duration))
                    .Join(wrapBackground.DOFade(0, duration))
                    .OnComplete(() =>
                    {
                        viewCanvas.SetActive(false);
                        viewCanvas.OnBindClose?.Invoke();
                    })
                    .SetUpdate(true),

                ViewAnimationType.SlideUp => DOTween.Sequence().SetAutoKill(false)
                    .Append(viewCanvas.WrapTr.DOLocalMove(new Vector3(0, slideYOffset, 0), duration))
                    .Join(wrapBackground.DOFade(0, duration))
                    .Join(viewCanvas.WrapCanvasGroup.DOFade(0, duration))
                    .OnComplete(() =>
                    {
                        wrapBackground.color = originBackgroundColor;
                        viewCanvas.SetActive(false);
                        viewCanvas.OnBindClose?.Invoke();
                    })
                    .SetUpdate(true),

                _ => DOTween.Sequence().SetAutoKill(false)
                    .OnPlay(() => { viewCanvas.SetActive(false); })
                    .OnComplete(() => viewCanvas.OnBindClose?.Invoke()),
            };
        }

        private const string BaseUrl = "Prefabs/ViewSlots/";
        public static List<T> GetViewSlots<T>(this List<T> viewSlots, string objectName, Transform parent, int count) where T : ViewSlot<T> 
        {
            var path = $"{BaseUrl}{objectName}";
            var viewSlotsCount = viewSlots.Count;
            
            for (var i = viewSlotsCount; i < count; ++i)
            {
                var viewPref = Resources.Load(path);
                if (((GameObject)Object.Instantiate(viewPref, parent)).TryGetComponent<T>(out var newSlot))
                {
                    newSlot.SetActive(false);
                    newSlot.OnBindDisable += () => newSlot.SetActive(false);
                    
                    viewSlots.Add(newSlot);
                }
            }

            for (var i = count; i < viewSlots.Count; ++i)
            {
                viewSlots[i].SetActive(false);
            }
            
            return viewSlots.GetRange(0, count);
        }
        
        public static Queue<T> GetViewSlots<T>(this Queue<T> viewSlots, string objectName, Transform parent, int count) where T : ViewSlot<T>
        {
            var path = $"{BaseUrl}{objectName}";
            var viewSlotsCount = viewSlots.Count;
            
            for (var i = viewSlotsCount; i < count; ++i)
            {
                var viewPref = Resources.Load(path);
                if (((GameObject)Object.Instantiate(viewPref, parent)).TryGetComponent<T>(out var newSlot))
                {
                    newSlot.SetActive(false);
                    newSlot.OnBindDisable += () =>
                    {
                        newSlot.SetActive(false);
                        viewSlots.Enqueue(newSlot);
                    };
                    viewSlots.Enqueue(newSlot);
                }
            }

            return viewSlots;
        }
    }
}