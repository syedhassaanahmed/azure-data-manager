#!/bin/bash

# Environment variables with default values
RESOURCE_GROUP=${RESOURCE_GROUP:=data-manager}
RESOURCE_GROUP_LOCATION=${RESOURCE_GROUP_LOCATION:=westeurope}
AD_APP_NAME=${AD_APP_NAME:=data-manager}
AD_APP_PASSWORD=${AD_APP_PASSWORD:=MyStrongADPaSSw0rd}
STORAGE_ACCOUNT_PREFIX=${STORAGE_ACCOUNT_PREFIX:=datamanager}
KEY_VAULT_NAME=${KEY_VAULT_NAME:=datamanager-kv}
DATABRICKS_CLUSTER_NAME=${DATABRICKS_CLUSTER_NAME:=datamanager-cluster}
DATABRICKS_SECRET_NAME=${DATABRICKS_SECRET_NAME:=databricks}
SQL_SERVER_NAME=${SQL_SERVER_NAME:=datamanager-sqlserver}
SQL_DB_NAME=${SQL_DB_NAME:=datamanager-sqldb}
SQL_ADMIN_USER=${SQL_ADMIN_USER:=datamanager-sqluser}
SQL_ADMIN_PASSWORD=${SQL_ADMIN_PASSWORD:='<YourStrong!Passw0rd>'}
WEB_APP_NAME=${WEB_APP_NAME:=datamanager-web}

# Create Azure AD Application for the Web App if needed
AD_APP_ID=$(az ad app list --display-name $AD_APP_NAME --query "[?displayName=='$AD_APP_NAME'].appId" -o tsv)
LOCAL_WEBSITE=https://localhost:44338

if [ -z "$AD_APP_ID" ]; then
    AD_APP_ID=$(az ad app create --display-name $AD_APP_NAME --native-app false \
        --identifier-uris https://localhost/$(uuidgen) \
        --homepage $LOCAL_WEBSITE \
        --password $AD_APP_PASSWORD \
        --query="appId" -o tsv)
fi

# Add Reply URLs to the AD App for Oauth flow
AZURE_WEBSITE=https://$WEB_APP_NAME.azurewebsites.net
REPLY_URLS="$LOCAL_WEBSITE $LOCAL_WEBSITE/signin-oidc $AZURE_WEBSITE $AZURE_WEBSITE/signin-oidc"
az ad app update --id $AD_APP_ID --reply-urls $REPLY_URLS

# Create Service Principal for the above AD App if needed.
AD_SP_ID=$(az ad sp list --display-name $AD_APP_NAME --query "[?appId=='$AD_APP_ID'].objectId" -o tsv)
if [ -z "$AD_SP_ID" ]; then
    AD_SP_ID=$(az ad sp create --id $AD_APP_ID --query "objectId" -o tsv)
fi

# Create Resource Group if needed
RG_EXISTS=$(az group exists -n $RESOURCE_GROUP -o tsv)
if [[ "$RG_EXISTS" != true ]]; then
  az group create -n $RESOURCE_GROUP -l $RESOURCE_GROUP_LOCATION
fi

STORAGE_ACCOUNT_CONTAINER=data-manager

# Deploy ARM Template
az group deployment create -g $RESOURCE_GROUP --template-file azuredeploy.json --parameters \
    storageAccountPrefix=$STORAGE_ACCOUNT_PREFIX \
    storageAccountContainer=$STORAGE_ACCOUNT_CONTAINER \
    keyVaultName=$KEY_VAULT_NAME \
    databricksSecretName=$DATABRICKS_SECRET_NAME \
    sqlServerName=$SQL_SERVER_NAME \
    sqlDBName=$SQL_DB_NAME \
    sqlAdminUser=$SQL_ADMIN_USER \
    sqlAdminPassword=$SQL_ADMIN_PASSWORD \
    azureADClientID=$AD_APP_ID \
    azureADClientSecret=$AD_APP_PASSWORD \
    webAppName=$WEB_APP_NAME \
    -o none

# Configure Databricks Token
databricks configure --token
DATABRICKS_TOKEN=$(grep token ~/.databrickscfg | awk -F ' = ' '{print $2}')

# Create Databricks Cluster if needed
CLUSTER_EXISTS=$(databricks clusters list | tr -s " " | cut -d" " -f2 | grep ^${DATABRICKS_CLUSTER_NAME}$)
if [ -z "$CLUSTER_EXISTS" ]; then
    BODY_CREATE_CLUSTER=$(cat << EOF
    {
        "cluster_name": "$DATABRICKS_CLUSTER_NAME",
        "autoscale": {
            "min_workers": 1,
            "max_workers": 3
        },
        "spark_version": "4.3.x-scala2.11",
        "spark_env_vars": {
            "PYSPARK_PYTHON": "/databricks/python3/bin/python3"
        },
        "autotermination_minutes": 120,
        "node_type_id": "Standard_D3_v2"
    }
EOF
    )
    databricks clusters create --json "$BODY_CREATE_CLUSTER"
fi

DATABRICKS_CLUSTER_ID=$(databricks clusters list | awk '/'$DATABRICKS_CLUSTER_NAME'/ {print $1}')

# Create Storage Container
STORAGE_ACCOUNT_NAME=$(az storage account list -g $RESOURCE_GROUP --query "[?starts_with(name, '$STORAGE_ACCOUNT_PREFIX')].name" -o tsv)
STORAGE_ACCOUNT_KEY=$(az storage account keys list -g $RESOURCE_GROUP -n $STORAGE_ACCOUNT_NAME --query "[0].value" -o tsv)

# Import all Databricks Notebooks
databricks workspace import_dir -o notebooks /Shared

# Mount Storage Container in DBFS
BODY_CREATE_JOB_RUN=$(cat << EOF
{
    "run_name": "Mount Azure Storage",
    "existing_cluster_id": "$DATABRICKS_CLUSTER_ID",
    "notebook_task": {
        "notebook_path": "/Shared/mount_azure_blob",
        "base_parameters": {
            "account": "$STORAGE_ACCOUNT_NAME",
            "container": "$STORAGE_ACCOUNT_CONTAINER",
            "key": "$STORAGE_ACCOUNT_KEY"
        }
    }
}
EOF
)
databricks runs submit --json "$BODY_CREATE_JOB_RUN"

# Set Databricks Cluster ID in Web App Settings
az webapp config appsettings set -g $RESOURCE_GROUP -n $WEB_APP_NAME --settings Databricks:ExistingClusterId=$DATABRICKS_CLUSTER_ID -o none

# Add Databricks Token in KeyVault
USER_PRINCIPAL_NAME=$(az account show --query "user.name" -o tsv)
az keyvault set-policy -n $KEY_VAULT_NAME --upn $USER_PRINCIPAL_NAME --secret-permissions get set list
az keyvault secret set --vault-name $KEY_VAULT_NAME --name $DATABRICKS_SECRET_NAME --value $DATABRICKS_TOKEN -o none

# Upload 2018-10-04 blobs to the container/input folder
az storage blob upload-batch --account-name $STORAGE_ACCOUNT_NAME --account-key $STORAGE_ACCOUNT_KEY -d $STORAGE_ACCOUNT_CONTAINER \
    -s sample-data/ --pattern messages_20181004T*.json --destination-path input

# Allow current public IP in SQL Server firewall rule
PUBLIC_IP=$(curl ipinfo.io/ip)
az sql server firewall-rule create -g $RESOURCE_GROUP -s $SQL_SERVER_NAME -n my-public-ip-rule --start-ip-address $PUBLIC_IP --end-ip-address $PUBLIC_IP

# Populate Sensors Table in SQL DB
sqlcmd -S tcp:$SQL_SERVER_NAME.database.windows.net,1433 -d $SQL_DB_NAME -U datamanager-sqluser -P $SQL_ADMIN_PASSWORD -N -i sample-data/ddl.sql

# Assign Servince Principal to the Resource Group so that the Web App can provision Data Factory resources (e.g. linked services, datasets, activities and pipelines)
# Reason for doing this so late in the script is this issue https://github.com/Azure/azure-powershell/issues/2286
ASSIGNMENT_EXISTS=$(az role assignment list --resource-group $RESOURCE_GROUP --query="[?principalId=='$AD_SP_ID'].id" -o tsv)
if [ -z "$ASSIGNMENT_EXISTS" ]; then
    az role assignment create --role "Contributor" --assignee $AD_SP_ID --resource-group $RESOURCE_GROUP
fi