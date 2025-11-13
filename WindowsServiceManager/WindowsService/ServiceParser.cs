using System.ServiceProcess;

namespace WindowsServiceManager.WindowsService
{
    internal class ServiceParser
    {
        private static Dictionary<string[], ServiceStartMode> _mapStartupType = null;
        private static void InitializeStartupType()
        {
            _mapStartupType = new()
            {
                { new string[] { "Automatic", "Auto" }, ServiceStartMode.Automatic },
                { new string[] { "Manual", "Manualy", "Man" }, ServiceStartMode.Manual },
                { new string[] { "Disabled", "Disable", "Dis" }, ServiceStartMode.Disabled },
            };
        }
        public static ServiceStartMode StringToStartupType(string text)
        {
            if (_mapStartupType == null) InitializeStartupType();
            foreach (var kvp in _mapStartupType)
            {
                if (kvp.Key.Any(x => string.Equals(x, text, StringComparison.OrdinalIgnoreCase)))
                {
                    return kvp.Value;
                }
            }
            throw new ArgumentException($"Invalid startup type string: {text}");
        }
        public static string StartupTypeToString(ServiceStartMode val)
        {
            if (_mapStartupType == null) InitializeStartupType();
            foreach (var kvp in _mapStartupType)
            {
                if (kvp.Value == val)
                {
                    return kvp.Key[0];
                }
            }
            return "Unknown";
        }
    }
}
