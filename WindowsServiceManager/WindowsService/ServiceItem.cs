using System.Management;
using System.ServiceProcess;
using WindowsServiceManager.Functions.EnumParser;

namespace WindowsService.WindowsService
{
    /// <summary>
    /// Service info (Detailed)
    /// </summary>
    internal class ServiceItem
    {
        #region Public parameter

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ServiceControllerStatus Status { get; set; }
        public ServiceStartMode StartupType { get; set; }
        public bool TriggerStart { get; set; }
        public bool DelayedAutoStart { get; set; }
        public string ExecutePath { get; set; }
        public string Description { get; set; }
        public string LogonName { get; set; }
        public long ProcessId { get; set; }

        #endregion

        const string _log_target = "ServiceItem";

        public ServiceItem(string serviceName)
            : this(ServiceController.GetServices().
                  FirstOrDefault(x =>
                      x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                      x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)))
        {
        }

        public ServiceItem(ServiceController sc, ManagementObject mo = null)
        {
            mo ??= new ManagementClass("Win32_Service").
               GetInstances().
               OfType<ManagementObject>().
               FirstOrDefault(x => sc.ServiceName == x["Name"] as string);

            this.Name = sc.ServiceName;
            this.DisplayName = sc.DisplayName;
            this.Status = sc.Status;
            this.StartupType = sc.StartType;
            this.TriggerStart = StartupChecker.IsTriggeredStart(sc);
            this.TriggerStart = StartupChecker.IsDelayedAutoStart(sc, mo);
            if (mo != null)
            {
                this.ExecutePath = mo["PathName"] as string;
                this.Description = mo["Description"] as string;
                this.LogonName = mo["StartName"] as string;
                this.ProcessId = (uint)mo["ProcessId"];
            }
        }

        public static ServiceItem[] Load(string serviceName = null)
        {
            var services = serviceName == null ?
                ServiceController.GetServices() :
                ServiceController.GetServices().
                    Where(x =>
                        x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                        x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
            var wmi_services = new ManagementClass("Win32_Service").
                GetInstances().
                OfType<ManagementObject>();

            return services.
                Select(sc => new ServiceItem(sc, wmi_services.FirstOrDefault(mo => sc.ServiceName == mo["Name"] as string))).
                ToArray();
        }

        /// <summary>
        /// Service exists check.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool Exists(string serviceName)
        {
            Logger.WriteLine("Info", $"Checking existence of {_log_target}: {serviceName}");
            var sc = ServiceController.GetServices().
                    FirstOrDefault(x =>
                        x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                        x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
            return sc != null;
        }

        /// <summary>
        /// Service exists check with startup mode.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="modeText"></param>
        /// <returns></returns>
        public static bool Exists(string serviceName, string modeText)
        {
            Logger.WriteLine("Info", $"Checking existence of {_log_target} with mode: {serviceName}, {modeText}");
            try
            {
                var sc = ServiceController.GetServices().
                        FirstOrDefault(x =>
                            x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                            x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                return sc != null &&
                    sc.StartType == ServiceStartModeParser.ParamsToRaw(modeText);
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error", $"Error while checking existence of service {serviceName}.");
                Logger.WriteRaw(e.ToString());

            }
            return false;
        }

        /// <summary>
        /// Service start. instance method.
        /// </summary>
        /// <returns></returns>
        public bool ToStart()
        {
            return ToStart(this.Name);
        }

        /// <summary>
        /// Service start. static method.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ToStart(string serviceName)
        {
            Logger.WriteLine("Info", $"Starting {_log_target}: {serviceName}");
            try
            {
                var sc = ServiceController.GetServices().
                    FirstOrDefault(x =>
                        x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                        x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                if (sc.Status == ServiceControllerStatus.Paused && sc.CanPauseAndContinue)
                {
                    sc.Continue();
                    Logger.WriteLine("Info", $"Service {serviceName} continued from paused state.");
                    return true;
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                    Logger.WriteLine("Info", $"Service {serviceName} started.");
                    return true;
                }
                else
                {
                    Logger.WriteLine("Warning", $"Service {serviceName} is already running or in a state that cannot be started.");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error", $"Failed to start service {serviceName}.");
                Logger.WriteRaw(e.ToString());
            }
            return false;
        }

        /// <summary>
        /// Service stop. instance method.
        /// </summary>
        /// <returns></returns>
        public bool ToStop()
        {
            return ToStop(this.Name);
        }

        /// <summary>
        /// Service stop. static method.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ToStop(string serviceName)
        {
            Logger.WriteLine("Info", $"Stopping {_log_target}: {serviceName}");
            try
            {
                var sc = ServiceController.GetServices().
                    FirstOrDefault(x =>
                        x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                        x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                if (sc.Status == ServiceControllerStatus.Running && sc.CanStop)
                {
                    sc.Stop();
                    return true;
                }
                else if (sc.Status == ServiceControllerStatus.Paused && sc.CanPauseAndContinue)
                {
                    sc.Continue();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    sc.Stop();
                    return true;
                }
                else
                {
                    Logger.WriteLine("Warning", $"Service {serviceName} is already stopped or in a state that cannot be stopped.");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error", $"Failed to stop service {serviceName}.");
                Logger.WriteRaw(e.ToString());
            }
            return false;
        }

        /// <summary>
        /// Service restart. instance method.
        /// </summary>
        /// <returns></returns>
        public bool ToRestart()
        {
            return ToRestart(this.Name);
        }

        /// <summary>
        /// Service restart. static method.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static bool ToRestart(string serviceName)
        {
            Logger.WriteLine("Info", $"Restarting {_log_target}: {serviceName}");
            try
            {
                var sc = ServiceController.GetServices().
                    FirstOrDefault(x =>
                        x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                        x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                if (sc.Status == ServiceControllerStatus.Running && sc.CanStop)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    sc.Start();
                    return true;
                }
                else if (sc.Status == ServiceControllerStatus.Paused && sc.CanPauseAndContinue)
                {
                    sc.Continue();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    sc.Start();
                    return true;
                }
                else
                {
                    Logger.WriteLine("Info", $"Service {serviceName} is not ReStarting the service.");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error", $"Failed to restart service {serviceName}.");
                Logger.WriteRaw(e.ToString());
            }
            return false;
        }

        /// <summary>
        /// Change startup type of this service.
        /// </summary>
        /// <param name="modeText"></param>
        /// <returns></returns>
        public bool ChangeStartupType(string modeText)
        {
            Logger.WriteLine("Info", $"Changing startup type of {_log_target}: {this.Name} to {modeText}");
            try
            {
                var mode = ServiceStartModeParser.ParamsToRaw(modeText);
                var sc = ServiceController.GetServices().
                    FirstOrDefault(x =>
                        x.ServiceName.Equals(this.Name, StringComparison.OrdinalIgnoreCase) ||
                        x.DisplayName.Equals(this.Name, StringComparison.OrdinalIgnoreCase));
                var wmi = new ManagementClass("Win32_Service").
                    GetInstances().
                    OfType<ManagementObject>().
                    FirstOrDefault(x => sc.ServiceName == x["Name"] as string);

                var ret = (uint)wmi.InvokeMethod("ChangeStartMode", new object[] { mode.ToString() });
                Logger.WriteLine("Info", $"ChangeStartMode returned: {ret}");
                return ret == 0;
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error", $"Failed to change startup type of service {this.Name}.");
                Logger.WriteRaw(e.ToString());
            }
            return false;
        }
    }
}
