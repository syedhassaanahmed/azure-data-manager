namespace DataManager.Models
{
    public class Job : BaseEntity
    {
        public bool IsActive { get; set; }
        public string Schedule { get; set; }
        public string[] From { get; set; }
        public string[] To { get; set; }
        public Specification Specification { get; set; }
    }

    public enum JobType
    {
        Databricks, Copy
    }

    public enum JobStage
    {
        Ingest, Transform
    }

    public class Specification
    {
        public JobStage Stage { get; set; }
        public JobType Type { get; set; }
        public string NotebookPath { get; set; }
        public NotebookParameter[] NotebookParameters { get; set; }
    }

    public class NotebookParameter
    {
        public string Name { get; set; }
        public string DatasetId { get; set; }
    }
}
