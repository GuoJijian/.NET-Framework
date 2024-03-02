using Webapi.Core.Configuration;
using System;
using System.Linq.Expressions;

namespace Webapi.Core.Domain {

    /// <summary>
    /// Represents a setting
    /// </summary>
    public partial class Setting : BaseEntity, ISettingEntity {
        /// <summary>
        /// Ctor
        /// </summary>
        public Setting() { }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="appId">App identifier</param>
        public Setting(uint id, string name, string value) {
            this.Id = id;
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }


        public int ModuleId { get; set; }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns>Result</returns>
        public override string ToString() {
            return Name;
        }
    }
}