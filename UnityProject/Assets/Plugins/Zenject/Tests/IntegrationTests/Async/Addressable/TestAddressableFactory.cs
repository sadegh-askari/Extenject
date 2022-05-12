using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

#if UNITASK_PLUGIN
using Cysharp.Threading.Tasks;

#else
using System.Threading.Tasks;
#endif

namespace Zenject.Tests.IntegrationTests.Async.Addressable
{
    public class TestAddressableFactory : ZenjectIntegrationTestFixture
    {
        private AssetReferenceGameObject addressablePrefabReference;

        private IEnumerator ValidateTestDependency()
        {
            yield return AddressableTestUtils.GetAddressableReference("TestAddressablePrefab", r => addressablePrefabReference = r);
        }

        [TearDown]
        public void Teardown()
        {
            addressablePrefabReference = null;
            Resources.UnloadUnusedAssets();
        }

        [UnityTest]
        public IEnumerator FactoryInstantiatePrefabForComponent()
        {
            yield return ValidateTestDependency();

            PreInstall();
            Container.BindFactory<int, AddressableTargetComponent, AddressableTargetComponent.Factory>()
                .FromComponentInAssetReference(addressablePrefabReference);
            PostInstall();

            yield return null;

            var factory = Container.Resolve<AddressableTargetComponent.Factory>();

            Assert.IsNotNull(factory);
            Assert.IsTrue(factory.IsAsync);

            const int parameterToPass = 2;
            bool success = false;
            _ = factory.CreateAsync(parameterToPass).ContinueWith(result =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(parameterToPass, result.InjectedValue);
                success = true;
            });

            for (int frameCounter = 0; frameCounter < 10000; frameCounter++)
            {
                yield return null;
                if (success)
                    Assert.Pass();
            }

            Assert.Fail();
        }

        [UnityTest]
        public IEnumerator FactoryInstantiatePrefab()
        {
            yield return ValidateTestDependency();

            PreInstall();
            Container.BindFactory<GameObject, SimpleFactory>()
                .FromAssetReferenceGameObject(addressablePrefabReference);
            PostInstall();

            yield return null;

            var factory = Container.Resolve<SimpleFactory>();

            Assert.IsNotNull(factory);
            Assert.IsTrue(factory.IsAsync);

            var task = factory.CreateAsync();
            bool success = false;
            _ = task.ContinueWith(result =>
            {
                Assert.IsNotNull(result);
                success = true;
            });

            for (int frameCounter = 0; frameCounter < 10000; frameCounter++)
            {
                yield return null;
                if (success)
                    Assert.Pass();
            }

            Assert.Fail();
        }

        [UnityTest]
        public IEnumerator FactoryInstantiatePrefabForComponent_Pooled()
        {
            yield return ValidateTestDependency();

            PreInstall();
            Container.BindFactory<int, AddressableTargetComponent, AddressableTargetComponent.Factory>()
                .FromMonoPoolableMemoryPoolAsync(x => x
                    .FromComponentInAssetReference(addressablePrefabReference)
                );
            PostInstall();

            yield return null;

            var factory = Container.Resolve<AddressableTargetComponent.Factory>();

            Assert.IsNotNull(factory);

            const int parameterToPass = 2;
            bool success = false;
            _ = factory.CreateAsync(parameterToPass).ContinueWith(result =>
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(parameterToPass, result.InjectedValue);
                success = true;
            });

            for (int frameCounter = 0; frameCounter < 10000; frameCounter++)
            {
                yield return null;
                if (success)
                    Assert.Pass();
            }

            Assert.Fail();
        }

        public class SimpleFactory : PlaceholderFactory<GameObject>
        {
        }
    }
}