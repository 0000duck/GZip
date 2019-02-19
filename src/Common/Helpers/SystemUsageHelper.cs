using System.Diagnostics;

namespace Common.Helpers
{
    public static class SystemUsageHelper
    {
        static readonly PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        static readonly PerformanceCounter RamCounter = new PerformanceCounter("Memory", "Available MBytes");

        public static float GetCurrentCpuUsage()
        {
            return CpuCounter.NextValue();
        }

        public static float GetAvailableRam()
        {
            return RamCounter.NextValue();
        }
    }
}
