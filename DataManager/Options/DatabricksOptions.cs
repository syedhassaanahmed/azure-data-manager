namespace DataManager.Options
{
    public class DatabricksOptions
    {
        public string Endpoint { get; set; }
        public string KeyVaultSecretName { get; set; }
        public string NodeType { get; set; }
        public string NumberOfWorkers { get; set; }
        public int PythonVersion { get; set; }
        public string RuntimeVersion { get; set; }
    }
}
