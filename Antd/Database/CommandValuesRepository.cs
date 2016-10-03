﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using antdlib.common;
using antdlib.views;
using antdlib.views.Repo;
using Newtonsoft.Json;

namespace Antd.Database {
    public class CommandValuesRepository {

        public class RootObject {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private const string ViewName = "CommandValues";

        public IEnumerable<CommandValuesSchema> GetAll() {
            var result = DatabaseRepository.Query<CommandValuesSchema>(AntdApplication.Database, ViewName);
            return result;
        }

        public CommandValuesSchema GetByGuid(string guid) {
            var result = DatabaseRepository.Query<CommandValuesSchema>(AntdApplication.Database, ViewName, schema => schema.Id == guid || schema.Guid == guid);
            return result.FirstOrDefault();
        }

        public CommandValuesSchema GetByName(string name) {
            var result = DatabaseRepository.Query<CommandValuesSchema>(AntdApplication.Database, ViewName, schema => schema.Name == name);
            return result.FirstOrDefault();
        }

        public void Import() {
            Directory.CreateDirectory(Parameter.AntdCfgCommands);
            var path = $"{Parameter.AntdCfgCommands}/values.json";
            var resourceFile = $"{Parameter.RootFrameworkAntdResources}/values.json";
            if (!File.Exists(path)) {
                if (!File.Exists(resourceFile)) {
                    return;
                }
                File.Copy(resourceFile, path, true);
            }
            else {
                var existingFileHash = FileSystem.GetFileHash(path);
                var storedFileHash = FileSystem.GetFileHash(resourceFile);
                if (existingFileHash != storedFileHash) {
                    File.Copy(resourceFile, path, true);
                }
            }
            var text = File.ReadAllText(path);
            if (string.IsNullOrEmpty(text)) {
                return;
            }
            try {
                var objs = JsonConvert.DeserializeObject<List<RootObject>>(text);
                var now = GetByName("_version");
                long tsNow = 0;
                if (now != null) {
                    tsNow = Convert.ToInt64(now.Value);
                }
                if (objs.Any(_ => _.Key == "_version")) {
                    var version = objs.FirstOrDefault(_ => _.Key == "_version")?.Value;
                    if (Convert.ToInt64(version) > tsNow) {
                        DeleteAll();
                        foreach (var o in objs) {
                            var obj = new CommandValuesModel {
                                Name = o.Key,
                                Value = o.Value
                            };
                            DatabaseRepository.Save(AntdApplication.Database, obj, true);
                        }
                    }
                }
            }
            catch (Exception ex) {
                ConsoleLogger.Warn(ex);
            }
        }

        public bool Create(IDictionary<string, string> dict) {
            var name = dict["Name"];
            var value = dict["Value"];
            var obj = new CommandValuesModel {
                Name = name,
                Value = value
            };
            var result = DatabaseRepository.Save(AntdApplication.Database, obj, true);
            return result;
        }

        public bool Edit(IDictionary<string, string> dict) {
            var id = dict["Id"];
            var name = dict["Name"];
            var value = dict["Value"];
            var objUpdate = new CommandValuesModel {
                Id = id.ToGuid(),
                Name = name.IsNullOrEmpty() ? null : name,
                Value = value.IsNullOrEmpty() ? null : value
            };
            var result = DatabaseRepository.Edit(AntdApplication.Database, objUpdate, true);
            return result;
        }

        public bool Delete(string guid) {
            var result = DatabaseRepository.Delete<CommandValuesModel>(AntdApplication.Database, Guid.Parse(guid));
            return result;
        }

        public void DeleteAll() {
            var all = GetAll();
            foreach (var el in all) {
                Delete(el.Id);
            }
        }
    }
}
