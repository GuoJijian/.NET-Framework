using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Webapi.Core.Utils {
    public static partial class Util {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
            if (enumerable != null) {
                foreach (var item in enumerable) {
                    action(item);
                }
            }
            return enumerable;
        }
        public static T[] ForEach<T>(this T[] enumerable, Action<T> action) {
            if (enumerable != null) {
                foreach (var item in enumerable) {
                    action(item);
                }
            }
            return enumerable;
        }
        public static IList<T> ForEach<T>(this IList<T> enumerable, Action<T> action) {
            if (enumerable != null) {
                foreach (var item in enumerable) {
                    action(item);
                }
            }
            return enumerable;
        }
        public static ICollection<T> ForEach<T>(this ICollection<T> enumerable, Action<T> action) {
            if (enumerable != null) {
                foreach (var item in enumerable) {
                    action(item);
                }
            }
            return enumerable;
        }
        public static Collection<T> ForEach<T>(this Collection<T> enumerable, Action<T> action) {
            if (enumerable != null) {
                foreach (var item in enumerable) {
                    action(item);
                }
            }
            return enumerable;
        }
        public static List<T> ForEach<T>(this List<T> enumerable, Action<T> action) {
            if (enumerable != null) {
                foreach (var item in enumerable) {
                    action(item);
                }
            }
            return enumerable;
        }
    }
}
