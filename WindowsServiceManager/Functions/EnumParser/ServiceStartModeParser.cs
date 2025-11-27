using System.ServiceProcess;
using WindowsService.Functions;
using WindowsService.Functions.EnumParser;

namespace WindowsServiceManager.Functions.EnumParser
{
    internal class ServiceStartModeParser : ParserBase<ServiceStartMode>
    {
        public ServiceStartModeParser()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            map = new()
            {
                { new string[] { "Automatic", "Auto" }, ServiceStartMode.Automatic },
                { new string[] { "Manual", "Manualy", "Man" }, ServiceStartMode.Manual },
                { new string[] { "Disabled", "Disable", "Dis" }, ServiceStartMode.Disabled },
            };
        }

        #region Static methods.

        private static ServiceStartModeParser _parser = null;

        public static ServiceStartMode ParamsToRaw(string text)
        {
            _parser ??= new ServiceStartModeParser();
            return _parser.TextToFlags(text);
        }

        public static string RawToParams(ServiceStartMode flags)
        {
            _parser ??= new ServiceStartModeParser();
            return _parser.FlagsToString(flags);
        }

        #endregion







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

        /*
        public static ServiceStartMode StringToStartupType(string text)
        {
            if (_mapStartupType == null) InitializeStartupType();
            return TextFunctions.StringToFlags<ServiceStartMode>(text, _mapStartupType);
        }
        */

        /*
        public static string StartupTypeToString(ServiceStartMode val)
        {
            if (_mapStartupType == null) InitializeStartupType();
            return TextFunctions.FlagsToString<ServiceStartMode>(val, _mapStartupType);
        }
        */
        /*
        public static string GetStartupTypeString(string text)
        {
            if (_mapStartupType == null) InitializeStartupType();
            return TextFunctions.GetCorrect<ServiceStartMode>(text, _mapStartupType);
        }
        */
    }
}
