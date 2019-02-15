﻿using System;
using StackExchange.Profiling.Internal;

namespace StackExchange.Profiling
{
    /// <summary>
    /// Handy extensions for <see cref="MiniProfilerBaseOptions"/>.
    /// </summary>
    public static class MiniProfilerOptionsExtensions
    {
        /// <summary>
        /// Sets a provider as the <see cref="MiniProfilerBaseOptions.ProfilerProvider"/> and optionally as the provider for <see cref="MiniProfiler.Current"/>.
        /// Extension method to support chaining while retaining the derivative options type.
        /// </summary>
        /// <typeparam name="T">The subtype of <see cref="MiniProfilerBaseOptions"/> to use (inferred for common usage).</typeparam>
        /// <param name="options">The options to set the provider on.</param>
        /// <param name="provider">The provider to use.</param>
        /// <param name="setAsCurrentProvider">Whether it should be set as the provider for getting <see cref="MiniProfiler.Current"/>.</param>
        public static T SetProvider<T>(this T options, IAsyncProfilerProvider provider, bool setAsCurrentProvider = true) where T : MiniProfilerBaseOptions
        {
            options.ProfilerProvider = provider ?? throw new ArgumentException(nameof(provider));
            if (setAsCurrentProvider)
            {
                MiniProfilerBaseOptions.CurrentProfilerProvider = provider;
            }
            return options;
        }

        /// <summary>
        /// Excludes an assembly from stack traces, convenience method for chaining, basically <see cref="MiniProfilerBaseOptions.ExcludedAssemblies"/>.Add(assembly)
        /// </summary>
        /// <typeparam name="T">The subtype of <see cref="MiniProfilerBaseOptions"/> to use (inferred for common usage).</typeparam>
        /// <param name="options">The options to exclude the assembly on.</param>
        /// <param name="assembly">The assembly name to exclude from stack traces.</param>
        public static T ExcludeAssembly<T>(this T options, string assembly) where T : MiniProfilerBaseOptions
        {
            options.ExcludedAssemblies.Add(assembly);
            return options;
        }

        /// <summary>
        /// Excludes a method from stack traces, convenience method for chaining, basically <see cref="MiniProfilerBaseOptions.ExcludedMethods"/>.Add(assembly)
        /// </summary>
        /// <typeparam name="T">The subtype of <see cref="MiniProfilerBaseOptions"/> to use (inferred for common usage).</typeparam>
        /// <param name="options">The options to exclude the method on.</param>
        /// <param name="method">The method name to exclude from stack traces.</param>
        public static T ExcludeMethod<T>(this T options, string method) where T : MiniProfilerBaseOptions
        {
            options.ExcludedMethods.Add(method);
            return options;
        }

        /// <summary>
        /// Excludes a type from stack traces, convenience method for chaining, basically <see cref="MiniProfilerBaseOptions.ExcludedTypes"/>.Add(assembly)
        /// </summary>
        /// <typeparam name="T">The subtype of <see cref="MiniProfilerBaseOptions"/> to use (inferred for common usage).</typeparam>
        /// <param name="options">The options to exclude the type on.</param>
        /// <param name="type">The type name to exclude from stack traces.</param>
        public static T ExcludeType<T>(this T options, string type) where T : MiniProfilerBaseOptions
        {
            options.ExcludedTypes.Add(type);
            return options;
        }

        /// <summary>
        /// Excludes a path from being profiled, convenience method for chaining, basically <see cref="MiniProfilerBaseOptions.IgnoredPaths"/>.Add(assembly)
        /// </summary>
        /// <typeparam name="T">The subtype of <see cref="MiniProfilerBaseOptions"/> to use (inferred for common usage).</typeparam>
        /// <param name="options">The options to exclude the type on.</param>
        /// <param name="path">The path to exclude from profiled.</param>
        public static T IgnorePath<T>(this T options, string path) where T : MiniProfilerBaseOptions
        {
            options.IgnoredPaths.Add(path);
            return options;
        }
    }
}
