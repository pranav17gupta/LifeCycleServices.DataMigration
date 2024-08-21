namespace POCDataMigrationWebApp.Models
{
    public class JobResponse
    {
        public NotebookOutput notebook_output { get; set; } 
        public Metadata metadata { get; set; }

    }
    public class NotebookOutput
    {
        public string result { get; set; }
        public Boolean truncated { get; set; }
    }
    public class Metadata
    {
        public long job_id { get; set; }
        public long run_id { get; set; }
        public long number_in_job { get; set; }
        public string creator_user_name { get; set; }   
        public long original_attempt_run_id { get; set; }
        public State state { get; set; }

    }
    public class State
    {
        public string life_cycle_state { get; set; }
        public string result_state { get; set; }
        public Boolean user_cancelled_or_timedout { get; set; }
        public string state_message { get; set; }
    }
}
