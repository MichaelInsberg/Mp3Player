using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Mp3Player.TestHelpers
{
    /// <summary>
    /// The exposed class.
    /// </summary>
    public class ExposedClass : DynamicObject
    {
        /// <summary>
        /// The internal type
        /// </summary>
        private readonly Type internalType;

        /// <summary>
        /// The static methods.
        /// </summary>
        private readonly Dictionary<string, Dictionary<int, List<MethodInfo>>> staticMethods;

        /// <summary>
        /// The gen static methods.
        /// </summary>
        private readonly Dictionary<string, Dictionary<int, List<MethodInfo>>> genStaticMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExposedClass"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        private ExposedClass(Type type)
        {
            internalType = type;

            staticMethods =
                internalType
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .Where(m => !m.IsGenericMethod)
                    .GroupBy(m => m.Name)
                    .ToDictionary(
                        p => p.Key,
                        p => p.GroupBy(r => r.GetParameters().Length).ToDictionary(r => r.Key, r => r.ToList()));

            genStaticMethods =
                internalType
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.IsGenericMethod)
                    .GroupBy(m => m.Name)
                    .ToDictionary(
                        p => p.Key,
                        p => p.GroupBy(r => r.GetParameters().Length).ToDictionary(r => r.Key, r => r.ToList()));
        }

        /// <summary>
        /// The from.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The dynamic.
        /// </returns>
        public static dynamic From(Type type)
        {
            return new ExposedClass(type);
        }
        /// <summary>
        /// The try invoke member.
        /// </summary>
        /// <param name="binder">
        /// The binder.
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
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentException(nameof(binder));
            }
            if (args == null)
            {
                throw new ArgumentException(nameof(args));
            }

            // Get type args of the call
            Type[] typeArgs = ExposedObjectHelper.GetTypeArgs(binder);
            if (typeArgs is { Length: 0 })
            {
                typeArgs = null;
            }

            // Try to call a non-generic instance method
            if (typeArgs == null
                    && staticMethods.ContainsKey(binder.Name)
                    && staticMethods[binder.Name].ContainsKey(args.Length)
                    && ExposedObjectHelper.InvokeBestMethod(args, null, staticMethods[binder.Name][args.Length], out result))
            {
                return true;
            }

            // Try to call a generic instance method
            if (staticMethods.ContainsKey(binder.Name)
                    && staticMethods[binder.Name].ContainsKey(args.Length))
            {
                var methods = new List<MethodInfo>();

                foreach (var method in genStaticMethods[binder.Name][args.Length])
                {
                    if (typeArgs != null && method.GetGenericArguments().Length == typeArgs.Length)
                    {
                        methods.Add(method.MakeGenericMethod(typeArgs));
                    }
                }

                if (ExposedObjectHelper.InvokeBestMethod(args, null, methods, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// The try set member.
        /// </summary>
        /// <param name="binder">
        /// The binder.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw new ArgumentException(nameof(binder));
            }
            var propertyInfo = internalType.GetProperty(
                binder.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (propertyInfo != null)
            {
                propertyInfo.SetValue(null, value, null);
                return true;
            }

            var fieldInfo = internalType.GetField(
                binder.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(null, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// The try get member.
        /// </summary>
        /// <param name="binder">
        /// The binder.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentException(nameof(binder));
            }
            var propertyInfo = internalType.GetProperty(
                binder.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (propertyInfo != null)
            {
                result = propertyInfo.GetValue(null, null);
                return true;
            }

            var fieldInfo = internalType.GetField(
                binder.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            if (fieldInfo != null)
            {
                result = fieldInfo.GetValue(null);
                return true;
            }

            result = null;
            return false;
        }
    }
}
