using System;
using System.Collections.Generic;
using BackEnd;
using ETD.Scripts.Common;

namespace ETD.Scripts.UI.Common
{
    public class UPostItem
    {
        public PostType postType;
        public LocalizedTextType title;
        public LocalizedTextType content;
        public DateTime expirationDate;
        public DateTime sentDate;
        public GoodType goodType;
        public string inDate;
        public double goodValue;

        public int GetRemainingDay()
        {
            var remainingDay =
                new TimeSpan(expirationDate.Ticks - sentDate.Ticks);
            
            return remainingDay.Days;
        }
    }
}