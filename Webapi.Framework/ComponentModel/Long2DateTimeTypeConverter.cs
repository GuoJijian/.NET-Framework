using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Webapi.Framework.ComponentModel {
    public class Long2DateTimeTypeConverter : TypeConverter {

        static DateTime unixbasedate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();

        /// <summary>
        /// Gets a value indicating whether this converter can        
        /// convert an object in the given source type to the native type of the converter
        /// using the context.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="sourceType">Source type</param>
        /// <returns>Result</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(long) || sourceType == typeof(DateTime))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the converter's native type.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="culture">Culture</param>
        /// <param name="value">Value</param>
        /// <returns>Result</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is long input) {
                try {
                    return unixbasedate.AddMilliseconds(input);
                }
                catch { }
            } else if (value is DateTime datatime) {
                try {
                    return (long)datatime.Subtract(unixbasedate).TotalMilliseconds;
                }
                catch { }
            }
            return base.ConvertFrom(context, culture, value);
        }


        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(DateTime) || destinationType == typeof(long))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Converts the given value object to the specified destination type using the specified context and arguments
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="culture">Culture</param>
        /// <param name="value">Value</param>
        /// <param name="destinationType">Destination type</param>
        /// <returns>Result</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(long) && value != null && value is DateTime datatime) {
                try {
                    return (long)datatime.Subtract(unixbasedate).TotalMilliseconds;
                }
                catch { }
            } else if (value is long input) {
                try {
                    return unixbasedate.AddMilliseconds(input);
                }
                catch { }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
