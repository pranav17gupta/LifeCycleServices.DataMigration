namespace POCDataMigrationWebApp.Models
{
    public class DataMigrationDetails
    {
        public string? dbserver { get; set; }
        public string? dbname { get; set; }
        public string? dbuser { get; set; }
        public string? dbpwd { get; set; }  
        public string? schema { get; set; }
        public string? storageaccount { get; set; }
    }
}
