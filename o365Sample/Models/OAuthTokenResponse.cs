using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;

namespace o365Sample.Models
{
    public class OAuthTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string expires_on { get; set; }
        public string id_token { get; set; }
        public string refresh_token { get; set; }
        public string resource { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }


        public JwtSecurityToken securityToken { get; set; }
    }
}