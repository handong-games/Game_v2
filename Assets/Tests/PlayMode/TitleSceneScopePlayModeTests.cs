using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public sealed class TitleSceneScopePlayModeTests
    {
        private const string SceneName = "TitleScene";
        private const int MaxFrames = 240;

        [UnityTest]
        public IEnumerator TitleScene_BuildsConnectedScopeWithoutDuplicateControllers()
        {
            VerifyDependencyManagerDoesNotOwnTitleSceneScopedTypes();
            VerifySceneManagerExIsRemoved();
            VerifyPureCSharpTitleSceneIsRemoved();

            AsyncOperation load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, $"{SceneName} could not be loaded.");

            while (!load.isDone)
                yield return null;

            object rootScope = null;
            object rootContainer = null;
            object titleScope = null;
            object titleContainer = null;

            for (int i = 0; i < MaxFrames; i++)
            {
                rootScope = GetStaticProperty(RequiredType("GameBootstrap"), "RootLifetimeScope");
                rootContainer = rootScope != null ? GetInstanceProperty(rootScope, "Container") : null;
                titleScope = FindSingleObject(RequiredType("Game.Core.Composition.TitleSceneScope"));
                titleContainer = titleScope != null ? GetInstanceProperty(titleScope, "Container") : null;

                if (rootContainer != null && titleContainer != null)
                    break;

                yield return null;
            }

            Assert.IsNotNull(rootScope, "RootLifetimeScope was not created.");
            Assert.IsNotNull(rootContainer, "RootLifetimeScope container was not built.");
            Assert.IsNotNull(titleScope, "TitleSceneScope was not created.");
            Assert.IsNotNull(titleContainer, "TitleSceneScope container was not built.");

            Type sceneLoaderInterfaceType = RequiredType("Game.Core.Ports.ISceneLoader");
            Type unitySceneLoaderType = RequiredType("Game.Core.Adapters.UnitySceneLoader");
            Type adventureStartStateType = RequiredType("Domains.Adventure.AdventureStartState");
            Type titleControllerType = RequiredType("Views.TitleView.TitleViewController");
            Type characterControllerType = RequiredType("Domains.CharacterSelect.CharacterSelectController");
            Type viewFlowType = RequiredType("Domains.Scene.Title.TitleSceneViewFlow");

            object sceneLoader = Resolve(rootContainer, sceneLoaderInterfaceType);
            object adventureStartState = Resolve(rootContainer, adventureStartStateType);
            object titleControllerA = Resolve(titleContainer, titleControllerType);
            object titleControllerB = Resolve(titleContainer, titleControllerType);
            object characterControllerA = Resolve(titleContainer, characterControllerType);
            object characterControllerB = Resolve(titleContainer, characterControllerType);
            MethodInfo sceneLoadMethod = sceneLoaderInterfaceType.GetMethod("Load");

            Assert.AreEqual(unitySceneLoaderType, sceneLoader.GetType(), "Root ISceneLoader must resolve to UnitySceneLoader.");
            Assert.IsNotNull(sceneLoadMethod, "ISceneLoader.Load method was not found.");
            Assert.IsFalse(sceneLoadMethod.IsGenericMethod, "ISceneLoader must use GameSceneId loading, not generic BaseScene loading.");
            Assert.IsNotNull(adventureStartState, "AdventureStartState could not be resolved from RootLifetimeScope.");
            Assert.IsNotNull(Resolve(titleContainer, viewFlowType), "TitleSceneViewFlow could not be resolved.");
            Assert.AreSame(titleControllerA, titleControllerB, "TitleViewController is not scoped as a single instance.");
            Assert.AreSame(characterControllerA, characterControllerB, "CharacterSelectController is not scoped as a single instance.");
        }

        private static void VerifyDependencyManagerDoesNotOwnTitleSceneScopedTypes()
        {
            Type registryType = RequiredType("Game.Core.Managers.Dependency.Generated.DependencyRegistry");
            FieldInfo allField = registryType.GetField("All", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(allField, "DependencyRegistry.All field was not found.");

            Type titleControllerType = RequiredType("Views.TitleView.TitleViewController");
            Type characterControllerType = RequiredType("Domains.CharacterSelect.CharacterSelectController");
            Array descriptors = (Array)allField.GetValue(null);

            foreach (object descriptor in descriptors)
            {
                Type dependencyType = (Type)GetInstanceProperty(descriptor, "Type");
                Assert.AreNotEqual(titleControllerType, dependencyType, "DependencyManager must not own TitleViewController.");
                Assert.AreNotEqual(characterControllerType, dependencyType, "DependencyManager must not own CharacterSelectController.");
            }
        }

        private static void VerifySceneManagerExIsRemoved()
        {
            const string sceneManagerExFullName = "Game.Core.Managers.Scene.SceneManagerEx";
            Type sceneManagerExType = Type.GetType($"{sceneManagerExFullName}, Assembly-CSharp");
            Assert.IsNull(sceneManagerExType, "SceneManagerEx type should be removed from Assembly-CSharp.");

            Type registryType = RequiredType("Game.System.Core.Manager.ManagerRegistry");
            FieldInfo allField = registryType.GetField("All", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(allField, "ManagerRegistry.All field was not found.");

            Array descriptors = (Array)allField.GetValue(null);

            foreach (object descriptor in descriptors)
            {
                Type managerType = (Type)GetInstanceProperty(descriptor, "Type");
                Assert.AreNotEqual(sceneManagerExFullName, managerType.FullName, "ManagerRegistry must not own SceneManagerEx.");
            }
        }

        private static void VerifyPureCSharpTitleSceneIsRemoved()
        {
            Type titleSceneType = Type.GetType("Domains.Scene.TitleScene, Assembly-CSharp");
            Assert.IsNull(titleSceneType, "Pure C# TitleScene should be removed; Unity TitleScene startup is owned by TitleSceneScope.");
        }

        private static object Resolve(object container, Type type)
        {
            MethodInfo resolve = container
                .GetType()
                .GetMethod("Resolve", new[] { typeof(Type), typeof(object) });

            Assert.IsNotNull(resolve, "Container Resolve(Type, object) method was not found.");
            return resolve.Invoke(container, new object[] { type, null });
        }

        private static object FindSingleObject(Type type)
        {
            UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsByType(
                type,
                FindObjectsInactive.Include);

            Assert.LessOrEqual(objects.Length, 1, $"Expected at most one {type.FullName}, found {objects.Length}.");
            return objects.Length == 1 ? objects[0] : null;
        }

        private static object GetStaticProperty(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(property, $"{type.FullName}.{propertyName} property was not found.");
            return property.GetValue(null);
        }

        private static object GetInstanceProperty(object instance, string propertyName)
        {
            PropertyInfo property = instance
                .GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(property, $"{instance.GetType().FullName}.{propertyName} property was not found.");
            return property.GetValue(instance);
        }

        private static Type RequiredType(string fullName)
        {
            Type type = Type.GetType($"{fullName}, Assembly-CSharp");
            Assert.IsNotNull(type, $"{fullName} type was not found in Assembly-CSharp.");
            return type;
        }
    }
}
