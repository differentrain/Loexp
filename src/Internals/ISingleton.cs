
namespace Loexp.Internals
{
    internal interface ISingleton<TSelf>
        where TSelf : ISingleton<TSelf>, new()
    {
        public static abstract TSelf Instance { get; }
    }
}
