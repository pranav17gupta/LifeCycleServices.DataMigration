namespace POCDataMigrationAPI.Models
{
    public class DataMigrateDetails
    {
        public string? DbServer { get; set; }
        public string? DbName { get; set; }
        public string? DbUser { get; set; }
        public string? DbPwd { get; set; }
        public List<DbData>? DbDataList { get; set; }
        public string? StorageAccount { get; set; }
        public string? ContainerName { get; set; }

    }
    public class DbData
    {
        public DbData() {
            
        }
        public string? SchemaName { get; set; }
        public List<string>? TableList { get; set; }
    }
}
