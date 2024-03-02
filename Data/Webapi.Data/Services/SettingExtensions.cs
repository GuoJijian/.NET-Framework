using Webapi.Core;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Webapi.Data.Services
{

    /// <summary>
    /// Setting extensions
    /// </summary>
    public static class SettingExtensions {
        /// <summary>
        /// Get setting key (stored into database)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <returns>Key</returns>
        public static string GetSettingKey<T, TPropType>(this T entity,
            Expression<Func<T, TPropType>> keySelector)
            where T : ISettings {
            var member = keySelector.Body as MemberExpression;
            if (member == null) {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    keySelector));
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null) {
                throw new ArgumentException(string.Format(
                       "Expression '{0}' refers to a field, not a property.",
                       keySelector));
            }

            var key = typeof(T).Name + "." + propInfo.Name;
            return key;
        }

        public static string GetPropertyAsKey(this PropertyInfo propInfo) {
            var key = propInfo.DeclaringType.Name + "." + propInfo.Name;
            return key;
        }
    }
}

