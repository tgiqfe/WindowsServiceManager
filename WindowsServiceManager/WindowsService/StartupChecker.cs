using System.Management;
using System.ServiceProcess;
using WindowsService.Functions;

namespace WindowsService.WindowsService
{
    internal class StartupChecker
    {
        /// <summary>
        /// Service is set to Delayed Auto Start.
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="mo"></param>
        /// <returns></returns>
        public static bool IsDelayedAutoStart(ServiceController sc, ManagementObject mo = null)
        {
            if (sc == null) return false;
            if (mo == null)
            {
                var keyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services";
                using (var regKey = RegistryFunctions.GetRegistryKey(keyPath, false))
                {
                    if (regKey != null)
                    {
                        using (var subKey = regKey.OpenSubKey(sc.ServiceName))
                        {
                            if (subKey != null)
                            {
                                var startValue = subKey.GetValue("Start");
                                var delayedAutoStartValue = subKey.GetValue("DelayedAutostart");
                                if (startValue != null && delayedAutoStartValue != null)
                                {
                                    return (int)startValue == 2 && (int)delayedAutoStartValue == 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return sc.StartType == ServiceStartMode.Automatic && mo["DelayedAutoStart"] as bool? == true;
            }
            return false;
        }

        /// <summary>
        /// Service is set to Triggered Start.
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static bool IsTriggeredStart(ServiceController sc)
        {
            if (sc == null) return false;
            var keyPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services";
            using (var regKey = RegistryFunctions.GetRegistryKey(keyPath, false))
            {
                if (regKey != null)
                {
                    using (var subKey = regKey.OpenSubKey(sc.ServiceName))
                    {
                        if (subKey != null)
                        {
                            return subKey.GetSubKeyNames().Any(x =>
                                x.Equals("TriggerInfo", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
            }
            return false;
        }
    }
}
