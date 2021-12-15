// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Defines the ExposedObject type. http://exposedobject.codeplex.com
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Mp3Player.TestHelpers
{
    /// <inheritdoc />
    /// <summary>
    /// The exposed object.
    /// </summary>
    public class ExposedObject : DynamicObject
    {
        /// <summary>
        /// The internal type
        /// </summary>
        private Type internalType;

        /// <summary>
        /// The instance methods.
        /// </summary>
        private Dictionary<string, Dictionary<int, List<MethodInfo>>> instanceMethods;

        /// <summary>
        /// The generic instance methods.
        /// </summary>
        private Dictionary<string, Dictionary<int, List<MethodInfo>>> genericInstanceMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mp3Player.TestHelpers.ExposedObject" /> class.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private ExposedObject(object obj)
        {
            ExObject = obj;
            InitType(obj.GetType());
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        public object ExObject { get; }

        /// <summary>
        /// The new.
        /// </summary>
        /// <typeparam name="T">The type T.
        /// </typeparam>
        /// <returns>
        /// The dynamic.
        /// </returns>
        public static dynamic New<T>()
        {
            return New(typeof(T));
        }

        /// <summary>
        /// The new.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The dynamic.
        /// </returns>
        public static dynamic New(Type type)
        {
            return new ExposedObject(Create(type));
        }

        /// <summary>
        /// The new operator with ability to create Object with arguments.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="arg">
        /// The arg.
        /// </param>
        /// <returns>
        /// The dynamic.
        /// </returns>
        public static dynamic New(Type type, object arg)
        {
            return new ExposedObject(Create(type, arg));
        }

        /// <summary>
        /// The from.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The dynamic.
        /// </returns>
        public static dynamic From(object obj)
        {
            return new ExposedObject(obj);
        }

        /// <summary>
        /// The cast.
        /// </summary>
        /// <param name="t">
        /// The t.
        /// </param>
        /// <typeparam name="T">
        /// The type T.
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public static T Cast<T>(ExposedObject t)
        {
            return (T)t?.ExObject;
        }

        /// <summary>
        /// The init type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        public void InitType(Type type)
        {
            internalType = type;

            instanceMethods = new Dictionary<string, Dictionary<int, List<MethodInfo>>>();
            foreach (IGrouping<string, MethodInfo> infos in internalType.GetMethods(
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Instance).Where(m => !m.IsGenericMethod).GroupBy(m => m.Name))
            {
                var dictionary1 = new Dictionary<int, List<MethodInfo>>();
                foreach (IGrouping<int, MethodInfo> grouping in infos.GroupBy(r => r.GetParameters().Length))
                {
                    dictionary1.Add(grouping.Key, grouping.ToList());
                }
                instanceMethods.Add(infos.Key, dictionary1);
            }

            genericInstanceMethods = new Dictionary<string, Dictionary<int, List<MethodInfo>>>();
            foreach (IGrouping<string, MethodInfo> infos in internalType.GetMethods(
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Instance).Where(m => m.IsGenericMethod).GroupBy(m => m.Name))
            {
                var dictionary1 = new Dictionary<int, List<MethodInfo>>();
                foreach (IGrouping<int, MethodInfo> grouping in infos.GroupBy(r => r.GetParameters().Length))
                {
                    dictionary1.Add(grouping.Key, grouping.ToList());
                }
                genericInstanceMethods.Add(infos.Key, dictionary1);
            }
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
        /// The <see cref="T:System.Boolean" />.
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

            if (!instanceMethods.ContainsKey(binder.Name))
            {
                Console.WriteLine(binder.Name + " not found!");
            }

            // Try to call a non-generic instance method
            if (typeArgs == null
                && instanceMethods.ContainsKey(binder.Name)
                && instanceMethods[binder.Name].ContainsKey(args.Length)
                && ExposedObjectHelper.InvokeBestMethod(args, ExObject, instanceMethods[binder.Name][args.Length], out result))
            {
                return true;
            }

            // Try to call a generic instance method
            if (!instanceMethods.ContainsKey(binder.Name) || !instanceMethods[binder.Name].ContainsKey(args.Length))
            {
                result = null;
                return false;
            }

            var methods = new List<MethodInfo>();

            foreach (var method in genericInstanceMethods[binder.Name][args.Length])
            {
                if (typeArgs != null && method.GetGenericArguments().Length == typeArgs.Length)
                {
                    methods.Add(method.MakeGenericMethod(typeArgs));
                }
            }

            if (ExposedObjectHelper.InvokeBestMethod(args, ExObject, methods, out result))
            {
                return true;
            }

            if (internalType.BaseType != null)
            {
                InitType(internalType.BaseType);
                return TryInvokeMember(binder, args, out result);
            }

            result = null;
            return false;
        }

        /// <inheritdoc />
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
        /// The <see cref="T:System.Boolean" />.
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentException(nameof(binder));
            }
            var propertyInfo = ExObject.GetType().GetProperty(
              binder.Name,
              BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null)
            {
                result = propertyInfo.GetValue(ExObject, null);
                return true;
            }

            var fieldInfo = ExObject.GetType().GetField(
              binder.Name,
              BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                result = fieldInfo.GetValue(ExObject);
                return true;
            }

            result = null;
            return false;
        }

        /// <inheritdoc />
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
        /// The <see cref="T:System.Boolean" />.
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw new ArgumentException(nameof(binder));
            }
            var propertyInfo = internalType.GetProperty(
              binder.Name,
              BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null)
            {
                propertyInfo.SetValue(ExObject, value, null);
                return true;
            }

            var fieldInfo = internalType.GetField(
              binder.Name,
              BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(ExObject, value);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// The try convert.
        /// </summary>
        /// <param name="binder">
        /// The binder.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.Boolean" />.
        /// </returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = ExObject;
            return true;
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="arg">
        /// The arg.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private static object Create(Type type, object arg)
        {
            // Create instance using Activator
            object res = Activator.CreateInstance(type, arg);
            return res;
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private static object Create(Type type)
        {
            ConstructorInfo constructorInfo = GetConstructorInfo(type);
            return constructorInfo.Invoke(Array.Empty<object>());
        }

        /// <summary>
        /// The get constructor info.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="ConstructorInfo"/>.
        /// </returns>
        private static ConstructorInfo GetConstructorInfo(Type type, params Type[] args)
        {
            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null);
            if (constructorInfo != null)
            {
                return constructorInfo;
            }

            throw new MissingMemberException(type.FullName,
                $".ctor({string.Join(", ", Array.ConvertAll(args, t => t.FullName))})");
        }
    }
}
