using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace ch.kainoo.core.utilities {

    public static class DateTimeUtil
    {
        public static DateTime FromISO8601(string dateAsIsoString)
        {
            return DateTime.Parse(dateAsIsoString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        public static string ToISO8601(DateTime date)
        {
            return date.ToString("o", CultureInfo.InvariantCulture);
        }

    }

}