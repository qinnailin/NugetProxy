using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http;

namespace NuGetProxy.Controllers
{
    [ApiController]
    public class V2Controller : ControllerBase
    {
        private readonly string replateurl ;
        private readonly IConfiguration _config;
        public V2Controller(IConfiguration config)
        {
            replateurl = config.GetSection("appSetting").GetValue<string>("NugetProxy");
        }

        [HttpGet]
        [Route("api/v2")]
        public async Task<IActionResult> Default()
        {
            using (WebClient client = new WebClient())
            {
                string address = $"https://www.nuget.org/api/v2";
                var res = await client.DownloadStringTaskAsync(address);
                return Content(res,"text/xml");
            }
        }

        [HttpGet]
        [Route("api/v2/Packages")]
        public async Task<string> Packages(string semVerLevel = "")
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string address = $"https://www.nuget.org/api/v2/Packages?semVerLevel={semVerLevel}";
                    var res = await client.DownloadStringTaskAsync(address);
                    res = res.Replace("https://www.nuget.org", replateurl);
                    return res;
                }
            }
            catch (Exception)
            {

                return "Is Fail !!";
            }
        }

        ///Search()?$filter=IsLatestVersion&searchTerm=''&targetFramework=''&includePrerelease=false&$skip=0&$top=26&semVerLevel=2.0.0
        [HttpGet]
        [Route("api/v2/Search()")]
        public async Task<string> Search(string searchTerm,string targetFramework,string includePrerelease,string semVerLevel)
        {
            try
            {
                string filter = Request.Query["$filter"].FirstOrDefault();
                if (string.IsNullOrEmpty(filter)) filter = "IsLatestVersion";
                string skip = Request.Query["$skip"].FirstOrDefault();
                if (string.IsNullOrEmpty(skip)) skip = "0";
                string top = Request.Query["$top"].FirstOrDefault();
                if (string.IsNullOrEmpty(top)) top = "26";

                using (WebClient client = new WebClient())
                {
                    string address = $"https://www.nuget.org/api/v2/Search()?$filter={filter}&searchTerm={searchTerm}&targetFramework={targetFramework}&includePrerelease={includePrerelease}&$skip={skip}&$top={top}&semVerLevel={semVerLevel}";
                    var res = await client.DownloadStringTaskAsync(address);
                    res = res.Replace("https://www.nuget.org", replateurl);
                    return res;
                }
            }
            catch (Exception)
            {

                return "Is Fail !!";
            }
        }

        ///api/v2/FindPackagesById()?id='NUnit'&semVerLevel=2.0.0

        [HttpGet]
        [Route("api/v2/FindPackagesById()")]
        public async Task<string> FindPackagesById(string id, string semVerLevel)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string address = $"https://www.nuget.org/api/v2/FindPackagesById()?id={id}&semVerLevel={semVerLevel}";
                    var res = await client.DownloadStringTaskAsync(address);
                    res = res.Replace("https://www.nuget.org", replateurl);
                    return res;
                }
            }
            catch (Exception)
            {

                return "Is Fail !!";
            }
        }

        //https://www.nuget.org/api/v2/package/QRCoder/1.3.5
        [HttpGet]
        [Route("api/v2/Package/{id}/{semVerLevel}")]
        public async Task<IActionResult> Package(string id, string semVerLevel)
        {
            string path= System.IO.Directory.GetCurrentDirectory() + "/Packages";
#if DEBUG
            path = @"c:\work\2018\NuGetProxy\NuGetProxy\Packages";
#endif
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string fullName = $"{path}/{id}.{semVerLevel}.nupkg";
            string fileName = $"{id}.{semVerLevel}.nupkg";
            if (System.IO.File.Exists(fullName))
            {
                return Download(path,fileName);
            }
            else
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string address = $"https://www.nuget.org/api/v2/package/{id}/{semVerLevel}";
                        await client.DownloadFileTaskAsync(address, fullName);
                        return Download(path, fileName);
                    }
                }
                catch (Exception)
                {
                    return Content("Is Fail !!");
                }
            }
        }

        private IActionResult Download(string path,string name)
        {
            IFileProvider provider = new PhysicalFileProvider(path);
            IFileInfo fileInfo = provider.GetFileInfo(name);
            var readStream = fileInfo.CreateReadStream();
            return File(readStream, "application/x-zip-compressed", name);
        }
    }
}