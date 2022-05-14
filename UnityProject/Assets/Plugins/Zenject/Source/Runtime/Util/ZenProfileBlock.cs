using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Zenject.Internal
{
    /// <summary>
    /// ProfileBlock specifically for Zenject. Will be enabled if ZEN_INTERNAL_PROFILING is defined.
    /// </summary>
    internal static class ZenProfileBlock
    {
#if ZEN_INTERNAL_PROFILING
        public static IDisposable StartForMethod([CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
        {
            return ProfileBlock.StartForMethod(filePath, methodName);
        }

        public static IDisposable StartForMethod(object obj, [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
        {
            return ProfileBlock.StartForMethod(obj, filePath, methodName);
        }

        public static IDisposable StartForMethod(object obj1, object obj2, [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
        {
            return ProfileBlock.StartForMethod(obj1, obj2, filePath, methodName);
        }

        public static IDisposable Start(string name)
        {
            return ProfileBlock.Start(name);
        }

        public static IDisposable Start(string name, object obj)
        {
            return ProfileBlock.Start(name, obj);
        }

        public static IDisposable Start(string name, object obj1, object obj2)
        {
            return ProfileBlock.Start(name, obj1, obj2);
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
        public static IDisposable Start(string name)
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Start(string name, object obj)
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Start(string name, object obj1, object obj2)
        {
            return null;
        }
#endif
    }
}