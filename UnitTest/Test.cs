
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class TestRunner
    {
        private void RunTest(Type target, bool runGhdl = true, bool runCpp = false)
        {
            // var programname = target.Assembly.GetName().Name;
            // //var targetfolder = Path.Combine(Path.GetTempPath(), programname);
            // var targetfolder = Path.GetDirectoryName(target.Assembly.Location);

            // var outputfolder = Path.Combine(targetfolder, "output");

            // if (!Directory.Exists(targetfolder))
            //     Directory.CreateDirectory(targetfolder);

            // if (Directory.Exists(outputfolder))
            //     Directory.Delete(outputfolder, true);

            // Environment.CurrentDirectory = targetfolder;

            var method = target.Assembly.EntryPoint;
            method.Invoke(null, new object[] { new string[0] });

        }


        // private static async Task CopyStreamAsync(TextReader source, TextWriter target)
        // {
        //     string read;
        //     while ((read = await source.ReadLineAsync()) != null)
        //         await target.WriteLineAsync(read);
        // }

        // private int RunExternalProgram(string command, string arguments, string workingfolder)
        // {
        //     var psi = new System.Diagnostics.ProcessStartInfo(command, arguments)
        //     {
        //         WorkingDirectory = workingfolder,
        //         RedirectStandardError = true,
        //         RedirectStandardOutput = true,
        //         UseShellExecute = false
        //     };

        //     var ps = System.Diagnostics.Process.Start(psi);
        //     var errorLine = string.Empty;

        //     Func<StreamReader, TextWriter, Task> copyAndCheck = async (source, sink) =>
        //     {
        //         string line;
        //         while ((line = await source.ReadLineAsync()) != null)
        //         {
        //             // Check for error markers
        //             if (string.IsNullOrWhiteSpace(errorLine) && (line ?? string.Empty).Contains("error"))
        //                 errorLine = line;

        //             await sink.WriteLineAsync(line);
        //         }
        //     };


        //     var tasks = Task.WhenAll(
        //         copyAndCheck(ps.StandardOutput, Console.Out),
        //         copyAndCheck(ps.StandardError, Console.Out)
        //     );

        //     ps.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
        //     if (ps.HasExited)
        //     {
        //         tasks.Wait(TimeSpan.FromSeconds(5));
        //         if (!string.IsNullOrWhiteSpace(errorLine))
        //             throw new Exception($"Console output indicates error: {errorLine}");

        //         return ps.ExitCode;
        //     }
        //     else
        //     {
        //         ps.Kill();
        //         throw new Exception($"Failed to run process within the time limit");
        //     }
        // }

        [TestMethod]
        public void RunAcceleration()
        {
            RunTest(typeof(Acceleration.Program), true, false);
        }

        [TestMethod]
        public void RunCache()
        {
            RunTest(typeof(Cache.Program), true, false);
        }

        [TestMethod]
        public void RunMagnitude()
        {
            RunTest(typeof(Magnitude.Program), true, false);
        }

        [TestMethod]
        public void RunPosition_Update()
        {
            RunTest(typeof(Position_Update.Program), true, false);
        }

        [TestMethod]
        public void RunVelocity_Update()
        {
            RunTest(typeof(Velocity_Update.Program), true, false);
        }

    }
}