databricks clusters create --json \
'{
    "cluster_name": "datamanager-cluster",
    "autoscale": {
        "min_workers": 1,
        "max_workers": 3
    },
    "spark_version": "4.3.x-scala2.11",
    "spark_env_vars": {
        "PYSPARK_PYTHON": "/databricks/python3/bin/python3"
    },
    "node_type_id": "Standard_D3_v2"
}'

databricks jobs create --json \
'{
    "name": "Mount Azure Storage",
    "existing_cluster_id": "1008-214810-tail467",
    "notebook_task": {
        "notebook_path": "/Shared/mount_azure_blob"
    }
}'