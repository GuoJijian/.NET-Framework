using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webapi.Data.Mapping
{
    public static class PropertyBuilderExtensions
    {
        /// <summary>
        /// 配置为小型文本，最大长度为64的Unicode字符
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <returns></returns>
        public static PropertyBuilder<string> IsSmallString(this PropertyBuilder<string> propertyBuilder)
        {
            return propertyBuilder.HasMaxLength(64).IsUnicode();
        }

        /// <summary>
        /// 配置为中型文本，最大长度为512的Unicode字符
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <returns></returns>
        public static PropertyBuilder<string> IsMediumString(this PropertyBuilder<string> propertyBuilder)
        {
            return propertyBuilder.HasMaxLength(512).IsUnicode();
        }

        /// <summary>
        /// 配置为大型文本，最大长度为2048的Unicode字符
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <returns></returns>
        public static PropertyBuilder<string> IsLargeString(this PropertyBuilder<string> propertyBuilder)
        {
            return propertyBuilder.HasMaxLength(2048).IsUnicode();
        }

    }
}
