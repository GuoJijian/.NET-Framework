using Webapi.Core;
using Webapi.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Webapi.Framework.ComponentModel
{
    /// <summary>
    /// Startup task for the registration custom type converters
    /// </summary>
    public class TypeConverterRegistrationStartUpTask : IStartupTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Long2DateTimeTypeConverter
            TypeDescriptor.AddAttributes(typeof(long), new TypeConverterAttribute(typeof(Long2DateTimeTypeConverter)));
            TypeDescriptor.AddAttributes(typeof(DateTime), new TypeConverterAttribute(typeof(Long2DateTimeTypeConverter)));

            //lists
            TypeDescriptor.AddAttributes(typeof(List<int>), new TypeConverterAttribute(typeof(GenericListTypeConverter<int>)));
            TypeDescriptor.AddAttributes(typeof(List<decimal>), new TypeConverterAttribute(typeof(GenericListTypeConverter<decimal>)));
            TypeDescriptor.AddAttributes(typeof(List<string>), new TypeConverterAttribute(typeof(GenericListTypeConverter<string>)));

            TypeDescriptor.AddAttributes(typeof(List<long>), new TypeConverterAttribute(typeof(GenericListTypeConverter<long>)));
            TypeDescriptor.AddAttributes(typeof(List<float>), new TypeConverterAttribute(typeof(GenericListTypeConverter<float>)));
            TypeDescriptor.AddAttributes(typeof(List<double>), new TypeConverterAttribute(typeof(GenericListTypeConverter<double>)));
            TypeDescriptor.AddAttributes(typeof(List<DateTime>), new TypeConverterAttribute(typeof(GenericListTypeConverter<DateTime>)));

            //dictionaries
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, int>), new TypeConverterAttribute(typeof(GenericDictionaryTypeConverter<int, int>)));

            TypeDescriptor.AddAttributes(typeof(Dictionary<int, string>), new TypeConverterAttribute(typeof(GenericDictionaryTypeConverter<int, string>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<string, int>), new TypeConverterAttribute(typeof(GenericDictionaryTypeConverter<string, int>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<string, string>), new TypeConverterAttribute(typeof(GenericDictionaryTypeConverter<string, string>)));

            //字典嵌套
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, int>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, int>>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, long>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, long>>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, float>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, float>>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, byte>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, byte>>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, DateTime>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, DateTime>>)));
            TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, string>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, string>>)));

            //int <-> string
            //TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<int, int>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<int, int>>)));
            //TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<int, string>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<int, string>>)));
            //TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, int>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, int>>)));
            //TypeDescriptor.AddAttributes(typeof(Dictionary<int, Dictionary<string, string>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<int, Dictionary<string, string>>)));

            //string <-> int
            //TypeDescriptor.AddAttributes(typeof(Dictionary<string, Dictionary<int, int>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<string, Dictionary<int, int>>)));
            //TypeDescriptor.AddAttributes(typeof(Dictionary<string, Dictionary<string, int>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<string, Dictionary<string, int>>)));
            //TypeDescriptor.AddAttributes(typeof(Dictionary<string, Dictionary<int, string>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<string, Dictionary<int, string>>)));
            //TypeDescriptor.AddAttributes(typeof(Dictionary<string, Dictionary<string, string>>), new TypeConverterAttribute(typeof(GenericDoubleDictionaryTypeConverter<string, Dictionary<string, string>>)));



        }

        /// <summary>
        /// Gets order of this startup task implementation
        /// </summary>
        public int Order => 1;
    }
}
