#!/bin/bash

# Define parameters with default values
RESOURCE_GROUP=${RESOURCE_GROUP:=syah-data-manager}
STORAGE_ACCOUNT_PREFIX=${STORAGE_ACCOUNT_PREFIX:=datamanager}
SQL_ADMIN_PASSWORD=${SQL_ADMIN_PASSWORD:='<YourStrong!Passw0rd>'}
DATABRICKS_CLUSTER_NAME=${DATABRICKS_CLUSTER_NAME:=datamanager-cluster}

# Create resource group if needed
RG_LOCATION=westeurope
RG_EXISTS=$(az group exists -n $RESOURCE_GROUP -o tsv)
if [[ "$RG_EXISTS" != true ]]; then
  az group create -l $RG_LOCATION -n $RESOURCE_GROUP
fi

# Deploy ARM Template
#az group deployment create -g $RESOURCE_GROUP --template-file azuredeploy.json --parameters sqlAdminPassword=$SQL_ADMIN_PASSWORD

# Configure Databricks Token
DATABRICKS_CONFIG=~/.databrickscfg
if [[ ! -f "$DATABRICKS_CONFIG" ]]; then
  databricks configure --token
fi
DATABRICKS_TOKEN=$(grep token $DATABRICKS_CONFIG | awk -F ' = ' '{print $2}')

# Create Databricks Cluster if needed
CLUSTER_EXISTS=$(databricks clusters list | tr -s " " | cut -d" " -f2 | grep ^${DATABRICKS_CLUSTER_NAME}$)
if [[ -n $CLUSTER_EXISTS ]]; then
    echo "Cluster $DATABRICKS_CLUSTER_NAME already exists."
else
    echo "Creating new cluster $DATABRICKS_CLUSTER_NAME..."

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

CLUSTER_ID=$(databricks clusters list | awk '/'$DATABRICKS_CLUSTER_NAME'/ {print $1}')

# Create Storage Container
STORAGE_ACCOUNT_NAME=$(az storage account list -g $RESOURCE_GROUP --query "[?starts_with(name, '$STORAGE_ACCOUNT_PREFIX')].name" -o tsv)
STORAGE_ACCOUNT_KEY=$(az storage account keys list -g $RESOURCE_GROUP -n $STORAGE_ACCOUNT_NAME --query "[0].value" -o tsv)
STORAGE_ACCOUNT_CONTAINER=data-manager

az storage container create --account-name $STORAGE_ACCOUNT_NAME --account-key $STORAGE_ACCOUNT_KEY --name $STORAGE_ACCOUNT_CONTAINER

# Import all Databricks Notebooks
databricks workspace import_dir -o notebooks /Shared

# Mount Storage Container in DBFS
BODY_CREATE_JOB_RUN=$(cat << EOF
{
    "run_name": "Mount Azure Storage",
    "existing_cluster_id": "$CLUSTER_ID",
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