using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POCDataMigration.Models;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using POCDataMigrationAPI.Models;
using System.Data.SqlClient;
using POCDataMigrationAPI.Helper;
using Azure.Storage.Blobs;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure;

namespace POCDataMigration.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly SecretClient _secretsClient;
        public JobController(IConfiguration configuration)
        {
            var kvUrl = configuration["AzureKeyVaultUrl"];
            _secretsClient = new SecretClient(new Uri(kvUrl), new DefaultAzureCredential());
        }

        /// <summary>
        /// Trigger Databricks job
        /// </summary>
        /// <param name="dmd"></param>
        /// <returns>
        /// </returns>
        [HttpPost]
        
        public async Task<string> TriggerJob(DataMigrationDetails dmd)
        {
            string apiUrl = "https://adb-4568774372593866.6.azuredatabricks.net/api/2.1/jobs/run-now";
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dapi122ef8bd2bbbfc76b4523dd462a67a68");
            try
            {
                var noteBookParam = new
                {
                    storageaccount = dmd.storageaccount,
                    dbserver = dmd.dbserver,
                    dbname = dmd.dbname,
                    dbpwd = dmd.dbpwd,
                    dbuser = dmd.dbuser,
                    schema = dmd.schema
                };
                var request = new { job_id = 596134393371431, notebook_params = noteBookParam };
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                //Console.WriteLine("Details: " + dmd.DbServer + " " + dmd.DbName + " " + dmd.DbUser + " " + dmd.DbPwd + " " + dmd.SchemaName + " " + dmd.StorageAccount);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return data;
                }
                else
                {
                    string statusCode = response.StatusCode.ToString();
                    return statusCode;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Get result from databricks job
        /// </summary>
        /// <param name="runId"></param>
        /// <returns></returns>
        [EnableCors("corspolicy")]
        [HttpGet("{runId}")]
        public async Task<String> GetJobResult(string runId)
        {
            string apiUrl = "https://adb-617673845129022.2.azuredatabricks.net/api/2.0/jobs/runs/get-output?run_id=" + runId;
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dapi94980c5ec73e8b6fa8bb0296d7fa742a");
            try
            {
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode) 
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return data;
                }
                else
                {
                    string statusCode = response.StatusCode.ToString();
                    return statusCode;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Get SchemaName for given DB details
        /// </summary>
        /// <param name="dbDetails"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> getSchema([FromQuery] DbDetails dbDetails)
        {
            string connetionString = "";
            string sql = "";
            SqlConnection? connection = null;
            SqlCommand? command = null;
            SqlDataReader? dataReader = null;
            List<string> schemaList = new();
            switch (dbDetails.DbType.ToUpper())
            {
                case "SQLSERVER":
                    connetionString = "Data Source=" + dbDetails.DbServer + ";Initial Catalog=" + dbDetails.DbName + ";User ID=" + dbDetails.DbUsername + ";Password=" + dbDetails.DbPassword;
                    sql = "USE [" + dbDetails.DbName + "]\r\nselect s.name as schema_name,\r\n    s.schema_id,\r\n    u.name as schema_owner\r\nfrom sys.schemas s\r\n    inner join sys.sysusers u\r\n        on u.uid = s.principal_id\r\nwhere u.name = 'dbo'\r\norder by s.name";
                    connection = new SqlConnection(connetionString);
                    try
                    {
                        connection.Open();
                        command = new SqlCommand(sql, connection);
                        command.ExecuteNonQuery();
                        dataReader = command.ExecuteReader();
                        while (dataReader.Read())
                        {
                            schemaList.Add(dataReader.GetString(0));
                            Console.WriteLine(dataReader.GetValue(0) + " - " + dataReader.GetValue(1) + " - " + dataReader.GetValue(2));
                        }
                        command.Dispose();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        var message = ex.Message;
                        var res = message.Split(" ");
                        string result = "DbConnectionError";
                        if (res[0] == "Login" && res[1] == "failed")
                            result = res[0] + res[1];
                        else if (res[1] == "network-related")
                            result = "InvalidServer";
                        return StatusCode(500, result);
                    }
                    break;
            }
            return Ok(schemaList);
        }


        /// <summary>
        /// Get DB tables for given Schema
        /// </summary>
        /// <param name="dbDetails"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> getTables([FromQuery] DbDetails dbDetails, string schema)
        {
            string connetionString = "";
            string sql = "";
            SqlConnection? connection = null;
            SqlCommand? command = null;
            SqlDataReader? dataReader = null;
            List<string> tableList = new();
            switch (dbDetails.DbType.ToUpper())
            {
                case "SQLSERVER":
                    connetionString = "Data Source=" + dbDetails.DbServer + ";Initial Catalog=" + dbDetails.DbName + ";User ID=" + dbDetails.DbUsername + ";Password=" + dbDetails.DbPassword;
                    sql = "USE [" + dbDetails.DbName + "]\r\nselect name as table_name\r\nfrom sys.tables\r\nwhere schema_name(schema_id) = '" + schema + "'\r\norder by name;";
                    connection = new SqlConnection(connetionString);
                    try
                    {
                        connection.Open();
                        command = new SqlCommand(sql, connection);
                        command.ExecuteNonQuery();
                        dataReader = command.ExecuteReader();
                        while (dataReader.Read())
                        {
                            tableList.Add(dataReader.GetString(0));
                            Console.WriteLine(dataReader.GetValue(0));
                        }
                        command.Dispose();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Can not open connection ! ");
                        return Ok("Please enter valid schema name");
                    }
                    break;
            }
            return Ok(tableList);
        }


        /// <summary>
        /// Updated job trigger method -- incomplete
        /// </summary>
        /// <param name="dmd"></param>
        /// <returns>
        /// {"run_id":int,"number_in_job":int}
        /// </returns>
        [HttpPost]
        public async Task<string> TriggerMigrateJob([FromBody] DataMigrateDetails dmd)
        {
            string apiUrl = "https://adb-617673845129022.2.azuredatabricks.net/api/2.1/jobs/run-now";
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "dapi94980c5ec73e8b6fa8bb0296d7fa742a");
            string encodedDetailedList = JsonConvert.SerializeObject(dmd.DbDataList);
            try
            {
                var noteBookParam = new
                {
                    storageaccount = dmd.StorageAccount,
                    dbserver = dmd.DbServer,
                    dbname = dmd.DbName,
                    dbpwd = dmd.DbPwd,
                    dbuser = dmd.DbUser,
                    //SchemaName = dmd.SchemaName,
                    container = dmd.ContainerName,
                    dbData = encodedDetailedList
                };
                //PRANAV
                var request = new { job_id = 126223547745136, notebook_params = noteBookParam };
                //var request = new { job_id = 457580438391974, notebook_params = noteBookParam };
                
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                //Console.WriteLine("Details: " + dmd.DbServer + " " + dmd.DbName + " " + dmd.DbUser + " " + dmd.DbPwd + " " + dmd.SchemaName + " " + dmd.StorageAccount);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return data;
                }
                else
                {
                    string statusCode = response.StatusCode.ToString();
                    return statusCode;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Check whether given container exists on laptestblob storage account 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<String> containerExist(string container)
        {
            bool isExists = false;
            try
            {
                var accKey = await _secretsClient.GetSecretAsync("laptestblob1key");
                var connStr = "DefaultEndpointsProtocol=https;AccountName=laptestblob1;AccountKey=" + accKey.Value.Value + ";EndpointSuffix=core.windows.net";
                container = HelperMethods.getValidContainerName(container);
                BlobContainerClient blobContainer = new BlobContainerClient(connStr, container);
                isExists = blobContainer.Exists();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            
            return isExists.ToString();
        }
    }
}
