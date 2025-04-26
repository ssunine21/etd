using System;
using System.Globalization;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.Common
{
    public static class ServerTime
    {
        public static bool IsInit { get; private set; }
        public static TimeSpan RemainingTimeUntilNextDay => _nextDayDate - _parsedDate;
        public static TimeSpan RemainingTimeUntilNextWeek => _nextWeekDate - _parsedDate;
        public static DateTime Date => _parsedDate;
        public static UnityAction onBindNextDay;
        public static UnityAction onBindNextWeek;
        
        private static DateTime _parsedDate;
        private static DateTime _nextDayDate;
        private static DateTime _nextWeekDate;

        private const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private static readonly string[] Formats = {"yyyy-MM-ddTHH:mm:ss", "yyyy/MM/dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "d/M/yyyy HH:mm:ss", "yyyy-MM-dd tt hh:mm:ss", "d/M/yyyy H:mm:ss"};

        public static void Init()
        {
            AsyncServerTime(() =>
            {
                _nextDayDate = IsoStringToDateTime(DataController.Instance.setting.nextDayToString);
                _nextWeekDate = IsoStringToDateTime(DataController.Instance.setting.nextWeekToString);
                UpdateTime().Forget();
                IsInit = true;
            });
        }

        public static int GetUtcOffset()
        {
            var localZone = TimeZoneInfo.Local;
            var offset = localZone.BaseUtcOffset;

            return offset.Hours;
        }

        public static string DateTimeToIsoString(DateTime dateTime)
        {
            return dateTime.ToString(TimeFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime IsoStringToDateTime(string isoString, int actionCount = 1)
        {
            try
            {
                if (!DateTime.TryParseExact(isoString, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    if (!DateTime.TryParseExact(isoString, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                        if (!DateTime.TryParse(isoString, out result)) return new DateTime();

                    if (actionCount <= 0)
                        return new DateTime();

                    IsoStringToDateTime(result.ToUniversalTime().ToString(TimeFormat, CultureInfo.InvariantCulture), --actionCount);
                }

                return result.ToUniversalTime();
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
                return new DateTime();
            }
        }

        private static void SetNextDay()
        {
            var tempDate = _parsedDate.AddDays(1);
            _nextDayDate = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day);
            DataController.Instance.setting.nextDayToString = DateTimeToIsoString(_nextDayDate);

            onBindNextDay?.Invoke();
            DataController.Instance.SaveBackendData();
        }

        private static void SetNextWeek()
        {
            var currentDay = _parsedDate.DayOfWeek;
            var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)currentDay + 7) % 7;
            if (daysUntilNextMonday == 0) daysUntilNextMonday = 7;

            var tempDate = _parsedDate.AddDays(daysUntilNextMonday);
            _nextWeekDate = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day);
            DataController.Instance.setting.nextWeekToString = DateTimeToIsoString(_nextWeekDate);
            
            onBindNextWeek?.Invoke();
            DataController.Instance.SaveBackendData();
        }

        private static void AsyncServerTime(UnityAction callback = null)
        {
            var servertime = Backend.Utils.GetServerTime();
            if (servertime.IsSuccess())
            {
                var time = servertime.GetReturnValuetoJSON()["utcTime"].ToString();
                _parsedDate = IsoStringToDateTime(time);
                callback?.Invoke();
            }
            else
            {
                _parsedDate = IsoStringToDateTime(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                callback?.Invoke();
            }
        }

        private static async UniTaskVoid UpdateTime()
        {
            var totalSeconds = 0;
            while (true)
            {
                if (totalSeconds > 600)
                {
                    totalSeconds = 0;
                    AsyncServerTime();
                }
                
                await UniTask.Delay(1000, true);
                _parsedDate = _parsedDate.AddSeconds(1);
                
                if(_parsedDate.Ticks > _nextDayDate.Ticks)
                    SetNextDay();
                
                if(_parsedDate.Ticks > _nextWeekDate.Ticks)
                    SetNextWeek();
                
                totalSeconds += 1;
            }
        }

        public static bool IsRemainingTimeUntilDisable(string dateTimeToString)
        {
            return IsRemainingTimeUntilDisable(IsoStringToDateTime(dateTimeToString));
        }

        public static bool IsRemainingTimeUntilDisable(DateTime disableTime)
        {
            return disableTime.Ticks - Date.Ticks > 0;
        }
            
        public static TimeSpan RemainingTimeToTimeSpan(DateTime toTime)
        {
            var timeSpan = (toTime <= _parsedDate) ? TimeSpan.Zero : toTime - _parsedDate;
            return timeSpan;
        }
        
        public static TimeSpan RemainingTimeToTimeSpan(string toTime)
        {
            return RemainingTimeToTimeSpan(IsoStringToDateTime(toTime));
        }

        public static TimeSpan UntilTimeToServerTime(DateTime fromTime)
        {
            var timeSpan = (fromTime > _parsedDate) ? TimeSpan.Zero : _parsedDate - fromTime;
            return timeSpan;
        }
        public static TimeSpan UntilTimeToServerTime(string toTime)
        {
            return UntilTimeToServerTime(IsoStringToDateTime(toTime));
        }

    }
}