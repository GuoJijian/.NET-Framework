using System;

namespace Webapi.Core.Events {
    public class UserLoggedinEvent<TUserState> : IDisposable where TUserState : class, IUser {
        public TUserState UserState { get; private set; }
        public UserLoggedinEvent(TUserState playerState) {
            this.UserState = playerState;
        }

        public void Dispose() {
            UserState = null;
        }
    }
}
