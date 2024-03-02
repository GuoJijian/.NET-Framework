using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Webapi.Core.Utils {
    public static partial class Util {

        static DateTime unixbasetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
        public static double GetCurrentTimeByMilliseconds(this DateTime time) {
            return Math.Round((time - unixbasetime).TotalMilliseconds, 0);
        }

        public static string PADRight(this string src, int leng, char ch = ' ') {
            Func<int, byte[], char[]> func = (srccount, bytes) => {
                var asciicount = 0;
                for (int i = 0; i < bytes.Length; i += 2) {
                    if (bytes[i + 1] == 0) asciicount++;
                }

                var count = Math.Max(0, (leng - srccount * 2 + asciicount));
                var buff = new char[count];
                for (int i = 0; i < buff.Length; i++) {
                    buff[i] = ch;
                }
                return buff;
            };

            return new string(src.ToCharArray().Concat(func(src.Length, Encoding.Unicode.GetBytes(src))).ToArray());
        }

        public static bool TryConvertTo(this Type srctype, Type destinationType, object src, out object destination) {
            destination = null;
            if (!TypeDescriptor.GetConverter(srctype).CanConvertTo(destinationType))
                return false;

            if (!TypeDescriptor.GetConverter(srctype).IsValid(src))
                return false;

            return (destination = TypeDescriptor.GetConverter(srctype).ConvertTo(src, destinationType)) != null;
        }

        public static bool TryTypeConvertFrom(this Type destinationType, Type srctype, object src, out object destination) {
            destination = null;
            if (!TypeDescriptor.GetConverter(destinationType).CanConvertFrom(srctype))
                return false;

            if (!TypeDescriptor.GetConverter(destinationType).IsValid(src))
                return false;

            return (destination = TypeDescriptor.GetConverter(destinationType).ConvertFrom(src)) != null;
        }

        public static TVal GetValue<TKey, TVal>(this IDictionary<TKey, TVal> dictionary, TKey key) {
            if (dictionary.TryGetValue(key, out var val)) {
                return val;
            }
            return default(TVal);
        }


        public static object Private(this object obj, string privateField)
        {
            return obj.Private(obj.GetType(), privateField);
        }

        public static TResult Private<TResult>(this object obj, string privateField) where TResult : class
        {
            return obj.Private<TResult>(obj.GetType(), privateField);
        }

        public static object Private(this object obj, Type type, string privateField)
        {
            if (obj == null) return null;
            var field = type.GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                var fieldVal = field.GetValue(obj);
                return fieldVal;
            }
            else
            {
                var prop = type.GetProperty(privateField, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (prop != null)
                {
                    var fieldval = prop.GetValue(obj);
                    return fieldval;
                }
            }
            return null;
        }

        static TResult Private<TResult>(this object obj, Type type, string privateField) where TResult : class
        {
            var val = obj.Private(type, privateField);
            var Tval = val as TResult;
            return Tval;
        }

        public static TResult Private<TType, TResult>(this TType obj, string privateField) where TResult : class
        {
            return obj.Private<TResult>(typeof(TType), privateField);
        }

    }
}
