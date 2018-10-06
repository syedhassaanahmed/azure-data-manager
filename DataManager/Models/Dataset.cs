namespace DataManager.Models
{
    public enum DatasetType
    {
        BlobStorage, SqlServer
    }

    public class Dataset : BaseEntity
    {
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public bool IsDynamic { get; set; }
        public DatasetType Type { get; set; }
        public string SecretName { get; set; }
        public string DataPath { get; set; }
    }
}