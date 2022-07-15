#if UNITY_EDITOR && !UNITY_WEBPLAYER
using NSubstitute;
#endif

namespace Zenject
{
    public static class ZenjectNSubstituteExtensions
    {
        public static ScopeConcreteIdArgConditionCopyNonLazyBinder FromSubstitute<TContract>(this FromBinderGeneric<TContract> binder)
            where TContract : class
        {
#if UNITY_EDITOR && !UNITY_WEBPLAYER
//            return binder.FromInstance(Mock.Of<TContract>());
            return binder.FromInstance(Substitute.For<TContract>());
#else
            Assert.That(false, "The use of 'ToMock' in web builds is not supported");
            return null;
#endif
        }

        public static ConditionCopyNonLazyBinder FromSubstitute<TContract>(this FactoryFromBinder<TContract> binder)
            where TContract : class
        {
//            return binder.FromInstance(Mock.Of<TContract>());
            return binder.FromInstance(Substitute.For<TContract>());
        }
    }
}
