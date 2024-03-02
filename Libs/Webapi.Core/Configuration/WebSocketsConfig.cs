using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Webapi.Core;

namespace Webapi.Core
{
    public partial class WebSocketsConfig : IConfig
    {
        public bool UseWebSockets { get; set; } = false;
        public string WebSocketRequestPath { get; set; } = "/ws";
        public int BufferSize { get; set; }
    }
}
