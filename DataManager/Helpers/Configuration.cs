namespace DataManager.Helpers
{
    public class Configuration
    {
        private Configuration()
        {
        }

        public const string DatabricksEndpoint = "https://westeurope.azuredatabricks.net";
        public const string DatabricksAccessToken = "dapi33674eb7bf967bc713a34a971a065f49";

        public const string AzureAdApplicationId = "6bb4d911-7812-4025-8a24-fb052ffb0a70";
        public const string AzureAdSubscriptionId = "68a5f7a3-d480-4f9c-972e-5c160bbfd830";
        public const string AzureAdTenantId = "db8e2ba9-95c1-4fbb-b558-6bf8bb1d2981";
        public const string AzureAdAuthenticationKey = "d/vTSvpjLsMz82KxwP8+v2JdqF5KS4fxq5TxolOl3ro=";

        public const string ResourceGroupName = "datalibhackfest-rg";
        public const string DataFactoryName = "schneiderhackdatafactory";

        public const string DataLakeStorageUri = "https://datalibpreview.azuredatalakestore.net/webhdfs/v1";

        public const string MongoDbServer = "10.1.12.4";
        public const string MongoDbDatabase = "wpapilotpoc";
        public const int MongoDbPort = 27017;
        public const string MongoDbUsername = "wpapilotpoc";
        public const string MongoDbPassword = "Ucg14JCFD9yoOS9KFARSpfPFdFuXeJwD7yYTSqB8EiPwTIoi6g5QyaBNFFzo1gLVeXY5RqwLLoRET60NBL7D1Q==";
        public const string MongoDbIntegrationRuntime = "integrationRuntimeMongoDBWPA";
    }
}
