using Newtonsoft.Json;
using o365Sample.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace o365Sample.Controllers
{
    public class VideoPushController : Controller
    {
        // GET: VideoPush
        public async Task<ActionResult> Index()
        {
            Office365Context o365Context = Session["Office365Context"] as Office365Context;
            if (o365Context == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Office365VideoServiceDiscoveryResponse videoInfo = null;
            var rootSiteResource = o365Context.Resources.SingleOrDefault(r => 
                r.capability.Equals("RootSite", StringComparison.InvariantCultureIgnoreCase));
            if (rootSiteResource == null) { throw new ArgumentException(); }

            //now that we have the resource, we need to check to see if we have a token for the resource
            var token = o365Context.Tokens.FirstOrDefault(t => t.resource.Equals(rootSiteResource.serviceResourceId));

            if (token == null)
            {
                //we need to use the refresh token from our original request to get a new token for the new resource
                var resourceToken =
                    o365Context.Tokens.First(t => t.resource.Equals(AuthUtil.DiscoverySvcResourceId));

                    //ok, so we don't yet have a token for that resource. We need to request one
                    token = await AuthUtil.GetOAuthAccessToken(resourceToken.refresh_token, rootSiteResource.serviceResourceId);
                    o365Context.Tokens.Add(token);
                    Session["Office365Context"] = o365Context;
            }

            var rootSiteUrl = rootSiteResource.serviceResourceId; //will be in the format https://perkes.sharepoint.com/
            var rootSiteApi = rootSiteResource.serviceEndpointUri;
            var videoPortalDiscoveryUrl = string.Format("{0}/VideoService.Discover", rootSiteApi);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
                using (var response = await client.GetAsync(videoPortalDiscoveryUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(json);
                        videoInfo = JsonConvert.DeserializeObject<Office365VideoServiceDiscoveryResponse>(json);
                        Session["VideoDiscoveryInfo"] = videoInfo;
                    }
                }
            }
            /*

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, videoPortalDiscoveryUrl);
                //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", o365Context.Token.access_token);

                //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + o365Context.Token.access_token);
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        videoInfo = JsonConvert.DeserializeObject<Office365VideoServiceDiscoveryResponse>(json);
                        Session["VideoDiscoveryInfo"] = videoInfo;
                    }
                }
            }
            */
            return View(videoInfo);
        }

        public async Task<ActionResult> Channels()
        {
            Office365Context o365Context = (Office365Context) Session["Office365Context"];
            var rootSiteResource = o365Context.Resources.Single(r =>
                r.capability.Equals("RootSite", StringComparison.InvariantCultureIgnoreCase));

            Office365VideoServiceDiscoveryResponse videoInfo =
                (Office365VideoServiceDiscoveryResponse) Session["VideoDiscoveryInfo"];
            var token = o365Context.Tokens.First(t => t.resource.Equals(rootSiteResource.serviceResourceId));

            var channelListUrl = string.Format("{0}/_api/VideoService/CanEditChannels", videoInfo.VideoPortalUrl);

            List<Office365VideoChannel> channels = new List<Office365VideoChannel>();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
                using (HttpResponseMessage response = await client.GetAsync(channelListUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(json);
                        JObject channelResult = JObject.Parse(json);
                        channels =
                            JsonConvert.DeserializeObject<List<Office365VideoChannel>>(channelResult["value"].ToString());
                    }
                }
            }

            return View(channels);
        }

        //public async Task<ActionResult> Folders()
        //{
        //            Office365Context o365Context = (Office365Context) Session["Office365Context"];
        //    var rootSiteResource = o365Context.Resources.Single(r =>
        //        r.capability.Equals("RootSite", StringComparison.InvariantCultureIgnoreCase));

        //    //Office365VideoServiceDiscoveryResponse videoInfo =
        //    //    (Office365VideoServiceDiscoveryResponse) Session["VideoDiscoveryInfo"];
        //    var token = o365Context.Tokens.First(t => t.resource.Equals(rootSiteResource.serviceResourceId));
    

        //}

        //public ActionResult PushToChannel(string channelId)
        //{

        //}
        [HttpGet]
        public ActionResult Videos(Guid channelid)
        {
            List<MyVideosViewModel> videos = new List<MyVideosViewModel>();
            var videoDir = VideoDirectory;
            foreach (var videoFile in videoDir.GetFiles())
            {
                videos.Add(new MyVideosViewModel()
                {
                    FileName = videoFile.Name,
                    FileSizeMb = (videoFile.Length/1024/1024).ToString("N") + "mb"
                });
            }
            ViewBag.ChannelId = channelid;
            return View(videos);
        }

        private DirectoryInfo VideoDirectory
        {
            get
            {
                var videoDir = new DirectoryInfo(Server.MapPath("~/content/samplevideos"));
                return videoDir;
            }
        }

        [HttpGet]
        public ActionResult UploadVideo(Guid channelId, string fileName)
        {
            return View(new UploadVideoViewModel()
            {
                ChannelId = channelId,
                Description = "",
                FileName = fileName,
                Title = fileName
            });
        }

        [HttpPost]
        public async Task<ActionResult> UploadVideo(UploadVideoViewModel videoData)
        {
            var videoFile = VideoDirectory.GetFiles(videoData.FileName)[0];

            Office365Context o365Context = (Office365Context)Session["Office365Context"];
            var rootSiteResource = o365Context.Resources.Single(r =>
                r.capability.Equals("RootSite", StringComparison.InvariantCultureIgnoreCase));

            Office365VideoServiceDiscoveryResponse videoDiscoveryInfo =
                (Office365VideoServiceDiscoveryResponse) Session["VideoDiscoveryInfo"];
            var token = o365Context.Tokens.First(t => t.resource.Equals(rootSiteResource.serviceResourceId));

            // Uploading videos is the 3 step process
            // 1. Create a placeholder for where you will upload the video
            // 2. Upload a smaller video in a single post or
            // 2. Upload a larger video in chunks

            // Create the placeholder
            var channelVideosUrl = string.Format("{0}/_api/VideoService/Channels('{1}')/Videos", 
                videoDiscoveryInfo.VideoPortalUrl, videoData.ChannelId);

            //make a pseudo unique name to avoid file naming conflicts during testing
            var o365VideoFileName = Math.Abs(Guid.NewGuid().GetHashCode()) + videoData.FileName;

            var jsonPostData =
                string.Format(
                    "{{ '__metadata': {{ 'type': 'SP.Publishing.VideoItem' }}, 'Description': ' {0} ', 'Title': ' {1} ', 'FileName': '{2}' }}",
                    videoData.Description, videoData.Title, o365VideoFileName);

            var placeholderRequestBody = new StringContent(
                    jsonPostData
                );
            placeholderRequestBody.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");

            VideoFileInformation o365VideoFileData = null;
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
                //client.DefaultRequestHeaders.Add("X-RequestDigest", formDigestValue);
                using (var response = await client.PostAsync(channelVideosUrl, placeholderRequestBody))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        o365VideoFileData = JsonConvert.DeserializeObject<VideoFileInformation>(JObject.Parse(json)["d"].ToString());

                        var videoFileInfo = new FileInfo(Path.Combine(Server.MapPath("~/content/samplevideos"), videoData.FileName));

                        // let's make up a size to slice our data on. Technically our slices could be up to 
                        // 100 MB if we weren't concerned with time outs.
                        const int chunkSize = 5*1024*1024; // 5 MB
                        if (videoFileInfo.Length <= chunkSize)
                        {
                            var uploadUrl =
                                string.Format(
                                    "{0}/_api/VideoService/Channels('{1}')/Videos('{2}')/GetFile()/SaveBinaryStream",
                                    videoDiscoveryInfo.VideoPortalUrl, videoData.ChannelId, o365VideoFileData.ID
                                    );

                            using (var fileContent = new StreamContent(videoFileInfo.OpenRead()))
                            {
                                using (var uploadResponse = await client.PostAsync(uploadUrl, fileContent))
                                {
                                    if (uploadResponse.IsSuccessStatusCode)
                                    {
                                        string uploadJsonResponse = await uploadResponse.Content.ReadAsStringAsync();
                                    }
                                }
                            }
                        }
                        else
                        {
                            long bytesUploaded = 0;
                            var uploadSessionId = Guid.NewGuid();
                            bool canContinue = true;
                            var startUrl = string.Format(
                                "{0}/_api/VideoService/Channels('{1}')/Videos('{2}')/GetFile()/StartUpload(uploadId=guid'{3}')",
                                videoDiscoveryInfo.VideoPortalUrl, videoData.ChannelId, o365VideoFileData.ID, uploadSessionId
                                );
                            using (HttpResponseMessage startResponseMessage = await client.PostAsync(startUrl, null))
                            {
                                canContinue = startResponseMessage.IsSuccessStatusCode;
                            }

                            var totalChunks = Math.Ceiling(videoFileInfo.Length/(double) chunkSize);

                            //need to upload all but the last chunk
                            while (bytesUploaded <  chunkSize * (totalChunks-1))
                            {
                                if (!canContinue) { break; }

                                using (
                                    var fileReader =
                                        new BinaryReader(System.IO.File.Open(videoFileInfo.FullName, FileMode.Open)))
                                {
                                    fileReader.BaseStream.Seek(bytesUploaded, SeekOrigin.Begin);
                                    
                                    var fileSlice = fileReader.ReadBytes(Convert.ToInt32(chunkSize));
                                    string chunkUploadUrl =
                                        string.Format(
                                            "{0}/_api/VideoService/Channels('{1}')/Videos('{2}')/GetFile()/ContinueUpload(uploadId=guid'{3}',fileOffset='{4}')",
                                            videoDiscoveryInfo.VideoPortalUrl, videoData.ChannelId, o365VideoFileData.ID,
                                            uploadSessionId, bytesUploaded
                                            );
                                    using (var fileContent = new StreamContent(new MemoryStream(fileSlice)))
                                    {
                                        using (
                                            HttpResponseMessage uploadResponse =
                                                await client.PostAsync(chunkUploadUrl, fileContent))
                                        {
                                            canContinue = uploadResponse.IsSuccessStatusCode;
                                            bytesUploaded += chunkSize;
                                        }
                                    }

                                }

                            }

                            if (canContinue)
                            {
                                // still sucessful, upload the last chunk of data
                                var bytesToRead = videoFileInfo.Length - bytesUploaded;
                                using (
                                    var fileReader =
                                        new BinaryReader(System.IO.File.Open(videoFileInfo.FullName, FileMode.Open)))
                                {
                                    fileReader.BaseStream.Seek(bytesUploaded, SeekOrigin.Begin);

                                    var fileSlice = fileReader.ReadBytes(Convert.ToInt32(bytesToRead));
                                    string chunkUploadUrl =
                                        string.Format(
                                            "{0}/_api/VideoService/Channels('{1}')/Videos('{2}')/GetFile()/FinishUpload(uploadId=guid'{3}',fileOffset='{4}')",
                                            videoDiscoveryInfo.VideoPortalUrl, videoData.ChannelId, o365VideoFileData.ID,
                                            uploadSessionId, bytesUploaded
                                            );
                                    using (var fileContent = new StreamContent(new MemoryStream(fileSlice)))
                                    {
                                        // this last request is the slowest
                                        // perhaps the Office 365 back-end has to compile all the chunks together
                                        using (
                                            HttpResponseMessage uploadResponse =
                                                await client.PostAsync(chunkUploadUrl, fileContent))
                                        {
                                            canContinue = uploadResponse.IsSuccessStatusCode;
                                            bytesUploaded += bytesToRead;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }



            Session["UploadedVideoFileInfo"] = o365VideoFileData;
            return RedirectToAction("UploadSuccessful"); //, new { videoUrl = ???});
        }

        public ActionResult UploadSuccessful()
        {
            VideoFileInformation videoInfo = (VideoFileInformation) Session["UploadedVideoFileInfo"];
            return View(videoInfo);
        }
    }
}