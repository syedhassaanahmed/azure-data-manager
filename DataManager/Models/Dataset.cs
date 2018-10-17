using System.IO;

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

        public string GetFileName() => Path.GetFileName(DataPath);
        public string GetFolderPath() => Path.GetDirectoryName(DataPath).Replace("\\", "/");
        public string GetFileExtension() => Path.GetExtension(DataPath);

        public string GetFolderParameter() => $"folderPath_{Id}";
        public string GetFileParameter() => $"fileName_{Id}";

        public string GetDataPathExpression() => IsDynamic ? 
            $"@if(equals(pipeline().TriggerType, 'BlobEventsTrigger'), concat('/', pipeline().parameters.{GetFolderParameter()}, '/', pipeline().parameters.{GetFileParameter()}), '{DataPath}')" 
            : DataPath;        
    }
}