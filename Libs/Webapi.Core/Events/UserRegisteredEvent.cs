using Webapi.Core.Domain;
using System;

namespace Webapi.Core.Events {
    public class UserRegisteredEvent<TUser> : IDisposable where TUser : User {
        public TUser User { get; private set; }
        public UserRegisteredEvent(TUser player) {
            this.User = player;
        }

        public void Dispose() {
            User = null;
        }
    }
}
