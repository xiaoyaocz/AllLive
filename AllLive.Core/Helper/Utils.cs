using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AllLive.Core.Helper
{
    public static class Utils
    {
        /// <summary>
        ///时间戳(秒)
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        /// <summary>
        /// 时间戳(毫秒)
        /// </summary>
        /// <returns></returns>
        public static long GetTimestampMs()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        public static string ToMD5(string data)
        {
            MD5 md5 = MD5.Create();
            
            var hash=md5.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in hash)
            {
                stringBuilder.Append(item.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        public static string MatchText(this string input, string pattern, string _default = "0")
        {
            try
            {
                return Regex.Match(input, pattern).Groups[1].Value;
            }
            catch (Exception)
            {
                return _default;
            }

        }
        public static string MatchTextSingleline(this string input, string pattern, string _default = "0")
        {
            try
            {
                return Regex.Match(input, pattern,RegexOptions.Singleline).Groups[1].Value;
            }
            catch (Exception)
            {
                return _default;
            }

        }
        public static int ToInt32(this object input)
        {
        
            if(int.TryParse(input?.ToString()??"0", out var result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        public static long ToInt64(this object input)
        {

            if (long.TryParse(input?.ToString() ?? "0", out var result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        public static bool ToBool(this object input)
        {

            if (bool.TryParse(input?.ToString() ?? "false", out var result))
            {
                return result;
            }
            else
            {
                return false;
            }
        }

    }
}
