﻿using antdlib.models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using anthilla.core;
using IoDir = System.IO.Directory;
using Parameter = antdlib.common.Parameter;

namespace antdlib.config {
    public class AuthorizedKeysConfiguration {

        private readonly AuthorizedKeysConfigurationModel _serviceModel;

        private readonly string _cfgFile = $"{Parameter.AntdCfgServices}/authorizedkeys.conf";

        public AuthorizedKeysConfiguration() {
            IoDir.CreateDirectory(Parameter.AntdCfgServices);
            if(!File.Exists(_cfgFile)) {
                _serviceModel = new AuthorizedKeysConfigurationModel();
            }
            else {
                try {
                    var text = File.ReadAllText(_cfgFile);
                    var obj = JsonConvert.DeserializeObject<AuthorizedKeysConfigurationModel>(text);
                    _serviceModel = obj;
                }
                catch(Exception) {
                    _serviceModel = new AuthorizedKeysConfigurationModel();
                }

            }
        }

        public void Save(AuthorizedKeysConfigurationModel model) {
            var text = JsonConvert.SerializeObject(model, Formatting.Indented);
            FileWithAcl.WriteAllText(_cfgFile, text, "644", "root", "wheel");
            ConsoleLogger.Log("[authorizedkeys] configuration saved");
        }

        public void Set() {
            Enable();
        }

        public bool IsActive() {
            if(!File.Exists(_cfgFile)) {
                return false;
            }
            return _serviceModel != null && _serviceModel.IsActive;
        }

        public AuthorizedKeysConfigurationModel Get() {
            return _serviceModel;
        }

        public void Enable() {
            _serviceModel.IsActive = true;
            Save(_serviceModel);
            ConsoleLogger.Log("[authorizedkeys] enabled");
        }

        public void Disable() {
            _serviceModel.IsActive = false;
            Save(_serviceModel);
            ConsoleLogger.Log("[authorizedkeys] disabled");
        }

        public void AddKey(AuthorizedKeyModel model) {
            var zones = _serviceModel.Keys;
            zones.Add(model);
            _serviceModel.Keys = zones;
            Save(_serviceModel);
        }

        public void RemoveKey(string guid) {
            var zones = _serviceModel.Keys;
            var model = zones.First(_ => _.Guid == guid);
            if(model == null) {
                return;
            }
            zones.Remove(model);
            _serviceModel.Keys = zones;
            Save(_serviceModel);
        }
    }
}
