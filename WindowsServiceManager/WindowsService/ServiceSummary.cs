using System.ServiceProcess;
using WindowsService.Functions;

namespace WindowsService.WindowsService
{
    /// <summary>
    /// Service info (Simple
    /// </summary>
    internal class ServiceSummary
    {
        #region Public parameter

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Status { get; set; }
        public string StartupType { get; set; }

        #endregion

        const string _log_target = "ServiceSummary";

        public ServiceSummary(string serviceName)
            : this(ServiceController.GetServices().
                  FirstOrDefault(x =>
                      x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                      x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase)))
        { }

        public ServiceSummary(ServiceController sc)
        {
            this.Name = sc.ServiceName;
            this.DisplayName = sc.DisplayName;
            this.Status = sc.Status switch
            {
                ServiceControllerStatus.Running => "実行中",
                ServiceControllerStatus.Stopped => "停止",
                ServiceControllerStatus.Paused => "一時中断",
                ServiceControllerStatus.StartPending => "開始中",
                ServiceControllerStatus.StopPending => "停止中",
                ServiceControllerStatus.PausePending => "一時中断保留中",
                ServiceControllerStatus.ContinuePending => "継続保留中",
                _ => "不明"
            };
            this.StartupType = sc.StartType switch
            {
                ServiceStartMode.Automatic => "自動",
                ServiceStartMode.Manual => "手動",
                ServiceStartMode.Disabled => "無効",
                _ => "不明"
            };

            var delay = StartupChecker.IsDelayedAutoStart(sc);
            var trigger = StartupChecker.IsTriggeredStart(sc);
            if (delay || trigger)
            {
                List<string> list = new();
                if (delay) list.Add("遅延自動");
                if (trigger) list.Add("トリガー開始");
                this.StartupType += " (" + string.Join(",", list) + ")";
            }
        }

        public static ServiceSummary[] Load(string serviceName = null)
        {
            var services = serviceName == null ?
                ServiceController.GetServices() :
                ServiceController.GetServices().
                    Where(x =>
                        x.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) ||
                        x.DisplayName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            return services.Select(x => new ServiceSummary(x)).ToArray();
        }
    }
}
