using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HexOSModule
{
    internal static class ReflectionHelper
    {
        internal static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetSafeTypes).FirstOrDefault(t => t.FullName == null ? false : t.FullName.Equals(fullName));
        }

        internal static Type FindType(string fullName, string assemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetSafeTypes).FirstOrDefault(t => t.FullName == null ? false : t.FullName.Equals(fullName) && t.Assembly.GetName().Name.Equals(assemblyName));
        }

        internal static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(x => x != null);
            }
            catch (Exception)
            {
                return new List<Type>();
            }
        }

        private static readonly Dictionary<string, MemberInfo> _memberCache = new Dictionary<string, MemberInfo>();

        internal static T GetCachedMember<T>(this Type type, string member) where T : MemberInfo
        {
            // Use AssemblyQualifiedName and the member name as a unique key to prevent a collision if two types have the same member name
            var key = type.AssemblyQualifiedName + member;
            if (_memberCache.ContainsKey(key)) return _memberCache[key] is T ? (T)_memberCache[key] : null;

            MemberInfo memberInfo = type.GetMember(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).FirstOrDefault();
            _memberCache[key] = memberInfo;

            return memberInfo is T ? (T)memberInfo : null;
        }

        internal static T GetValue<T>(this Type type, string member, object target = null)
        {
            var fieldMember = type.GetCachedMember<FieldInfo>(member);
            var propertyMember = type.GetCachedMember<PropertyInfo>(member);

            return (T)((fieldMember != null ? fieldMember.GetValue(target) : default(T)) ?? (propertyMember != null ? propertyMember.GetValue(target, null) : default(T)));
        }

        internal static void SetValue(this Type type, string member, object value, object target = null)
        {
            var fieldMember = type.GetCachedMember<FieldInfo>(member);
            if (fieldMember != null)
                fieldMember.SetValue(target, value);

            var propertyMember = type.GetCachedMember<PropertyInfo>(member);
            if (propertyMember != null)
                propertyMember.SetValue(target, value, null);
        }

        internal static T CallMethod<T>(this Type type, string method, object target = null, params object[] arguments)
        {
            var member = type.GetCachedMember<MethodInfo>(method);
            if (member != null)
                return (T)(member.Invoke(target, arguments));

            return default(T);
        }

        internal static void CallMethod(this Type type, string method, object target = null, params object[] arguments)
        {
            var member = type.GetCachedMember<MethodInfo>(method);
            if (member != null)
                member.Invoke(target, arguments);
        }

        internal static T GetValue<T>(this object @object, string member)
        {
            return @object.GetType().GetValue<T>(member, @object);
        }

        internal static void SetValue(this object @object, string member, object value)
        {
            @object.GetType().SetValue(member, value, @object);
        }

        internal static T CallMethod<T>(this object @object, string member, params object[] arguments)
        {
            return @object.GetType().CallMethod<T>(member, @object, arguments);
        }

        internal static void CallMethod(this object @object, string member, params object[] arguments)
        {
            @object.GetType().CallMethod(member, @object, arguments);
        }

        internal static IEnumerable<DictionaryEntry> CastDict(this IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }
    }
}