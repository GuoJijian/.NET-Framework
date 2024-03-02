using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Webapi.Core.ComponentModel {
    /// <summary>
    /// Generic Dictionary type converted
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    public class ClassTypeConverter<T> : TypeConverter {

        Type type;

        /// <summary>
        /// Ctor
        /// </summary>
        public ClassTypeConverter() {
            type = typeof(T);
        }

        /// <summary>
        /// Gets a value indicating whether this converter can        
        /// convert an object in the given source type to the native type of the converter
        /// using the context.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="sourceType">Source type</param>
        /// <returns>Result</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string))
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
            if (value is string) {
                var input = (string)value;
                try {
                    return Deserialize(input);
                }
                catch { }
            }
            return base.ConvertFrom(context, culture, value);
        }

        T Deserialize(string input) {
            var dict = input.Split(':').Select(p => p.Split(',')).ToDictionary(p => p[0], p => p[1]);
            T instance = Activator.CreateInstance<T>();
            foreach (var prop in type.GetProperties()) {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (!dict.TryGetValue(prop.Name, out string valuestr))
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(valuestr))
                    continue;

                var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(valuestr);

                //set property
                prop.SetValue(instance, value, null);
            }
            return instance;
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
            if (destinationType == typeof(string) && value != null) {
                try {
                    return string.Join(":", Serialize(value));
                }
                catch { }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        IEnumerable<string> Serialize(object instance) {
            ICollection<string> result = new Collection<string>();
            foreach (var prop in type.GetProperties()) {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                var key = prop.Name;
                var str = string.Empty;
                //Duck typing is not supported in C#. That's why we're using dynamic type
                dynamic value = prop.GetValue(instance, null);
                if (value != null) {
                    str = TypeDescriptor.GetConverter(prop.PropertyType).ConvertToInvariantString(value);
                }
                result.Add($"{key}, {str}");
            }
            return result;
        }
    }
}
