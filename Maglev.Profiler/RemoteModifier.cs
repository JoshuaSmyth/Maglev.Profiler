using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MaglevProfiler.Attributes;

namespace MaglevProfiler
{
    public class RemoteModifier
    {
        /// <summary>
        /// Gets all classes in all loaded assemblies that have the attribute [RemotelyModifiableClass]
        /// </summary>
        public IEnumerable<Type> GetAllModifiableClassTypes()
        {
            var rv = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                rv.AddRange(GetAllModifiableClassTypes(assembly));
            }

            return rv;
        }

        /// <summary>
        /// Gets all classes for an assembly that have the attribute [RemotelyModifiableClass]
        /// </summary>
        public IEnumerable<Type> GetAllModifiableClassTypes(Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(RemotelyModifiableClass), true).Length > 0);
        }

        /// <summary>
        /// Gets all properties that can be modified by the remote debugger
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<PropertyInfo> GetAllModifiableProperties(Type type)
        {
            var rv = new List<PropertyInfo>();
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof(RemotelyModifiableProperty),true).Length > 0)
                {
                    if (propertyInfo.CanRead && 
                        propertyInfo.CanWrite)
                    {
                        rv.Add(propertyInfo);

                    }
                }
            }

            return rv;
        }

        public RemotelyModifiableClassInfo GetRemotelyModifiableClassInfo(Type type)
        {
            var remoteClass = new RemotelyModifiableClassInfo(type.ToString());
            remoteClass.AssemblyName = type.Assembly.FullName;

            var properties = GetAllModifiableProperties(type);

            foreach (var p in properties)
            {
                var parameterType = p.GetSetMethod().GetParameters().First().ParameterType;

             
                bool isStatic = p.GetGetMethod().IsStatic && p.GetSetMethod().IsStatic;

                if (!isStatic)
                {
                    var info = new RemotelyModifiablePropertyInfo(p.Name, parameterType.ToString(), "");
                    info.IsStatic = isStatic;
                    remoteClass.ClassProperties.Add(info);
                }
                else
                {
                    var value = p.GetGetMethod().Invoke(null, null);
                    if (value is Enum)
                    {
                        // Get enums
                        var lst = new List<String>();
                        var enumCount = 0;
                        foreach (var enumValue in Enum.GetValues(parameterType))
                        {
                            lst.Add(enumValue.ToString());

                            enumCount++;
                        }

                        var info = new RemotelyModifiablePropertyInfo(p.Name, parameterType.ToString(), value.ToString(),enumCount, lst);
                        info.IsStatic = isStatic;
                        remoteClass.ClassProperties.Add(info);
                    }
                    else
                    {
                        var info = new RemotelyModifiablePropertyInfo(p.Name, parameterType.ToString(), value.ToString());
                        info.IsStatic = isStatic;
                        remoteClass.ClassProperties.Add(info);
                    }
                }
            }

            return remoteClass;
        }

        public IEnumerable<RemotelyModifiableClassInfo> GetAllRemotelyModifiableClasses()
        {
            var classes = GetAllModifiableClassTypes();
            return classes.Select(GetRemotelyModifiableClassInfo).ToList();
        }

        public void UpdateRemotelyModifiableClass(RemotelyModifiableClassInfo classInfo)
        {
           // Find the class by name
            var t = Type.GetType(classInfo.ClassName + "," + classInfo.AssemblyName);

            if (t == null)
                return;         // TODO perhaps error this?

            foreach(var p in classInfo.ClassProperties)
            {
                var property = t.GetProperty(p.PropertyName);
                if (p.IsEnum)
                {
                    var getMethod = property.GetGetMethod();
                    var enumType = getMethod.ReturnType;

                    var newValue = Enum.Parse(enumType, p.PropertyValue);
                    var setMethod = property.GetSetMethod();
                    setMethod.Invoke(null, new[] { newValue });
                }
                else
                {
                    property.GetSetMethod().Invoke(null, new[] { Convert.ChangeType(p.PropertyValue, property.PropertyType) });
                }
            }
        }
    }
}
