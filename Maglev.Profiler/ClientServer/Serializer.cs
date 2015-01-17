using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace MaglevProfiler.ClientServer
{
    class Serializer
    {
        public string Write(List<RemotelyModifiableClassInfo> classes)
        {
            var sb = new StringBuilder();
            sb.Append(classes.Count);
            sb.Append("|");
            foreach (var remotelyModifiableClassInfo in classes)
            {
                WriteRemotelyModifiableClassInfo(sb, remotelyModifiableClassInfo);
            }
            return sb.ToString();
        }

        public string Write(RemotelyModifiableClassInfo classInfo)
        {
            var sb = new StringBuilder();
            WriteRemotelyModifiableClassInfo(sb, classInfo);
            return sb.ToString();
        }


        public string Write(Collection<IntervalNode> frameHistory)
        {

            var sb = new StringBuilder();
            WriteIntervalNodeCollection(sb, frameHistory);
            return sb.ToString();
        }

        private void WriteIntervalNodeCollection(StringBuilder sb, Collection<IntervalNode> frameHistory)
        {
            var sw = new Stopwatch();
            sw.Start();

            sb.Append(frameHistory.Count);
            sb.Append("|");
            foreach (var node in frameHistory)
            {
                WriteIntervalNode(sb, node);
            }

            sw.Stop();
            Console.WriteLine(String.Format("Time taken to serialize frame history:{0}ms",sw.ElapsedMilliseconds));
        }

        private void WriteIntervalNode(StringBuilder sb, IntervalNode node)
        {
            sb.Append(node.Name);
            sb.Append("|");
            sb.Append(node.FrameType);
            sb.Append("|");
            sb.Append(node.ManagedThreadId);
            sb.Append("|");
            sb.Append(node.TotalMilliseconds);
            sb.Append("|");
            sb.Append(node.Logs.Count);
            sb.Append("|");
            foreach (var log in node.Logs)
            {
                sb.Append(log);
                sb.Append("|");
            }
            sb.Append(node.Children.Count);
            sb.Append("|");
            foreach (var child in node.Children)
            {
                WriteIntervalNode(sb, child);
            }
        }

        private static void WriteRemotelyModifiableClassInfo(StringBuilder sb, RemotelyModifiableClassInfo remotelyModifiableClassInfo)
        {
            sb.Append(remotelyModifiableClassInfo.AssemblyName);
            sb.Append("|");
            sb.Append(remotelyModifiableClassInfo.ClassName);
            sb.Append("|");
            sb.Append(remotelyModifiableClassInfo.ClassProperties.Count);
            sb.Append("|");
            foreach (var property in remotelyModifiableClassInfo.ClassProperties)
            {
                sb.Append(property.IsStatic);
                sb.Append("|");
                sb.Append(property.PropertyName);
                sb.Append("|");
                sb.Append(property.PropertyType);
                sb.Append("|");
                sb.Append(property.PropertyValue);
                sb.Append("|");
                sb.Append(property.EnumCount);
                sb.Append("|");
                for (int i = 0; i < property.EnumCount;i++)
                {
                    sb.Append(property.PotentialValues[i]);
                    sb.Append("|");
                }
            }
        }

        public RemotelyModifiableClassInfo ReadRemotelyModifyClassInfo(String data)
        {
            var rv = new RemotelyModifiableClassInfo();
            var items = data.Split(new char[] { '|' });
            var index = 0;
            var assemblyName = items[index];
            index++;
            var className = items[index];
            index++;
            var propertyCount = Int32.Parse(items[index]);
            index++;
            var classProperties = new List<RemotelyModifiablePropertyInfo>(propertyCount);
            for (int j = 0; j < propertyCount; j++)
            {
                var isStatic = Boolean.Parse(items[index]);
                index++;
                var propName = items[index];
                index++;
                var propType = items[index];
                index++;
                var propValue = items[index];
                index++;
                var enumCount = Int32.Parse(items[index]);
                index++;

                var lst = new List<String>();
                for (int i = 0; i < enumCount; i++)
                {
                    lst.Add(items[index]);
                    index++;
                }

                if (enumCount > 0)
                {
                    var property = new RemotelyModifiablePropertyInfo(propName, propType, propValue, enumCount, lst);
                    property.IsStatic = isStatic;
                    classProperties.Add(property);
                }
                else
                {
                    var property = new RemotelyModifiablePropertyInfo(propName, propType, propValue);
                    property.IsStatic = isStatic;
                    classProperties.Add(property);
                }
            }

            rv.ClassProperties = classProperties;
            rv.AssemblyName = assemblyName;
            rv.ClassName = className;
            return rv;
        }

        public List<RemotelyModifiableClassInfo> ReadAllRemotelyModifiableClassInfo(string data)
        {
            var rv = new List<RemotelyModifiableClassInfo>();
            var items = data.Split(new char[] {'|'});
            var count = Int32.Parse(items[0]);
            var index = 1;
            for (int i = 0; i < count; i++)
            {
                var classInfo = new RemotelyModifiableClassInfo();

                var assemblyName = items[index];
                index++;
                var className = items[index];
                index++;
                var propertyCount = Int32.Parse(items[index]);
                index++;
                var classProperties = new List<RemotelyModifiablePropertyInfo>(propertyCount);
                for (int j = 0; j < propertyCount; j++)
                {
                    var isStatic = Boolean.Parse(items[index]);
                    index++;
                    var propName = items[index];
                    index++;
                    var propType = items[index];
                    index++;
                    var propValue = items[index];
                    index++;

                    var enumCount = Int32.Parse(items[index]);
                    index++;
                    if (enumCount > 0)
                    {
                        var lst = new List<String>();
                        for (int k = 0; k < enumCount; k++)
                        {
                            lst.Add(items[index]);
                            index++;
                        }

                        var property = new RemotelyModifiablePropertyInfo(propName, propType, propValue, enumCount, lst);
                        property.IsStatic = isStatic;
                        classProperties.Add(property);
                    }
                    else
                    {
                        var property = new RemotelyModifiablePropertyInfo
                        {
                            PropertyName = propName,
                            PropertyType = propType,
                            PropertyValue = propValue
                        };

                        property.IsStatic = isStatic;
                        classProperties.Add(property);
                    }
                }

                classInfo.AssemblyName = assemblyName;
                classInfo.ClassName = className;
                classInfo.ClassProperties = classProperties;

                rv.Add(classInfo);
            }
            return rv;
        }

        public IEnumerable<IntervalNode> ReadFrameHistory(string data)
        {
            var rv = new List<IntervalNode>();
            var items = data.Split(new char[] { '|' });

            var index = 0;
            var numFrames = Int32.Parse(items[index]);

            index++;
            for (int i = 0; i < numFrames; i++)
            {
                var node = ReadIntervalNode(ref index, items);
                rv.Add(node);
            }

            return rv;
        }

        private IntervalNode ReadIntervalNode(ref int index, string[] items)
        {
            var rv = new IntervalNode();

            rv.Name = items[index];
            index++;
            rv.FrameType = (ProfilerFrameType)Enum.Parse(typeof(ProfilerFrameType), items[index]);
            index++;
            rv.ManagedThreadId = Int32.Parse(items[index]);
            index++;
            rv.TotalMilliseconds = items[index];
            index++;
            var logCount = Int32.Parse(items[index]);
            index++;
            var logs = new List<String>(logCount);
            rv.Logs = logs;
            for (int i = 0; i < logCount; i++)
            {
                logs.Add(items[index]);
                index++;
            }

            var childCount = Int32.Parse(items[index]);
            index++;

            for (int i = 0; i < childCount; i++)
            {
               var child = ReadIntervalNode(ref index, items);
                rv.AddChild(child);
            }

            return rv;
        }
    }
}
