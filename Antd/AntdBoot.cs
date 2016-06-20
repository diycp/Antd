﻿using System.IO;
using System.Linq;
using System.Threading;
using antdlib;
using antdlib.Apps;
using antdlib.common;
using antdlib.common.Helpers;
using Antd.Database;
using Antd.MountPoint;
using Antd.Scheduler;
using Antd.Users;

namespace Antd {
    public class AntdBoot {

        public static void CheckOsIsRw() {
            Terminal.Execute($"{Parameter.Aossvc} reporemountrw");
        }

        public static void SetWorkingDirectories() {
            if (!Parameter.IsUnix)
                return;
            Mount.WorkingDirectories();
            ConsoleLogger.Log("working directories ready");
        }

        public static void SetCoreParameters() {
            ApplicationSetting.WriteDefaults();
            ConsoleLogger.Log("antd core parameters ready");
        }

        public static void CheckCertificate() {
            var certificate = ApplicationSetting.CertificatePath();
            if (!File.Exists(certificate)) {
                File.Copy($"{Parameter.Resources}/certificate.pfx", certificate, true);
            }
            ConsoleLogger.Log("certificates ready");
        }

        public static void ReloadUsers() {
            if (!Parameter.IsUnix)
                return;
            SystemUser.Config.ResetPasswordForUserStoredInDb();
            ConsoleLogger.Log("users config ready");
        }

        public static void ReloadSsh() {
            if (!Parameter.IsUnix)
                return;
            Terminal.Execute("mkdir -p /root/.ssh");
            if (!File.Exists(Parameter.AuthKeys)) {
                Terminal.Execute($"touch {Parameter.AuthKeys}");
            }
            const string dir = "/etc/ssh";
            var mntDir = Mounts.SetDirsPath(dir);
            if (!Directory.Exists(mntDir)) {
                Terminal.Execute($"cp -fR {dir} {mntDir}");
            }
            Mounts.Umount(dir);
            Mount.Dir(dir);
            Terminal.Execute("ssh-keygen -A");
            Terminal.Execute("systemctl restart sshd.service");
            ConsoleLogger.Log("ssh config ready");
            //SshConfig.Keys.PropagateKeys(new[] { "" }, new[] { "" });
        }

        public static void SetOverlayDirectories() {
            if (!Parameter.IsUnix)
                return;
            Mount.OverlayDirectories();
            ConsoleLogger.Log("overlay ready");
        }

        public static void SetMounts() {
            if (!Parameter.IsUnix)
                return;
            Mount.AllDirectories();
            ConsoleLogger.Log("mounts ready");
        }

        public static void SetOsMount() {
            if (!Parameter.IsUnix)
                return;
            if (Mounts.IsAlreadyMounted("/mnt/cdrom/Kernel/active-firmware", "/lib64/firmware") == false) {
                Terminal.Execute($"mount {"/mnt/cdrom/Kernel/active-firmware"} {"/lib64/firmware"}");
            }
            const string module = "/mnt/cdrom/Kernel/active-modules";
            var kernelRelease = Terminal.Execute("uname -r").Trim();
            var linkedRelease = Terminal.Execute($"file {module}").Trim();
            if (Mounts.IsAlreadyMounted(module) == false && linkedRelease.Contains(kernelRelease)) {
                var moduleDir = $"/lib64/modules/{kernelRelease}/";
                ConsoleLogger.Log($"Creating {moduleDir} to mount OS-modules");
                Directory.CreateDirectory(moduleDir);
                Terminal.Execute($"mount {module} {moduleDir}");
            }
            Terminal.Execute("systemctl restart systemd-modules-load.service");
            ConsoleLogger.Log("os mounts ready");
        }

        public static void LaunchDefaultOsConfiguration() {
            if (!Parameter.IsUnix)
                return;
            ConsoleLogger.Log("default os configuration ready");
        }

        public static void SetWebsocketd() {
            if (!Parameter.IsUnix)
                return;
            var filePath = $"{Parameter.AntdCfg}/websocketd";
            if (File.Exists(filePath))
                return;
            File.Copy($"{Parameter.Resources}/websocketd", filePath);
            Terminal.Execute($"chmod 777 {filePath}");
            ConsoleLogger.Log("websocketd ready");
        }

        public static void SetSystemdJournald() {
            if (!Parameter.IsUnix)
                return;
            var file = $"{Parameter.RepoDirs}/{"FILE_etc_systemd_journald.conf"}";
            if (File.Exists(file)) {
                return;
            }
            File.Copy($"{Parameter.Resources}/FILE_etc_systemd_journald.conf", file);
            var realFileName = Mounts.GetFilesPath("FILE_etc_systemd_journald.conf");
            if (Mounts.IsAlreadyMounted(file, realFileName) == false) {
                Mount.File(realFileName);
            }
            Terminal.Execute("systemctl restart systemd-journald.service");
            ConsoleLogger.Log("journald config ready");
        }

        public static void CheckResolv() {
            if (!Parameter.IsUnix)
                return;
            if (File.Exists("/etc/resolv.conf"))
                return;
            Terminal.Execute("touch /etc/resolv.conf");
            ConsoleLogger.Log("resolv ready");
        }

        public static void SetFirewall() {
            if (!Parameter.IsUnix)
                return;
            ConsoleLogger.Log("firewall ready");
        }

        public static void ImportSystemInformation() {
            if (!Parameter.IsUnix)
                return;
            if (!new NetworkInterfaceRepository().GetAll().Any()) {
                new NetworkInterfaceRepository().Import();
            }
            ConsoleLogger.Log("network interfaces imported");
        }

        public static void StartScheduler(bool loadFromDatabase) {
            JobScheduler.Start(loadFromDatabase);
            ConsoleLogger.Log("scheduler ready");
        }

        public static void StartDirectoryWatcher() {
            new DirectoryWatcher().StartWatching();
            ConsoleLogger.Log("directory watcher ready");
        }

        public static void LaunchApps() {
            if (!Parameter.IsUnix)
                return;
            var apps = Management.DetectApps();
            if (apps.Length > 0) {
                foreach (
                    var dir in
                        from app in apps
                        select Management.GetWantedDirectories(app)
                        into dirs
                        where dirs.Length > 0
                        from dir in dirs
                        select dir) {
                    Mount.Dir(dir);
                }
            }
            Thread.Sleep(10);
            AnthillaSp.SetApp();
            ConsoleLogger.Log("apps ready");
        }

        public static void DownloadDefaultRepoFiles() {
            if (!Parameter.IsUnix)
                return;
            var dir = $"{Parameter.RepoConfig}/database";
            Directory.CreateDirectory(dir);
            FileSystem.Download("http://www.internic.net/domain/named.root", $"{dir}/named.root");
            FileSystem.Download("http://www.internic.net/domain/root.zone", $"{dir}/root.zone");
            FileSystem.Download("http://standards-oui.ieee.org/oui.txt", $"{dir}/oui.txt");
        }

        public static void LoadCollectd() {
            var file = $"{Parameter.RepoDirs}/{"FILE_etc_collectd.conf"}";
            File.Copy($"{Parameter.Resources}/FILE_etc_collectd.conf", file);
            var realFileName = Mounts.GetFilesPath("FILE_etc_collectd.conf");
            if (Mounts.IsAlreadyMounted(file, realFileName) == false) {
                Mount.File(realFileName);
            }
            Terminal.Execute("systemctl restart collectd.service");
        }

        public static void LoadWpaSupplicant() {
            var file = $"{Parameter.RepoDirs}/{"FILE_etc_wpa_supplicant_wpa_suplicant.conf"}";
            File.Copy($"{Parameter.Resources}/FILE_etc_wpa_supplicant_wpa_suplicant.conf", file);
            var realFileName = Mounts.GetFilesPath("FILE_etc_wpa_supplicant_wpa__suplicant.conf");
            if (Mounts.IsAlreadyMounted(file, realFileName) == false) {
                Mount.File(realFileName);
            }
            Terminal.Execute("systemctl restart wpa_supplicant.service");
        }

        //public static void StartWebsocketServer() {
        //    var port = PortManagement.GetFirstAvailable(1234);
        //    ApplicationSetting.SetWebsocketPort(port.ToString());
        //    WebSocket.Start(Convert.ToInt32(ApplicationSetting.WebsocketPort()));
        //}
    }
}