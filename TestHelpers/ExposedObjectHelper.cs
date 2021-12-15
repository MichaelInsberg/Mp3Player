using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Mp3Player.TestHelpers
{
    /// <summary>
    /// The exposed object helper.
    /// </summary>
    internal static class ExposedObjectHelper
    {
        /// <summary>
        /// The s_csharp invoke property type.
        /// </summary>
        private static readonly Type CsharpInvokePropertyType =
            typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                .Assembly
                .GetType("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");

        /// <summary>
        /// The invoke best method.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="instanceMethods">
        /// The instance methods.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        internal static bool InvokeBestMethod(object[] args, object target, List<MethodInfo> instanceMethods, out object result)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            if (instanceMethods.Count == 1)
            {
                // Just one matching instance method - call it
                if (TryInvoke(instanceMethods[0], target, args, out result))
                {
                    return true;
                }
            }
            else if (instanceMethods.Count > 1)
            {
                // Find a method with best matching parameters
                MethodInfo best = null;
                Type[] bestParams = null;
                Type[] actualParams = args.Select(p => p == null ? typeof(object) : p.GetType()).ToArray();

                bool IsAssignableFrom(Type[] a, Type[] b)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        if (!a[i].IsAssignableFrom(b[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                foreach (var method in instanceMethods.Where(m => m.GetParameters().Length == args.Length))
                {
                    Type[] parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();
                    if (IsAssignableFrom(parameters, actualParams) &&
                        (best == null || IsAssignableFrom(bestParams, parameters)))
                    {
                        best = method;
                        bestParams = parameters;
                    }
                }

                if (best != null && TryInvoke(best, target, args, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// The try invoke.
        /// </summary>
        /// <param name="methodInfo">
        /// The method info.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        internal static bool TryInvoke(MethodInfo methodInfo, object target, object[] args, out object result)
        {
            try
            {
                result = methodInfo.Invoke(target, args);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException || ex is TargetParameterCountException)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                }
                else
                {
                    throw;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// The get type args.
        /// </summary>
        /// <param name="binder">
        /// The binder.
        /// </param>
        /// <returns>
        /// The Type[].
        /// </returns>
        internal static Type[] GetTypeArgs(InvokeMemberBinder binder)
        {
            if (CsharpInvokePropertyType.IsInstanceOfType(binder))
            {
                PropertyInfo typeArgsProperty = CsharpInvokePropertyType.GetProperty("TypeArguments");
                if (typeArgsProperty is not null)
                {
                    return ((IEnumerable<Type>)typeArgsProperty.GetValue(binder, null) ?? Type.EmptyTypes).ToArray();
                }
            }
            return null;
        }
    }
}
