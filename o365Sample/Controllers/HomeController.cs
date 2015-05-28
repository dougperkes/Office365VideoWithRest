using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using o365Sample.Models;

namespace o365Sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult o365AuthZStart()
        {
            var returnUrl = AuthUtil.GetAuthZFinishUrl(Request);
            Session["AuthZState"] = Guid.NewGuid().ToString();
            var oAuthUrl = string.Format("{0}?response_type=code&client_id={1}&redirect_url={2}&state={3}", AuthUtil.OauthEndpoint, AuthUtil.AzureAdApplicationClientId, returnUrl, Session["AuthZState"]);

            return new RedirectResult(oAuthUrl);
        }

        [HttpGet]
        public async Task<ActionResult> o365AuthZFinish()
        {
            Office365AuthZResponse o365AuthZ = new Office365AuthZResponse()
            {
                admin_consent = Request["admin_consent"],
                code = Request["code"],
                session_state = Request["session_state"],
                state = Request["state"]
            };

            if (!o365AuthZ.state.Equals(Session["AuthZState"]))
            {
                // something bad happened and this could be a cross-site request forgery attack
                throw new ArgumentException("The 'state' value in the o365AuthZ response did not match the expected value. Possible cross-site request forgery attack");
            }

            Office365Context o365Context = new Office365Context();
            o365Context.Tokens = new List<OAuthTokenResponse>();
            //now that we have the AuthZ data, we need to cache it. Let's stash it in the user session for now
            //Session["o365AuthZ"] = o365AuthZ;

            OAuthTokenResponse token = await AuthUtil.GetOAuthAccessToken(Request, o365AuthZ, AuthUtil.DiscoverySvcResourceId);
            o365Context.Tokens.Add(token);
            //Session["o365Token"] = token;
            //make a call into the discovery service
            List<Office365DiscoveryResource> office365Resources = await DiscoverOffice365Resources(token);
            o365Context.Resources = office365Resources;
            //Session["o365Resources"] = office365Resources;
            //var myFileResource = office365Resources.FirstOrDefault(i => i.capability == "MyFiles");
            Session["Office365Context"] = o365Context;
            //ViewBag.ctx = JsonConvert.SerializeObject(o365Context);
            return new RedirectResult("~/home/signinsuccessful");
        }

        public ActionResult SignInSuccessful()
        {
            Office365Context o365Context = Session["Office365Context"] as Office365Context;
            ViewBag.PersonName = o365Context.Tokens[0].securityToken.Claims.First(c => c.Type == "name").Value;
            ViewBag.PersonUniqueName = o365Context.Tokens[0].securityToken.Claims.First(c => c.Type == "unique_name").Value;
            return View(o365Context);
        }



        private static async Task<List<Office365DiscoveryResource>> DiscoverOffice365Resources(OAuthTokenResponse token)
        {
            List<Office365DiscoveryResource> office365Resources = null;
            using (HttpClient client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
                using (HttpResponseMessage response = await client.GetAsync(AuthUtil.DiscoveryEndpoint))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JObject root = JObject.Parse(json);
                        office365Resources = ((JArray)root["value"]).ToObject<List<Office365DiscoveryResource>>();
                        //
                    }
                }
            }
            return office365Resources;
        }


    }
}