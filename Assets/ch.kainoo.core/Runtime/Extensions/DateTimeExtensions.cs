using ch.kainoo.core.utilities;
using System;

namespace ch.kainoo.core {

    public static class DateTimeExtensions
    {
        public static string ToISO8601(this DateTime dateTime)
        {
            return DateTimeUtil.ToISO8601(dateTime);
        }
    }

}