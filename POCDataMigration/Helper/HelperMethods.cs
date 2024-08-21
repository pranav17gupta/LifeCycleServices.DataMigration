using POCDataMigrationAPI.Models;
using System.Text.RegularExpressions;

namespace POCDataMigrationAPI.Helper
{
    public class HelperMethods
    {
        public static string encodeData(List<DbData>? dbDataList)
        {
            //DbData dbd1 = new();
            //dbd1.SchemaName = "PROD_SCH";
            //dbd1.TableList = new List<string>() { "Company", "Product", "Purchase" };
            //DbData dbd2 = new();
            //dbd2.SchemaName = "EMP_SCH";
            //dbd2.TableList = new List<string>() { "Employee", "Department" };
            //DbDataList = new List<DbData>() { dbd1, dbd2 };

            string enc = "";
            for(var i=0; i<dbDataList.Count; i++)
            {
                string schema = dbDataList[i].SchemaName;
                string schTbl = schema + ",";
                List<string> list = dbDataList[i].TableList;
                for(var j=0; j<list.Count-1; j++)
                    schTbl += list[j]+",";
                schTbl += list[list.Count - 1] + "|";
                enc += schTbl;
            }
            enc = enc.Substring(0, enc.Length-1);
            return enc;
        }
        public static string getValidContainerName(String containerName)
        {
            containerName = containerName.Trim().ToLower();
            Regex rgx = new Regex("[^0-9a-zA-Z]+");
            containerName = rgx.Replace(containerName, "-");
            if(containerName.Length < 3)
                containerName = "abc" + containerName;
            if (containerName.Length > 63)
                containerName = containerName.Substring(0, 63);
            if (containerName[0] == '-')
                containerName = "x" + containerName.Substring(1);
            if (containerName[containerName.Length - 1] == '-')
                containerName = containerName.Substring(0, containerName.Length - 1) + "x";
            return containerName;
        }
    }
}
