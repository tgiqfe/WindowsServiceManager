using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsService.Functions
{
    internal class RegistryHelper
    {
        #region load/unload hive Registry parameters

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        const uint HKEY_USERS = 0x80000003;

        #endregion

        public static RegistryKey GetRegistryKey(string path, bool isCreate = false, bool writable = false)
        {
            string rootPath = path.Substring(0, path.IndexOf("\\"));
            string keyPath = path.Substring(path.IndexOf("\\") + 1);

            RegistryKey rootKey = rootPath.ToLower() switch
            {
                "hkcr" or "hkcr:" or "hkey_classes_root" => Registry.ClassesRoot,
                "hkcu" or "hkcu:" or "hkey_current_user" => Registry.CurrentUser,
                "hklm" or "hklm:" or "hkey_local_machine" => Registry.LocalMachine,
                "hku" or "hku:" or "hkey_users" => Registry.Users,
                "hkcc" or "hkcc:" or "hkey_current_config" => Registry.CurrentConfig,
                _ => null
            };
            if (rootKey == null) return null;

            return isCreate ?
                rootKey.CreateSubKey(keyPath, writable) :
                rootKey.OpenSubKey(keyPath, writable);
        }

        /// <summary>
        /// writableとcreateを逆に。最終的にはこちらを採用する予定。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="writable"></param>
        /// <param name="isCreate"></param>
        /// <returns></returns>
        public static RegistryKey GetRegistryKey2(string path, bool writable = false, bool isCreate = false)
        {
            string rootPath = path.Substring(0, path.IndexOf("\\"));
            string keyPath = path.Substring(path.IndexOf("\\") + 1);

            RegistryKey rootKey = rootPath.ToLower() switch
            {
                "hkcr" or "hkcr:" or "hkey_classes_root" => Registry.ClassesRoot,
                "hkcu" or "hkcu:" or "hkey_current_user" => Registry.CurrentUser,
                "hklm" or "hklm:" or "hkey_local_machine" => Registry.LocalMachine,
                "hku" or "hku:" or "hkey_users" => Registry.Users,
                "hkcc" or "hkcc:" or "hkey_current_config" => Registry.CurrentConfig,
                _ => null
            };
            if (rootKey == null) return null;

            return isCreate ?
                rootKey.CreateSubKey(keyPath, writable) :
                rootKey.OpenSubKey(keyPath, writable);
        }

        public static void Load(string keyName, string hiveFile)
        {
            ProcessPrivilege.AdjustToken(Privilege.SeRestorePrivilege);
            ProcessPrivilege.AdjustToken(Privilege.SeBackupPrivilege);
            RegLoadKey(HKEY_USERS, keyName, hiveFile);
        }

        public static void Unload(string keyName)
        {
            ProcessPrivilege.AdjustToken(Privilege.SeRestorePrivilege);
            ProcessPrivilege.AdjustToken(Privilege.SeBackupPrivilege);
            RegUnLoadKey(HKEY_USERS, keyName);
        }
    }
}
