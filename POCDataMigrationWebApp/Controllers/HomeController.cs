using Microsoft.AspNetCore.Mvc;
using POCDataMigrationWebApp.Models;
using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting.Server;
using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;

namespace POCDataMigrationWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        static int runId = 0;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> JobTrigger(DataMigrationDetails dmd)
        {
            string _storageaccount = dmd.storageaccount;
            string _dbserver = dmd.dbserver;
            string _dbname = dmd.dbname;
            string _dbpwd = dmd.dbpwd;
            string _dbuser = dmd.dbuser;
            string _schema = dmd.schema;

            //Calling API to trigger Job
            string apiUrl = "https://localhost:7205/api/Job/TriggerJob";
            using HttpClient client = new();
            try
            {
                var request = new { dbserver = _dbserver, dbuser = _dbuser, dbpwd = _dbpwd, dbname = _dbname, schema = _schema, storageaccount = _storageaccount };
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    RunDetail rd = JsonConvert.DeserializeObject<RunDetail>(data);
                    runId = rd.run_id;
                    ViewBag.RunId = runId;
                    Console.WriteLine(rd.run_id);
                }
                else
                {
                    string statusCode = response.StatusCode.ToString();
                    Console.WriteLine(statusCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //return View();
            var result = new { res = "Job is triggered" };
            //var result = _storageaccount + " " + _dbuser + " " + _dbserver + " " + _dbname + " " + _dbpwd + " " + _schema + " " + _storageaccount;
            //return Json("hello", new Newtonsoft.Json.JsonSerializerSettings());
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(result));
        }

        public async Task<IActionResult> ViewResult(DataMigrationDetails dmd)
        {
            ViewBag.RunId = runId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSchema(DbDetails dbDetails)
        {
            string apiUrl = "https://localhost:7205/api/Job/getSchema";
            var query = new Dictionary<string, string?>()
            {
                ["DbType"] = dbDetails.DbType,
                ["DbServer"] = dbDetails.DbServer,
                ["DbUsername"] = dbDetails.DbUsername,
                ["DbPassword"] = dbDetails.DbPassword,
                ["DbName"] = dbDetails.DbName,

            };
            var uri = QueryHelpers.AddQueryString(apiUrl, query);
            using HttpClient client = new();
            try
            {
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return Ok(data);
                }
                else
                {
                    var data = await response.Content.ReadAsStringAsync();
                    string statusCode = response.StatusCode.ToString();
                    return Ok(data);
                }
            }
            catch (Exception e)
            {
                return Ok(e.Message);
                //return View("~/Views/Shared/Error.cshtml");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTables(DbDetails dbDetails, string schema)
        {
            string apiUrl = "https://localhost:7205/api/Job/getTables";
            var query = new Dictionary<string, string?>()
            {
                ["DbType"] = dbDetails.DbType,
                ["DbServer"] = dbDetails.DbServer,
                ["DbUsername"] = dbDetails.DbUsername,
                ["DbPassword"] = dbDetails.DbPassword,
                ["DbName"] = dbDetails.DbName,
                ["Schema"] = schema,
            };
            var uri = QueryHelpers.AddQueryString(apiUrl, query);
            using HttpClient client = new();
            try
            {
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return Ok(data);
                }
                else
                {
                    var data = await response.Content.ReadAsStringAsync();
                    string statusCode = response.StatusCode.ToString();
                    return Ok(data);
                }
            }
            catch (Exception e)
            {
                return Ok(e.Message);
                //return View("~/Views/Shared/Error.cshtml");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ContainerExist(string containerName)
        {
            string apiUrl = "https://localhost:7205/api/Job/containerExist?container=" + containerName;
            using HttpClient client = new();
            try
            {
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return Ok(data);
                }
                else
                {
                    var data = await response.Content.ReadAsStringAsync();
                    string statusCode = response.StatusCode.ToString();
                    return Ok(data);
                }
            }
            catch (Exception e)
            {
                return Ok(e.Message);
                //return View("~/Views/Shared/Error.cshtml");
            }
        }

        [HttpPost]
        public async Task<IActionResult> JobTriggerUpdate(DataMigrateDetails dmd)
        { 
            //Calling API to trigger Job
            string apiUrl = "https://localhost:7205/api/Job/TriggerMigrateJob";
            using HttpClient client = new();
            try
            {
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(dmd), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    RunDetail rd = JsonConvert.DeserializeObject<RunDetail>(data);
                    runId = rd.run_id;
                    ViewBag.RunId = runId;
                    Console.WriteLine(rd.run_id);
                }
                else
                {
                    string statusCode = response.StatusCode.ToString();
                    Console.WriteLine(statusCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //return View();
            var result = new { res = "Job is triggered" };
            //var result = _storageaccount + " " + _dbuser + " " + _dbserver + " " + _dbname + " " + _dbpwd + " " + _schema + " " + _storageaccount;
            //return Json("hello", new Newtonsoft.Json.JsonSerializerSettings());
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(result));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}