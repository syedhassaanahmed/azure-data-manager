# Configuration

The ASP.NET Core web app has the following configuration schema.
> **Hint:** Use the .NET Core [Secret Manager tool](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=windows) to store the configuration.

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<Azure AD Tenant ID>",
    "TenantId": "<Azure AD Tenant ID>",
    "ClientId": "<Azure AD Application ID>",
    "CallbackPath": "/signin-oidc",
    "ClientSecret": "<Azure AD Application Secret>"
  },
  "CosmosDb": {
    "Endpoint": "https://<Cosmos DB Account Name>.documents.azure.com:443/",
    "PrimaryKey": "<Cosmos DB Primary Key>",
    "Database": "<Cosmos DB database name>",
    "DefaultThroughput": 400
  },
  "DataFactory": {
    "SubscriptionId": "<Azure Subscription ID in which ADF exists>",
    "ResourceGroup": "<Resource Group in which ADF exists>",
    "Name": "<Name of the ADF resource>"
  },
  "KeyVault": {
    "Name": "<Name of the Azure Key Vault>"
  },
  "StorageAccount": {
    "Name": "<Name of Azure Storage Account to be used for all semi-structured data>"
  },
  "Databricks": {
    "Endpoint": "https://<Azure region>.azuredatabricks.net",
    "KeyVaultSecretName": "<Name of the Azure Key Vault secret which contains Databricks authentication token>",
    "ExistingClusterId": "<Cluster ID of the Databricks cluster to be used for transformations>"
  }
}
```