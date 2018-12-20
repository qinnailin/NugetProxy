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
using BlueFox.Log;
using System.Xml;
using System.Xml.Linq;
using NuGet.Proxy.model;

namespace NuGet.Proxy.Controllers
{
    [ApiController]
    public class V2Controller : ControllerBase
    {
        private readonly string replateurl ;
        private readonly string connstring;
        private readonly IConfiguration _config;
        public V2Controller(IConfiguration config)
        {
            replateurl = config.GetSection("appSetting").GetValue<string>("NugetProxy");
            connstring = config.GetConnectionString("ConnectionString");
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
            catch (Exception ex)
            {
                LogService.Error(ex);
                return "Is Fail !!";
            }
        }

        [HttpGet]
        [Route("api/v2/Packages(Id='{id}',Version='{semVerLevel}')")]
        public async Task<string> Packages(string id,string semVerLevel)
        {
            try
            {
                DBServer db = new DBServer(connstring);
                var bo = db.Get(id, semVerLevel);
                using (WebClient client = new WebClient())
                {
                    string address = $"https://www.nuget.org/api/v2/Packages(Id='{id}',Version='{semVerLevel}')";
                    var res = await client.DownloadStringTaskAsync(address);
                    res = res.Replace("https://www.nuget.org", replateurl);
                    if (bo == null)
                    {
                        XElement element = XElement.Parse(res);
                        var x = element.Elements();
                        var properties = x.Where(w => w.Name.LocalName.ToLower() == "properties");
                        bo = new ODataPackage();
                        foreach (var item in properties.Elements())
                        {
                            var t = bo.GetType();
                            foreach (var p in t.GetProperties())
                            {
                                if (p.Name.ToLower() == item.Name.LocalName.ToLower())
                                {
                                    p.SetValue(bo, item.Value);
                                }
                            }
                        }
                        db.Save(bo);
                    }
                    return res;
                }
                
            }
            catch (Exception ex)
            {
                LogService.Error(ex);
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
            catch (Exception ex)
            {
                LogService.Error(ex);
                return "Is Fail !!";
            }
        }

        ///api/v2/FindPackagesById()?id='NUnit'&semVerLevel=2.0.0

        [HttpGet]
        [Route("api/v2/FindPackagesById()")]
        public string FindPackagesById(string id, string semVerLevel)
        {
            try
            {
                string skip = Request.Query["$skip"].FirstOrDefault();
                if (string.IsNullOrEmpty(skip)) skip = "0";
                using (WebClient client = new WebClient())
                {
                    string address = $"https://www.nuget.org/api/v2/FindPackagesById()?id={id}&semVerLevel={semVerLevel}&$skip={skip}";
                    var res = client.DownloadString(address);
                    res = res.Replace("https://www.nuget.org", replateurl);
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex);
                return "Is Fail !!";
            }
        }

        [HttpGet]
        [Route("api/v2/FindPackagesByIdAsyncCore")]
        public async Task<string> FindPackagesByIdAsyncCore(string id, string semVerLevel)
        {
            try
            {
                string skip = Request.Query["$skip"].FirstOrDefault();
                if (string.IsNullOrEmpty(skip)) skip = "0";
                using (WebClient client = new WebClient())
                {
                    string address = $"https://www.nuget.org/api/v2/FindPackagesById()?id={id}&semVerLevel={semVerLevel}&$skip={skip}";
                    var res = await client.DownloadStringTaskAsync(address);
                    res = res.Replace("https://www.nuget.org", replateurl);
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex);
                return "Is Fail !!";
            }
        }

        //https://www.nuget.org/api/v2/package/QRCoder/1.3.5
        [HttpGet]
        [Route("api/v2/Package/{id}/{semVerLevel}")]
        public async Task<IActionResult> Package(string id, string semVerLevel)
        {
           
            string path= System.IO.Directory.GetCurrentDirectory() + $"/Packages/{id}";
#if DEBUG
            path = $@"c:\work\2018\NuGetProxy\NuGetProxy\Packages\{id}";
#endif
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string fullName = $"{path}/{id}.{semVerLevel}.nupkg";
            string fileName = $"{id}.{semVerLevel}.nupkg";

            DBServer db = new DBServer(connstring);
            var bo = db.Get(id, semVerLevel);
            if (bo == null)
            {
                await Packages(id, semVerLevel);
                bo = db.Get(id, semVerLevel);
            }
            if (System.IO.File.Exists(fullName))
            {
                if (bo.IsDowning)
                {
                    throw new Exception("下载中~请稍后！");
                }
                try
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(fullName);
                    if (fileInfo.Length != Convert.ToInt64(bo.PackageSize))
                    {
                        fileInfo.Delete();
                        bo.IsDowning = true;
                        db.Save(bo);
                        await DownPackage(id, semVerLevel, fullName, path, fullName);
                        bo.IsDowning = false;
                        db.Save(bo);
                    }
                }
                catch (Exception ex)
                {
                    LogService.Error(ex);
                }
                return Download(path,fileName);
            }
            else
            {
                bo.IsDowning = true;
                db.Save(bo);
                await DownPackage(id, semVerLevel, fullName, path, fullName);
                bo.IsDowning = false;
                db.Save(bo);
                return Download(path, fileName);
            }
        }

        private async  Task DownPackage(string id,string semVerLevel,string fullName,string path,string fileName)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string address = $"https://www.nuget.org/api/v2/package/{id}/{semVerLevel}";
                    await client.DownloadFileTaskAsync(address, fullName);
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex);
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