namespace DataManager.Models
{
    public class Job : BaseEntity
    {
        public string[] DependsOn { get; set; }
        public bool Active { get; set; }
        public string Schedule { get; set; }
        public string[] From { get; set; }
        public string[] To { get; set; }
        public Specification Specification { get; set; }
    }
}

public class Specification
{
    public string Stage { get; set; }
    public string Type { get; set; }
    public string NotebookPath { get; set; }
    public NotebookParameter[] NotebookParameters { get; set; }
}

public class NotebookParameter
{
    public string Name { get; set; }
    public string DatasetId { get; set; }
}
