using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Common
{

    public struct JwtAuthResult
    {

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expiration")]
        public int Expiration { get; set; }

    }

}