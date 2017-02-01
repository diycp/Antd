﻿using antdlib.config;
using antdlib.models;
using Nancy;
using Newtonsoft.Json;

namespace Antd.ServerModules {
    public class AntdOvermountStatusModule : NancyModule {

        public AntdOvermountStatusModule() {
            Get["/overmountstatus"] = x => {
                var info = new MountManagement();
                var model = new PageOvermountStatusModel {
                    Components = info.GetAll()
                };
                return JsonConvert.SerializeObject(model);
            };
        }
    }
}
