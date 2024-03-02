namespace Webapi.Core.Infrastructure
{
    public class Singleton<T> 
    {

        /// <summary>
        /// The singleton instance for the specified type T. Only one instance (at the time) of this object for each type of T.
        /// </summary>
        public static T Instance { get; set; }
    }
}
