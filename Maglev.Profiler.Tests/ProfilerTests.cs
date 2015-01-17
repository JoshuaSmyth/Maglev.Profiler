using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using TFProfiler;
using TinyFrog.Profiler;

namespace TFProfiler.Tests
{
    [TestFixture]
    public class ProfilingTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ProfileTree_TCPIP_RegressionTest_DataSetA_Succeeds()
        {
           
            Profiler.Enable();
            for (int i = 0; i < 10; i++)
            {
                Profiler.ProfileFrame(ProfilerFrameType.NoneSpecified);
                using (var p = Profiler.ProfileSection("Game Loop"))
                {
                    Thread.Sleep(1);

                    using (var p2 = Profiler.ProfileSection("Render"))
                    {
                        using (var p3 = Profiler.ProfileSection("Render Geometry"))
                        {
                            Thread.Sleep(1);
                            Profiler.Log("Number of Triangles:25000");
                        }
                        using (var p3 = Profiler.ProfileSection("Render Actors"))
                        {
                            Thread.Sleep(1);
                            Profiler.Log("Number of Actors:5");
                        }
                        using (var p3 = Profiler.ProfileSection("Render UI"))
                        {
                            Thread.Sleep(1);
                            Profiler.Log("Number of UI Elements:5");
                        }
                    }

                    using (var p2 = Profiler.ProfileSection("Update"))
                    {
                        using (var p3 = Profiler.ProfileSection("Update AI"))
                        {
                            Thread.Sleep(1);
                        }

                        using (var p3 = Profiler.ProfileSection("Update Player"))
                        {
                            Thread.Sleep(2);
                        }
                    }
                }
            }

            var data = Profiler.Serialize();

            Trace.WriteLine(data);
        }

    }
}
