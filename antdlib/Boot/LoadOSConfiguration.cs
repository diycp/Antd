﻿//-------------------------------------------------------------------------------------
//     Copyright (c) 2014, Anthilla S.r.l. (http://www.anthilla.com)
//     All rights reserved.
//
//     Redistribution and use in source and binary forms, with or without
//     modification, are permitted provided that the following conditions are met:
//         * Redistributions of source code must retain the above copyright
//           notice, this list of conditions and the following disclaimer.
//         * Redistributions in binary form must reproduce the above copyright
//           notice, this list of conditions and the following disclaimer in the
//           documentation and/or other materials provided with the distribution.
//         * Neither the name of the Anthilla S.r.l. nor the
//           names of its contributors may be used to endorse or promote products
//           derived from this software without specific prior written permission.
//
//     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//     ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//     DISCLAIMED. IN NO EVENT SHALL ANTHILLA S.R.L. BE LIABLE FOR ANY
//     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//     20141110
//-------------------------------------------------------------------------------------

using antdlib.MountPoint;
using System.IO;
using antdlib.Log;

namespace antdlib.Boot {
    public class LoadOsConfiguration {
        public static void LoadCollectd() {
            var file = $"{Folder.RepoDirs}/{"FILE_etc_collectd.conf"}";
            File.Copy($"{Folder.Resources}/FILE_etc_collectd.conf", file);
            var realFileName = Mount.GetFilesPath("FILE_etc_collectd.conf");
            if (Mount.IsAlreadyMounted(file, realFileName) == false) {
                Mount.File(realFileName);
            }
            Terminal.Terminal.Execute("systemctl restart collectd.service");
        }

        public static void LoadSystemdJournald() {
            var file = $"{Folder.RepoDirs}/{"FILE_etc_systemd_journald.conf"}";
            File.Copy($"{Folder.Resources}/FILE_etc_systemd_journald.conf", file);
            var realFileName = Mount.GetFilesPath("FILE_etc_systemd_journald.conf");
            if (Mount.IsAlreadyMounted(file, realFileName) == false) {
                Mount.File(realFileName);
            }
            Terminal.Terminal.Execute("systemctl restart systemd-journald.service");
        }

        public static void LoadWpaSupplicant() {
            var file = $"{Folder.RepoDirs}/{"FILE_etc_wpa_supplicant_wpa_suplicant.conf"}";
            File.Copy($"{Folder.Resources}/FILE_etc_wpa_supplicant_wpa_suplicant.conf", file);
            var realFileName = Mount.GetFilesPath("FILE_etc_wpa_supplicant_wpa__suplicant.conf");
            if (Mount.IsAlreadyMounted(file, realFileName) == false) {
                Mount.File(realFileName);
            }
            Terminal.Terminal.Execute("systemctl restart wpa_supplicant.service");
        }

        public static void LoadFirewall() {
            File.Copy($"{Folder.Resources}/antd.boot.firewall.conf", $"{Folder.RepoDirs}/{"antd.boot.firewall.conf"}");
        }

        public static void LoadWebsocketd() {
            var filePath = $"{Folder.AntdCfg}/websocketd";
            if (File.Exists(filePath)) return;
            ConsoleLogger.Log("Downloading websocketd");
            File.Copy($"{Folder.Resources}/websocketd", filePath);
            Terminal.Terminal.Execute($"chmod 777 {filePath}");
        }
    }
}
