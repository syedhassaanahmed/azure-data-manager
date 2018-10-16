# Deployment

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fsyedhassaanahmed%2Fazure-data-manager%2Fmaster%2Fazuredeploy.json)

The above button uses [ARM template](../azuredeploy.json) to deploy the necessary resources in the resource group. The ARM template however **cannot** deploy the following;

- Azure AD Application
- Databricks Cluster
- [Databricks Notebooks](../notebooks)
- [Mount Azure Blob Storage containers with DBFS](https://docs.azuredatabricks.net/spark/latest/data-sources/azure/azure-storage.html#mount-azure-blob-storage)
- Key Vault secret for Databricks authentication token
- [SQL Database Table](../sample-data/ddl.sql)
- [Files in Azure Blob container](../sample-data)

## Setup Script

In order to address the above, we've created a setup bash script - [deploy.sh](../setup.sh)

### Prerequisites

- [Azure CLI 2.x](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
- [Databricks CLI](https://docs.databricks.com/user-guide/dev-tools/databricks-cli.html#install-the-cli)
- [sqlcmd](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup-tools?view=sql-server-2017)

### Environment Variables

The script uses environment variables to parameterize the deployment. They can be overwritten like this prior to executing the script;

```bash
export RESOURCE_GROUP=data-manager
export AD_APP_PASSWORD=MyStrongADPaSSw0rd
...
```