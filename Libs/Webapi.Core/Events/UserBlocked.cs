using Webapi.Core;
using Webapi.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Webapi.Core.Events {
    /// <summary>
    /// A container for passing entities that have been deleted. This is not used for entities that are deleted logicaly via a bit column.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class UserBlocked<TUser> : IDisposable where TUser : User {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="user">Entity</param>
        public UserBlocked(TUser user) {
            this.User = user;
        }

        /// <summary>
        /// User
        /// </summary>
        public TUser User { get; private set; }

        public void Dispose() {
            User = null;
        }
    }
}
