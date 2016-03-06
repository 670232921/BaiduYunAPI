using System;
using System.Collections.Generic;
using System.Text;

namespace BaiduApi
{
	public class Utility
	{
		public static long GetCurrentTimeStamp()
		{
			DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
			return (long)(DateTime.Now - dt).TotalSeconds;
		}

		public static long GetTimeStamp(DateTime dt)
		{
			DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
			return (long)(dt - dtStart).TotalSeconds;
		}

		public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime); return dtStart.Add(toNow);
        }
	}
}
