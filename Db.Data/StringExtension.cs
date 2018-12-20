using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Db.Data
{
    /// <summary>
    /// 字符串扩展类
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 指示指定的字符串是 null 还是 System.String.Empty 字符串。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns>如果 value 参数为 null 或空字符串 ("")，则为 true；否则为 false。</returns>
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 指示指定的字符串是 null、空还是仅由空白字符组成。
        /// </summary>
        /// <param name="value">要测试的字符串。</param>
        /// <returns> 如果 value 参数为 null 或 System.String.Empty，或者如果 value 仅由空白字符组成，则为 true。</returns>
        public static bool IsEmptySpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 去末尾的0
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>value</returns>
        private static string TrimClearZeor(string value)
        {
            return value.IndexOf('.') > -1 ? value.TrimEnd('0').TrimEnd('.') : value;
        }

        /// <summary>
        /// 去末尾的0
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>value</returns>
        public static string TrimClearZero(this double value)
        {
            return TrimClearZeor(value.ToString());
        }

        /// <summary>
        /// 去末尾的0
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>value</returns>
        public static string TrimClearZero(this float value)
        {
            return TrimClearZeor(value.ToString());
        }

        /// <summary>
        /// 去末尾的0
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>value</returns>
        public static string TrimClearZero(this decimal value)
        {
            return TrimClearZeor(value.ToString());
        }

        /// <summary>
        /// 手机号码隐藏中间几位
        /// </summary>
        /// <param name="mobile">手机号码</param>
        /// <returns>结果</returns>
        public static string ToMobile(this string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return mobile;
            }

            if (mobile.Length != 11)
            {
                return mobile;
            }

            return Regex.Replace(mobile, @"(\d{3})\d{6}(\d{2})", "$1******$2");
        }

        /// <summary>
        /// 字符串转UniCode
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>UniCode字符串</returns>
        public static string StringToUniCode(this string value)
        {
            var charbuffers = value.ToCharArray();
            byte[] buffer;
            var sb = new StringBuilder();
            for (var i = 0; i < charbuffers.Length; i++)
            {
                buffer = Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(string.Format(@"\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// UniCode转字符串
        /// </summary>
        /// <param name="value">UniCode字符串</param>
        /// <returns>字符串</returns>
        public static string UnicodeToString(this string value)
        {
            var dst = string.Empty;
            var src = value;
            var len = value.Length / 6;
            for (var i = 0; i <= len - 1; i++)
            {
                var str = "";
                str = src.Substring(0, 6).Substring(2);
                src = src.Substring(6);
                var bytes = new byte[2];
                bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }

            return dst;
        }

        /// <summary>
        /// 字符串格式化填充
        /// </summary>
        /// <param name="format">格式化定义</param>
        /// <param name="args">参数集合</param>
        /// <returns>格式化填充后的字符串</returns>
        public static string Format(this string format, params object[] args)
        {
            if (format == null || args == null)
            {
                throw new ArgumentNullException(format == null ? "format" : "args");
            }

            var capacity = format.Length + args.Where(a => a != null).Select(p => p.ToString()).Sum(p => p.Length);
            var sb = new StringBuilder(capacity);
            sb.AppendFormat(format, args);
            return sb.ToString();
        }

        /// <summary>
        /// 字符串集合格式化填充
        /// </summary>
        /// <param name="formats">格式化定义集合</param>
        /// <param name="args">参数集合</param>
        /// <returns>格式化填充后的字符串</returns>
        public static string Format(this IEnumerable<string> formats, params object[] args)
        {
            if (formats == null || args == null)
            {
                throw new ArgumentNullException(formats == null ? "formats" : "args");
            }

            var count = formats.Count();
            var capacity = count + formats.Where(f => !string.IsNullOrEmpty(f)).Sum(f => f.Length) +
                           args.Where(a => a != null).Select(p => p.ToString()).Sum(p => p.Length) * count;
            var sb = new StringBuilder(capacity);
            foreach (var f in formats)
            {
                if (!string.IsNullOrEmpty(f))
                {
                    sb.AppendFormat(f, args);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 随机字符串
        /// </summary>
        /// <param name="count">count</param>
        /// <returns>随机字符串</returns>
        public static string RandomString(int count)
        {
            var checkCode = string.Empty; //存放随机码的字符串   
            var random = new Random();
            for (var i = 0; i < count; i++) //产生4位校验码   
            {
                var number = random.Next();
                number = number % 36;
                if (number < 10)
                {
                    number += 48; //数字0-9编码在48-57   
                }
                else
                {
                    number += 55; //字母A-Z编码在65-90   
                }

                checkCode += ((char)number).ToString();
            }

            return checkCode;
        }

        #region 拆分字符串

        /// <summary>
        /// 根据字符串拆分字符串
        /// </summary>
        /// <param name="source">要拆分的字符串</param>
        /// <param name="separator">拆分符</param>
        /// <returns>数组</returns>
        public static string[] Split(this string source, string separator)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (separator == null)
            {
                throw new ArgumentNullException("separator");
            }

            var strtmp = new string[1];
            // ReSharper disable once StringIndexOfIsCultureSpecific.2
            var index = source.IndexOf(separator, 0);
            if (index < 0)
            {
                strtmp[0] = source;
                return strtmp;
            }

            strtmp[0] = source.Substring(0, index);
            return Split(source.Substring(index + separator.Length), separator, strtmp);
        }

        /// <summary>
        /// 采用递归将字符串分割成数组
        /// </summary>
        /// <param name="source">要拆分的字符串</param>
        /// <param name="separator">拆分符</param>
        /// <param name="attachArray">attachArray</param>
        /// <returns>string[]</returns>
        private static string[] Split(string source, string separator, string[] attachArray)
        {
            // while循环的方式
            while (true)
            {
                var strtmp = new string[attachArray.Length + 1];
                attachArray.CopyTo(strtmp, 0);

                // ReSharper disable once StringIndexOfIsCultureSpecific.2
                var index = source.IndexOf(separator, 0);
                if (index < 0)
                {
                    strtmp[attachArray.Length] = source;
                    return strtmp;
                }

                strtmp[attachArray.Length] = source.Substring(0, index);
                source = source.Substring(index + separator.Length);
                attachArray = strtmp;
            }

            // 递归的方式
            /*
            string[] strtmp = new string[attachArray.Length + 1];
            attachArray.CopyTo(strtmp, 0);

            // ReSharper disable once StringIndexOfIsCultureSpecific.2
            int index = source.IndexOf(separator, 0);
            if (index < 0)
            {
                strtmp[attachArray.Length] = source;
                return strtmp;
            }
            else
            {
                strtmp[attachArray.Length] = source.Substring(0, index);
                return Split(source.Substring(index + separator.Length), separator, strtmp);
            }*/
        }

        #endregion
    }
}
