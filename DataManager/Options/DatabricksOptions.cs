namespace DataManager.Options
{
    public class DatabricksOptions
    {
        public string Endpoint { get; set; }
        public string KeyVaultSecretName { get; set; }
        public string ExistingClusterId { get; set; }
        public NewCluster NewCluster { get; set; }
    }

    public class NewCluster
    {
        public string NodeType { get; set; }
        public string NumberOfWorkers { get; set; }
        public string PythonVersion { get; set; }
        public string RuntimeVersion { get; set; }
    }
}
