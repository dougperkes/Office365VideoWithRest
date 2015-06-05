using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using o365Sample.Controllers;

namespace o365Sample.Models
{
    internal class AuthUtil
    {
        public static readonly string AzureAdApplicationClientId = ConfigurationManager.AppSettings["AAD:ClientId"];
        public static readonly string AzureAdApplicationClientSecret = ConfigurationManager.AppSettings["AAD:ClientSecret"];
        public const string OauthEndpoint = "https://login.windows.net/common/oauth2/authorize";
        public const string AccessTokenEndpoint = "https://login.windows.net/common/oauth2/token";
        public const string DiscoveryEndpoint = "https://api.office.com/discovery/v1.0/me/services";
        public const string DiscoverySvcResourceId = "https://api.office.com/discovery/";

        public static async Task<OAuthTokenResponse> GetOAuthAccessToken(HttpRequestBase httpRequest, 
            Office365AuthZResponse o365AuthZ,
            string resourceId)
        {
            //now that he have an AuthZ code, we need to get a Access Token
            HttpContent content = new StringContent(
                String.Format(
                    "grant_type=authorization_code&client_id={0}&code={1}&client_secret={2}&resource={3}&redirect_uri={4}",
                    AuthUtil.AzureAdApplicationClientId, o365AuthZ.code,
                    Uri.EscapeDataString(AuthUtil.AzureAdApplicationClientSecret), resourceId, GetAuthZFinishUrl(httpRequest)
                    )
                );

            OAuthTokenResponse token = null;
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
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

        public async static Task<OAuthTokenResponse> GetOAuthAccessToken(string refreshToken, string resource)
        {
            //Retrieve access token using refresh token
            OAuthTokenResponse token = null;
            using (HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                HttpContent content =
                    new StringContent(
                        String.Format(
                            @"grant_type=refresh_token&refresh_token={0}&client_id={1}&client_secret={2}&resource={3}",
                            refreshToken, AzureAdApplicationClientId,
                            Uri.EscapeDataString(AzureAdApplicationClientSecret), resource));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                using (
                    HttpResponseMessage response =
                        await client.PostAsync("https://login.windows.net/common/oauth2/token", content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        token = JsonConvert.DeserializeObject<OAuthTokenResponse>(json);
                    }
                }
            }
            return token;
        }


        internal static string GetAuthZFinishUrl(HttpRequestBase httpRequest)
        {
            if (httpRequest.Url != null)
            {
                var returnUrl = String.Format("{0}://{1}/home/o365AuthZFinish",
                    httpRequest.Url.Scheme,
                    httpRequest.Headers["host"]);
                return returnUrl;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}