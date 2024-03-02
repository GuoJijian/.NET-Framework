using System;

namespace Webapi.Core.Events {

    public class UserLoggedOutEvent<TUserState> : IDisposable where TUserState : class, IUser {
        public TUserState UserState { get; private set; }
        public UserLoggedOutEvent(TUserState playerState) {
            this.UserState = playerState;
        }

        public void Dispose() {
            UserState = null;
        }
    }
}
