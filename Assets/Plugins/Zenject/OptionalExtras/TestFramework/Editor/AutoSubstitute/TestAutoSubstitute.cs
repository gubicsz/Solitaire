using NUnit.Framework;
using Assert=ModestTree.Assert;

namespace Zenject.Tests.AutoSubstitute
{
    [TestFixture]
    public class TestAutoSubstitute
    {
        DiContainer _container;

        [SetUp]
        public void Setup()
        {
            _container = new DiContainer();
        }

        [Test]
        public void Test1()
        {
            _container.Bind<IFoo>().FromSubstitute();

            _container.Bind<Bar>().AsSingle();

            _container.Resolve<Bar>().Run();
        }

        [Test]
        public void TestFactories()
        {
            _container.BindFactory<IFoo, FooFactory>().FromSubstitute();

            _container.Bind<Bar2>().AsSingle();

            _container.Resolve<Bar2>().Run();
        }

        public class Bar
        {
            readonly IFoo _foo;

            public Bar(IFoo foo)
            {
                _foo = foo;
            }

            public void Run()
            {
                _foo.DoSomething();

                var result = _foo.GetTest();

                Assert.IsEmpty(result);
            }
        }

        public class Bar2
        {
            readonly FooFactory _fooFactory;

            public Bar2(FooFactory fooFactory)
            {
                _fooFactory = fooFactory;
            }

            public void Run()
            {
                var foo = _fooFactory.Create();

                foo.DoSomething();

                var result = foo.GetTest();

                Assert.IsEmpty(result);
            }
        }

        public class FooFactory : PlaceholderFactory<IFoo>
        {
        }

        public interface IFoo
        {
            string GetTest();
            void DoSomething();
        }
    }
}



