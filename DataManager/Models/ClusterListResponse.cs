public class ClusterListResponse
{
    public Cluster[] clusters { get; set; }
}

public class Cluster
{
    public string cluster_id { get; set; }
    public Driver driver { get; set; }
    public Executor[] executors { get; set; }
    public long spark_context_id { get; set; }
    public int jdbc_port { get; set; }
    public string cluster_name { get; set; }
    public string spark_version { get; set; }
    public Spark_Conf spark_conf { get; set; }
    public string node_type_id { get; set; }
    public string driver_node_type_id { get; set; }
    public Spark_Env_Vars spark_env_vars { get; set; }
    public int autotermination_minutes { get; set; }
    public bool enable_elastic_disk { get; set; }
    public string cluster_source { get; set; }
    public string state { get; set; }
    public string state_message { get; set; }
    public long start_time { get; set; }
    public long terminated_time { get; set; }
    public long last_state_loss_time { get; set; }
    public long last_activity_time { get; set; }
    public Autoscale autoscale { get; set; }
    public int cluster_memory_mb { get; set; }
    public float cluster_cores { get; set; }
    public Default_Tags default_tags { get; set; }
    public string creator_user_name { get; set; }
}

public class Driver
{
    public string public_dns { get; set; }
    public string node_id { get; set; }
    public Node_Aws_Attributes node_aws_attributes { get; set; }
    public string instance_id { get; set; }
    public long start_timestamp { get; set; }
    public string host_private_ip { get; set; }
    public string private_ip { get; set; }
}

public class Node_Aws_Attributes
{
    public bool is_spot { get; set; }
}

public class Spark_Conf
{
    public string sparkdatabricksdeltapreviewenabled { get; set; }
}

public class Spark_Env_Vars
{
    public string PYSPARK_PYTHON { get; set; }
}

public class Autoscale
{
    public int min_workers { get; set; }
    public int max_workers { get; set; }
}

public class Default_Tags
{
    public string Vendor { get; set; }
    public string Creator { get; set; }
    public string ClusterName { get; set; }
    public string ClusterId { get; set; }
}

public class Executor
{
    public string public_dns { get; set; }
    public string node_id { get; set; }
    public Node_Aws_Attributes1 node_aws_attributes { get; set; }
    public string instance_id { get; set; }
    public long start_timestamp { get; set; }
    public string host_private_ip { get; set; }
    public string private_ip { get; set; }
}

public class Node_Aws_Attributes1
{
    public bool is_spot { get; set; }
}
