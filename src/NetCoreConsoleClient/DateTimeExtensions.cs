using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreConsoleClient
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimeSeconds(this DateTime d)
        {
            var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)epoch.TotalSeconds;
        }

        public static long ToUnixTimeMilliseconds(this DateTime d)
        {
            var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)epoch.TotalMilliseconds;
        }

        /// <summary>
        /// Convert unixTimeStamp(milliseconds to date time)
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long unixTimeStamp)
        {
            System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp);
            return dateTime;
        }

        /// <summary>
        /// Convert unixTimeStamp(milliseconds to date time)
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long? unixTimeStamp)
        {
            System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp.Value);
            return dateTime;
        }
    }
}
