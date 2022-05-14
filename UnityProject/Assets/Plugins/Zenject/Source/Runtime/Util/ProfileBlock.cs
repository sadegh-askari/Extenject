#if !ZEN_TESTS_OUTSIDE_UNITY && (UNITY_EDITOR || ZEN_INTERNAL_PROFILING || DEBUG || BUILD_WITH_ZEN_PROFILER)
#define ENABLE_ZEN_PROFILER

using System.Collections.Generic;
using UnityEngine.Profiling;
using ModestTree;
#endif

using System.Runtime.CompilerServices;
using System.Threading;
using System;
using System.Text.RegularExpressions;

namespace Zenject
{
    [NoReflectionBaking]
    public static class ProfileBlock
    {
        public static Thread UnityMainThread { get; set; }
        public static Regex ProfilePattern { get; set; }

#if ENABLE_ZEN_PROFILER
        static Dictionary<int, SamplerEnder> _samplerCache = new Dictionary<int, SamplerEnder>();
        static Dictionary<string, string> _filePathToName = new Dictionary<string, string>();
        static object[] _argArray = new object[4];
        static CustomSampler _profilerOverhead;

#if UNITY_EDITOR
        // Required for disabling domain reload in enter the play mode feature. See: https://docs.unity3d.com/Manual/DomainReloading.html
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticValues()
        {
            if (!UnityEditor.EditorSettings.enterPlayModeOptionsEnabled)
            {
                return;
            }

            _samplerCache.Clear();
            _filePathToName.Clear();
        }
#endif

        static int GetHashCode(object p1, object[] args)
        {
            if (args == null)
                return p1.GetHashCode();

            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 29 + p1.GetHashCode();

                foreach (object o in args)
                {
                    if (o != null)
                    {
                        hash = hash * 29 + o.GetHashCode();
                    }
                    else
                    {
                        break;
                    }
                }

                return hash;
            }
        }

        public static IDisposable StartForMethod([CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
        {
            if (!ShouldStart())
                return null;

            ResetArgArray();
            _argArray[0] = GetFileName(filePath);
            _argArray[1] = methodName;
            return StartInternal(GetSampler("{0}.{1}", _argArray));
        }

        public static IDisposable StartForMethod(object obj, [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
        {
            ResetArgArray();
            _argArray[0] = GetFileName(filePath);
            _argArray[1] = methodName;
            _argArray[2] = obj;
            return StartInternal(GetSampler("{0}.{1} ({2})", _argArray));
        }

        public static IDisposable StartForMethod(object obj1, object obj2, [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
        {
            if (!ShouldStart())
                return null;

            ResetArgArray();
            _argArray[0] = GetFileName(filePath);
            _argArray[1] = methodName;
            _argArray[2] = obj1;
            _argArray[3] = obj2;
            return StartInternal(GetSampler("{0}.{1} ({2}, {3})", _argArray));
        }

        public static IDisposable Start(string sampleNameFormat, object obj1, object obj2)
        {
            if (!ShouldStart())
                return null;

            ResetArgArray();
            _argArray[0] = obj1;
            _argArray[1] = obj2;
            return StartInternal(GetSampler(sampleNameFormat, _argArray));
        }

        public static IDisposable Start(string sampleNameFormat, object obj)
        {
            if (!ShouldStart())
                return null;

            ResetArgArray();
            _argArray[0] = obj;
            return StartInternal(GetSampler(sampleNameFormat, _argArray));
        }

        public static IDisposable Start(string sampleName)
        {
            if (!ShouldStart())
                return null;

            return StartInternal(GetSampler(sampleName, null));
        }

        private static IDisposable StartInternal(SamplerEnder ender)
        {
            Assert.That(Profiler.enabled);

            if (ProfilePattern == null || ProfilePattern.Match(ender.Sampler.name).Success)
            {
                ender.Sampler.Begin();
                return ender;
            }

            return null;
        }

        private static SamplerEnder GetSampler(string sampleNameFormat, object[] parameters)
        {
            _profilerOverhead ??= CustomSampler.Create("ProfilerOverhead");
            _profilerOverhead.Begin();

            // We need to ensure that we do not have per-frame allocations in ProfileBlock
            // to avoid infecting the test too much, so use a cache of formatted strings given
            // the input values
            // This only works if the input values do not change per frame
            int hash = GetHashCode(sampleNameFormat, parameters);

            SamplerEnder ender;

            if (!_samplerCache.TryGetValue(hash, out ender))
            {
                string samplerName = parameters != null ? string.Format(sampleNameFormat, parameters) : sampleNameFormat;
                ender = new SamplerEnder(CustomSampler.Create(samplerName));
                _samplerCache.Add(hash, ender);
            }

            _profilerOverhead.End();

            return ender;
        }

        private static string GetFileName(string filePath)
        {
            if (!_filePathToName.TryGetValue(filePath, out string name))
            {
                name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                _filePathToName.Add(filePath, name);
            }

            return name;
        }

        private static void ResetArgArray()
        {
            for (var i = 0; i < _argArray.Length; i++)
            {
                _argArray[i] = null;
            }
        }

        private static bool ShouldStart()
        {
            if (UnityMainThread != null && !UnityMainThread.Equals(Thread.CurrentThread))
            {
                return false;
            }

            if (!Profiler.enabled)
            {
                return false;
            }

            return true;
        }

        class SamplerEnder : IDisposable
        {
            public readonly CustomSampler Sampler;

            public SamplerEnder(CustomSampler sampler)
            {
                Sampler = sampler;
            }

            public void Dispose()
            {
                Sampler.End();
            }
        }
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable StartForMethod(string filePath = "", string methodName = "")
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable StartForMethod(object obj, string filePath = "", string methodName = "")
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable StartForMethod(object obj1, object obj2, string filePath = "", string methodName = "")
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Start()
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Start(string sampleNameFormat, object obj1, object obj2)
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Start(string sampleNameFormat, object obj)
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Start(string sampleName)
        {
            return null;
        }
#endif
    }
}