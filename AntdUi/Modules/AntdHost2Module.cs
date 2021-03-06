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

using antdlib.models;
using Nancy;
using anthilla.core;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AntdUi.Modules {
    public class AntdHost2Module : NancyModule {

        private readonly ApiConsumer _api = new ApiConsumer();

        public AntdHost2Module() {
            Get["/host2"] = x => {
                var model = _api.Get<PageHost2Model>($"http://127.0.0.1:{Application.ServerPort}/host2");
                var json = JsonConvert.SerializeObject(model);
                return json;
            };

            Post["/host2/info"] = x => {
                string hostName = Request.Form.HostName;
                string hostChassis = Request.Form.HostChassis;
                string hostDeployment = Request.Form.HostDeployment;
                string hostLocation = Request.Form.HostLocation;
                string hostAliasPrimary = Request.Form.HostAliasPrimary;
                string internalDomainPrimary = Request.Form.InternalDomainPrimary;
                string externalDomainPrimary = Request.Form.ExternalDomainPrimary;
                string internalHostIpPrimary = Request.Form.InternalHostIpPrimary;
                string externalHostIpPrimary = Request.Form.ExternalHostIpPrimary;
                string internalNetPrimaryBits = Request.Form.InternalNetPrimaryBits;
                string externalNetPrimaryBits = Request.Form.ExternalNetPrimaryBits;
                string resolvNameserver = Request.Form.ResolvNameserver;
                string resolvDomain = Request.Form.ResolvDomain;
                string timezone = Request.Form.Timezone;
                string ntpdateServer = Request.Form.NtpdateServer;
                string cloud = Request.Form.Cloud;
                var dict = new Dictionary<string, string> {
                    { "HostName", hostName },
                    { "HostChassis", hostChassis },
                    { "HostDeployment", hostDeployment },
                    { "HostLocation", hostLocation },
                    { "HostAliasPrimary", hostAliasPrimary },
                    { "InternalDomainPrimary", internalDomainPrimary },
                    { "ExternalDomainPrimary", externalDomainPrimary },
                    { "InternalHostIpPrimary", internalHostIpPrimary },
                    { "ExternalHostIpPrimary", externalHostIpPrimary },
                    { "InternalNetPrimaryBits", internalNetPrimaryBits },
                    { "ExternalNetPrimaryBits", externalNetPrimaryBits },
                    { "ResolvNameserver", resolvNameserver },
                    { "ResolvDomain", resolvDomain },
                    { "Timezone", timezone },
                    { "NtpdateServer", ntpdateServer },
                    { "Cloud", cloud }
                };
                return _api.Post($"http://127.0.0.1:{Application.ServerPort}/host2/info", dict);
            };
        }
    }
}