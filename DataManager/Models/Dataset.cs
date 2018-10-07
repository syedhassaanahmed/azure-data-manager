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

        public string FileName => Path.GetFileName(DataPath);
        public string FolderPath => Path.GetDirectoryName(DataPath).Replace("\\", "/");
        public string FileExtension => Path.GetExtension(DataPath);

        public string FolderParameter => $"folderPath_{Id}";
        public string FileParameter => $"fileName_{Id}";

        public string DataPathExpression => IsDynamic ? 
            $"@if(equals(pipeline().TriggerType, 'BlobEventsTrigger'), concat('/', pipeline().parameters.{FolderParameter}, '/', pipeline().parameters.{FileParameter}), '{DataPath}')" 
            : DataPath;        
    }
}