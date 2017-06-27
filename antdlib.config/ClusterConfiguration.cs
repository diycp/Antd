﻿using antdlib.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using anthilla.core;
using Parameter = antdlib.common.Parameter;

namespace antdlib.config {
    public static class ClusterConfiguration {

        private static readonly string CfgFile = $"{Parameter.AntdCfgCluster}/cluster.conf";

        public static void Prepare() {
            var dir = "/var/lib/haproxy";
            Directory.CreateDirectory(dir);
            Bash.Execute($"chown haproxy:haproxy {dir}");
            Bash.Execute($"chmod 755 {dir}");
            //net.ipv4.ip_nonlocal_bind=1
            if(File.Exists("/proc/sys/net/ipv4/ip_nonlocal_bind")) {
                File.WriteAllText("/proc/sys/net/ipv4/ip_nonlocal_bind", "1");
            }
        }

        private static List<Cluster.Node> Load() {
            if(!File.Exists(CfgFile)) {
                return new List<Cluster.Node>();
            }
            try {
                var text = File.ReadAllText(CfgFile);
                var obj = JsonConvert.DeserializeObject<List<Cluster.Node>>(text);
                return obj;
            }
            catch(Exception) {
                return new List<Cluster.Node>();
            }
        }

        public static void Save(List<Cluster.Node> model) {
            Prepare();
            var text = JsonConvert.SerializeObject(model, Formatting.Indented);
            FileWithAcl.WriteAllText(CfgFile, text, "644", "root", "wheel");
            ConsoleLogger.Log("[cluster] configuration saved");
        }

        private static readonly string IpFile = $"{Parameter.AntdCfgCluster}/cluster-info.conf";

        public static void SaveClusterInfo(Cluster.Configuration model) {
            Prepare();
            var text = JsonConvert.SerializeObject(model, Formatting.Indented);
            FileWithAcl.WriteAllText(IpFile, text, "644", "root", "wheel");
            ConsoleLogger.Log("[cluster] configuration saved");
        }

        public static Cluster.Configuration GetClusterInfo() {
            if(!File.Exists(IpFile)) {
                return new Cluster.Configuration();
            }
            try {
                var text = File.ReadAllText(IpFile);
                var obj = JsonConvert.DeserializeObject<Cluster.Configuration>(text);
                return obj;
            }
            catch(Exception) {
                return new Cluster.Configuration();
            }
        }

        public static List<Cluster.Node> Get() {
            return Load();
        }
    }
}
