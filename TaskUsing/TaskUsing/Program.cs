using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;

namespace TaskUsing
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (!OperatingSystem.IsWindows())
            {
                Console.Error.WriteLine("Administrator elevation is only supported on Windows.");
                return 1;
            }

            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))
            {
                return RestartAsAdministrator(args) ? 0 : 1;
            }

            Console.WriteLine($"{"Process name",-35} {"Memory (MB)",14} {"CPU time",15}");
            Console.WriteLine(new string('-', 68));
            Console.WriteLine();// print resource usage for all processes
            Console.WriteLine($"{"Total"} {Process.GetProcesses().Sum(p => p.WorkingSet64) / (1024d * 1024d),14:N1} {"unavailable",15}");

            foreach (Process process in Process.GetProcesses())
            {
                using (process)
                {
                    try
                    {
                        double memoryInMegabytes = process.WorkingSet64 / (1024d * 1024d);
                        TimeSpan cpuTime = process.TotalProcessorTime;

                        Console.WriteLine($"{process.ProcessName,-35} {memoryInMegabytes,14:N1} {cpuTime,15}");
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine($"{process.ProcessName,-35} {"unavailable",14} {"unavailable",15}");
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        Console.WriteLine($"{process.ProcessName,-35} {"unavailable",14} {"unavailable",15}");
                    }
                }
            }

            var goup = Process.GetProcesses().GroupBy(p => p.ProcessName).Select(g => new { Key = g.Key, Processes = g.ToList(), MemoryInMegabytes = g.Sum(p => p.WorkingSet64) / (1024d * 1024d) }).ToList();
            foreach (var item in Enumerable.Range(1, 4))
            {
                Console.WriteLine();
            }
            Console.WriteLine($"{"Process name",-35} {"Count",14} {"Memory (MB)",14}");
            goup = goup.OrderByDescending(g => g.MemoryInMegabytes).ToList();
            foreach (var group in goup)
            {
                Console.WriteLine($"{group.Key,-35} {group.Processes.Count,14} {group.MemoryInMegabytes,14:N1}");
            }

           Console.ReadKey();

            return 0;
        }

        private static bool RestartAsAdministrator(string[] args)
        {
            string? executable = Environment.ProcessPath;
            if (string.IsNullOrEmpty(executable))
            {
                Console.Error.WriteLine("Cannot determine the executable path for administrator elevation.");
                return false;
            }

            var startInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Environment.CurrentDirectory
            };

            if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
            {
                string? assemblyPath = Assembly.GetEntryAssembly()?.Location;
                if (string.IsNullOrEmpty(assemblyPath))
                {
                    Console.Error.WriteLine("Cannot determine the application assembly path for administrator elevation.");
                    return false;
                }

                startInfo.ArgumentList.Add(assemblyPath);
            }

            foreach (string arg in args)
            {
                startInfo.ArgumentList.Add(arg);
            }

            try
            {
                using Process? elevatedProcess = Process.Start(startInfo);
                if (elevatedProcess == null)
                {
                    Console.Error.WriteLine("Failed to start the application as administrator.");
                    return false;
                }

                return true;
            }
            catch (Win32Exception ex)
            {
                Console.Error.WriteLine($"Failed to start the application as administrator: {ex.Message}");
                return false;
            }
        }
    }
}
