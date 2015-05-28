using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using o365Sample.Controllers;

namespace o365Sample.Models
{
    public class Office365AuthZResponse
    {
    //    public string user_email { get; set; }

    //    public string account_type { get; set; }

    //    public string authorization_service { get; set; }

    //    public string token_service { get; set; }

    //    public string scope { get; set; }

    //    public string unsupported_scope { get; set; }

    //    public string discovery_service { get; set; }

    //    public string discovery_resource { get; set; }
        public string admin_consent { get; set; }

        public string code { get; set; }

        public string session_state { get; set; }

        public string state { get; set; }

        public async Task<OAuthTokenResponse> GetOffice365AccessToken(string resourceId, HomeController homeController)
        {
            //now that he have an AuthZ code, we need to get a Access Token
            HttpContent content = new StringContent(
                string.Format("grant_type=authorization_code&client_id={0}&code={1}&client_secret={2}&resource={3}&redirect_uri={4}", AuthUtil.AzureAdApplicationClientId, code,
                    Uri.EscapeDataString(AuthUtil.AzureAdApplicationClientSecret), resourceId, AuthUtil.GetAuthZFinishUrl(homeController.Request)
                    )
                );

            OAuthTokenResponse token = null;
            using (var client = new HttpClient())
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(AuthUtil.AccessTokenEndpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonAccessData = await response.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<OAuthTokenResponse>(jsonAccessData);
                    JwtSecurityTokenHandler jst = new JwtSecurityTokenHandler();
                    if (jst.CanReadToken(token.id_token))
                    {
                        var securityToken = jst.ReadToken(token.id_token) as JwtSecurityToken;
                        token.securityToken = securityToken;
                    }
                }
                else
                {
                    throw new HttpException("Could not obtain an Office 365 Access token");
                }
            }
            return token;
        }
    }
}