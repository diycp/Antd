﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using antdlib;
using antdlib.common;
using antdlib.common.Helpers;
using antdlib.common.Tool;
using antdlib.Systemd;
using antdlib.views;
using Antd.Apps;
using Antd.Avahi;
using Antd.Bind;
using Antd.Configuration;
using Antd.Database;
using Antd.Dhcpd;
using Antd.Firewall;
using Antd.Gluster;
using Antd.MountPoint;
using Antd.Samba;
using Antd.Storage;
using Antd.SystemdTimer;
using Antd.Timer;
using Antd.Users;
using Newtonsoft.Json;
using RaptorDB;

namespace Antd {
    public class AntdBoot {

        private readonly Bash _bash = new Bash();

        public void RemoveLimits() {
            if(!Parameter.IsUnix)
                return;
            const string limitsFile = "/etc/security/limits.conf";
            if(File.Exists(limitsFile)) {
                var t = File.ReadAllText(limitsFile);
                if(!t.Contains("root - nofile 1024000")) {
                    File.AppendAllLines(limitsFile, new[] { "root - nofile 1024000" });
                }
            }
            _bash.Execute("ulimit -n 1024000", false);
        }

        public void StartOverlayWatcher() {
            new OverlayWatcher().StartWatching();
            ConsoleLogger.Log("overlay watcher ready");
        }

        public void CheckOsIsRw() {
            _bash.Execute($"{Parameter.Aossvc} reporemountrw", false);
        }

        private readonly Mount _mount = new Mount();

        public void SetWorkingDirectories() {
            if(!Parameter.IsUnix)
                return;
            _mount.WorkingDirectories();
            ConsoleLogger.Log("working directories ready");
        }

        private readonly ApplicationSetting _applicationSetting = new ApplicationSetting();

        public void SetCoreParameters() {
            _applicationSetting.WriteDefaults();
            ConsoleLogger.Log("antd core parameters ready");
        }

        public RaptorDB.RaptorDB StartDatabase() {
            var path = _applicationSetting.DatabasePath();
            var database = RaptorDB.RaptorDB.Open(path);
            Global.RequirePrimaryView = false;
            Global.BackupCronSchedule = null;
            database.RegisterView(new ApplicationView());
            database.RegisterView(new AuthorizedKeysView());
            database.RegisterView(new BootModuleLoadView());
            database.RegisterView(new BootServiceLoadView());
            database.RegisterView(new BootOsParametersLoadView());
            database.RegisterView(new FirewallListView());
            database.RegisterView(new NftView());
            database.RegisterView(new TimerView());
            database.RegisterView(new MountView());
            database.RegisterView(new RsyncView());
            database.RegisterView(new UserClaimView());
            database.RegisterView(new UserView());
            database.RegisterView(new MacAddressView());
            database.RegisterView(new SyslogView());

            ConsoleLogger.Log("database ready");
            return database;
        }

        private readonly MachineConfiguration _machineConfiguration = new MachineConfiguration();

        public void PrepareConfiguration() {
            _machineConfiguration.Set();

            try {
                var conf = $"{Parameter.AntdCfg}/machine.conf";
                if(!File.Exists(conf)) {
                    var machineDefault = new MachineModel();
                    File.WriteAllText(conf, JsonConvert.SerializeObject(machineDefault, Formatting.Indented));
                }
            }
            catch(Exception ex) {
                ConsoleLogger.Log(ex);
            }

            ConsoleLogger.Log("configuration prepared");
        }

        public void SetOsMount() {
            if(!Parameter.IsUnix)
                return;
            if(Mounts.IsAlreadyMounted("/mnt/cdrom/Kernel/active-firmware", "/lib64/firmware") == false) {
                _bash.Execute("mount /mnt/cdrom/Kernel/active-firmware /lib64/firmware", false);
            }
            const string module = "/mnt/cdrom/Kernel/active-modules";
            var kernelRelease = _bash.Execute("uname -r").Trim();
            var linkedRelease = _bash.Execute($"file {module}").Trim();
            if(Mounts.IsAlreadyMounted(module) == false && linkedRelease.Contains(kernelRelease)) {
                var moduleDir = $"/lib64/modules/{kernelRelease}/";
                Directory.CreateDirectory(moduleDir);
                _bash.Execute($"mount {module} {moduleDir}", false);
            }
            _bash.Execute("systemctl restart systemd-modules-load.service", false);
            ConsoleLogger.Log("os mounts ready");
        }

        public void SetOsParametersLocal() {
            if(!Parameter.IsUnix)
                return;
            var kvps = new BootOsParametersLoadRepository().Retrieve();
            if(kvps != null) {
                foreach(var kvp in kvps) {
                    var file = kvp.Key;
                    var value = kvp.Value;
                    if(!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(value)) {
                        File.WriteAllText(file, value);
                    }
                }
            }
            ConsoleLogger.Log("os local parameters ready");
        }

        public void LoadModules() {
            if(!Parameter.IsUnix)
                return;
            var modules = new BootModuleLoadRepository().Retrieve();
            if(modules != null) {
                foreach(var module in modules) {
                    _bash.Execute($"modprobe {module}", false);
                }
            }
            ConsoleLogger.Log("modules ready");
        }

        public void SetMounts() {
            if(!Parameter.IsUnix)
                return;
            _mount.AllDirectories();
            ConsoleLogger.Log("mounts ready");
        }

        public void ImportCommands() {
            if(!Parameter.IsUnix)
                return;
            var storedconf = $"{Parameter.RootFrameworkAntdShellScript}/var/kerbynet.conf";
            File.Copy(storedconf, "/etc/kerbynet.conf", true);
            ConsoleLogger.Log("commands and scripts configuration imported");
        }

        private readonly UserRepository _userRepository = new UserRepository();
        private readonly SystemUser _systemUser = new SystemUser();

        public void ReloadUsers() {
            if(!Parameter.IsUnix)
                return;
            var sysUser = _userRepository.Import();
            foreach(var user in _userRepository.GetAll()) {
                if(!sysUser.ContainsKey(user.Alias)) {
                    _systemUser.Create(user.Alias);
                }
                if(!string.IsNullOrEmpty(user.Password)) {
                    _systemUser.SetPassword(user.Alias, user.Password);
                }
            }
            ConsoleLogger.Log("users config ready");
        }

        private readonly SetupConfiguration _setupConfiguration = new SetupConfiguration();

        public void CommandExecuteLocal() {
            if(!Parameter.IsUnix)
                return;
            _setupConfiguration.Set();
            ConsoleLogger.Log("machine configured");
        }

        public void Ssh() {
            if(!Parameter.IsUnix) {
                return;
            }
            var storedKeyRepo = new AuthorizedKeysRepository();
            var storedKeys = storedKeyRepo.GetAll();
            foreach(var storedKey in storedKeys) {
                var home = storedKey.User == "root" ? "/root/.ssh" : $"/home/{storedKey.User}/.ssh";
                var authorizedKeysPath = $"{home}/authorized_keys";
                if(!File.Exists(authorizedKeysPath)) {
                    File.Create(authorizedKeysPath);
                }
                var line = $"{storedKey.KeyValue} {storedKey.RemoteUser}";
                File.AppendAllLines(authorizedKeysPath, new List<string> { line });
                _bash.Execute($"chmod 600 {authorizedKeysPath}");
                _bash.Execute($"chown {storedKey.User}:{storedKey.User} {authorizedKeysPath}");
            }
        }

        private readonly NfTables _nfTables = new NfTables();

        public void SetFirewall() {
            if(!Parameter.IsUnix)
                return;
            _nfTables.Setup();
            _nfTables.ReloadConfiguration();
            _nfTables.Import();
            _nfTables.Export();
            _nfTables.ReloadConfiguration();
            ConsoleLogger.Log("firewall ready");
        }

        public void LoadServices() {
            if(!Parameter.IsUnix)
                return;
            var services = new BootServiceLoadRepository().Retrieve();
            if(services != null) {
                foreach(var service in services) {
                    if(Systemctl.IsActive(service) == false) {
                        Systemctl.Restart(service);
                    }
                }
            }
            ConsoleLogger.Log("services ready");
        }

        public void StartDhcpd() {
            if(!Parameter.IsUnix)
                return;
            var dhcpdConfiguration = new DhcpdConfiguration();
            if(dhcpdConfiguration.IsActive()) {
                dhcpdConfiguration.Set();
                dhcpdConfiguration.Enable();
                dhcpdConfiguration.Restart();
                ConsoleLogger.Log("dhcp server start");
            }
        }

        public void StartBind() {
            if(!Parameter.IsUnix)
                return;
            var bindConfiguration = new BindConfiguration();
            if(bindConfiguration.IsActive()) {
                bindConfiguration.Set();
                bindConfiguration.Enable();
                bindConfiguration.Restart();
                ConsoleLogger.Log("bind server start");
            }
        }

        public void StartSamba() {
            if(!Parameter.IsUnix)
                return;
            var sambaConfiguration = new SambaConfiguration();
            if(sambaConfiguration.IsActive()) {
                sambaConfiguration.Set();
                sambaConfiguration.Enable();
                sambaConfiguration.Restart();
                ConsoleLogger.Log("samba server start");
            }
        }

        private readonly SyslogConfiguration _syslogConfiguration = new SyslogConfiguration();

        public void SetSyslogNg() {
            if(!Parameter.IsUnix)
                return;
            var s = _syslogConfiguration.Set();
            if(s) {
                ConsoleLogger.Log("syslog ready");
            }
        }

        public void InitAvahi() {
            if(!Parameter.IsUnix)
                return;
            const string avahiServicePath = "/etc/avahi/services/antd.service";
            var xml = AvahiCustomXml.Generate(_applicationSetting.HttpPort());
            if(File.Exists(avahiServicePath)) {
                File.Delete(avahiServicePath);
            }
            File.WriteAllLines(avahiServicePath, xml);
            _bash.Execute("chmod 755 /etc/avahi/services", false);
            _bash.Execute($"chmod 644 {avahiServicePath}", false);
            Systemctl.Restart("avahi-daemon.service");
            Systemctl.Restart("avahi-daemon.socket");
            ConsoleLogger.Log("avahi ready");
        }

        private readonly Zpool _zpool = new Zpool();

        public void ImportPools() {
            if(!Parameter.IsUnix)
                return;
            var pools = _zpool.ImportList().ToList();
            foreach(var pool in pools) {
                if(!string.IsNullOrEmpty(pool)) {
                    ConsoleLogger.Log($"pool {pool} imported");
                    _zpool.Import(pool);
                }
            }
            if(pools.Count > 0) {
                ConsoleLogger.Log("pools imported");
            }
        }

        private readonly Timers _timers = new Timers();

        public void StartScheduler() {
            if(!Parameter.IsUnix)
                return;
            _timers.CleanUp();
            _timers.Setup();
            _timers.Import();
            _timers.Export();

            var pools = _zpool.List();
            foreach(var zp in pools) {

                _timers.Create(zp.Name.ToLower() + "snap", "hourly", $"/sbin/zfs snap -r {zp.Name}@${{TTDATE}}");
            }

            _timers.StartAll();
            ConsoleLogger.Log("scheduler ready");
        }

        private readonly GlusterConfiguration _glusterConfiguration = new GlusterConfiguration();

        public void InitGlusterfs() {
            if(!Parameter.IsUnix)
                return;
            _glusterConfiguration.Set();
            if(_glusterConfiguration.IsConfigured) {
                _glusterConfiguration.Start();
                _glusterConfiguration.Launch();
                ConsoleLogger.Log("glusterfs ready");
            }
        }

        public void StartDirectoryWatcher() {
            new DirectoryWatcher().StartWatching();
            ConsoleLogger.Log("directory watcher ready");
        }

        public void LaunchInternalTimers() {
            if(!Parameter.IsUnix)
                return;
            new SnapshotCleanup().Start(new TimeSpan(2, 00, 00));
            ConsoleLogger.Log("internal timers ready");
        }

        private readonly ApplicationRepository _applicationRepository = new ApplicationRepository();
        private readonly AppTarget _appTarget = new AppTarget();
        public void LaunchApps() {
            if(!Parameter.IsUnix)
                return;
            _appTarget.Setup();
            var apps = _applicationRepository.GetAll().Select(_ => new ApplicationModel(_)).ToList();
            foreach(var app in apps) {
                var units = app.UnitLauncher;
                foreach(var unit in units) {
                    if(Systemctl.IsActive(unit) == false) {
                        Systemctl.Restart(unit);
                    }
                }
            }
            //AppTarget.StartAll();
            ConsoleLogger.Log("apps ready");
        }

        #region Unused Configuration
        public void SetWebsocketd() {
            if(!Parameter.IsUnix)
                return;
            var filePath = $"{Parameter.AntdCfg}/websocketd";
            if(File.Exists(filePath))
                return;
            File.Copy($"{Parameter.Resources}/websocketd", filePath);
            _bash.Execute($"chmod 777 {filePath}", false);
            ConsoleLogger.Log("websocketd ready");
        }

        public void SetSystemdJournald() {
            if(!Parameter.IsUnix)
                return;
            var file = $"{Parameter.RepoDirs}/FILE_etc_systemd_journald.conf";
            if(File.Exists(file)) {
                return;
            }
            File.Copy($"{Parameter.Resources}/FILE_etc_systemd_journald.conf", file);
            var realFileName = Mounts.GetFilesPath("FILE_etc_systemd_journald.conf");
            if(Mounts.IsAlreadyMounted(file, realFileName) == false) {
                _mount.File(realFileName);
            }
            _bash.Execute("systemctl restart systemd-journald.service", false);
            ConsoleLogger.Log("journald config ready");
        }

        public void LoadCollectd() {
            var file = $"{Parameter.RepoDirs}/FILE_etc_collectd.conf";
            File.Copy($"{Parameter.Resources}/FILE_etc_collectd.conf", file);
            var realFileName = Mounts.GetFilesPath("FILE_etc_collectd.conf");
            if(Mounts.IsAlreadyMounted(file, realFileName) == false) {
                _mount.File(realFileName);
            }
            _bash.Execute("systemctl restart collectd.service", false);
        }

        public void LoadWpaSupplicant() {
            var file = $"{Parameter.RepoDirs}/FILE_etc_wpa_supplicant_wpa_suplicant.conf";
            File.Copy($"{Parameter.Resources}/FILE_etc_wpa_supplicant_wpa_suplicant.conf", file);
            var realFileName = Mounts.GetFilesPath("FILE_etc_wpa_supplicant_wpa__suplicant.conf");
            if(Mounts.IsAlreadyMounted(file, realFileName) == false) {
                _mount.File(realFileName);
            }
            _bash.Execute("systemctl restart wpa_supplicant.service", false);
        }
        #endregion
    }
}