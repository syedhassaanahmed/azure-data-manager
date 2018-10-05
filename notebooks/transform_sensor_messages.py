# Databricks notebook source
dbutils.widgets.text("metadata", "")
dbutils.widgets.text("timeseries", "")
dbutils.widgets.text("output", "")

# COMMAND ----------

dfMetadata = spark.read.option("header", True).csv("/mnt" + dbutils.widgets.get("metadata"))
display(dfMetadata)

# COMMAND ----------

dfTimeseries = spark.read.json("/mnt" + dbutils.widgets.get("timeseries"))
display(dfTimeseries)

# COMMAND ----------

from pyspark.sql.functions import from_unixtime, col

dfResult = dfTimeseries.join(dfMetadata, "id") \
  .withColumn("timestamp", from_unixtime(col("ts") / 1000)) \
  .selectExpr("timestamp", "name", "value")

display(dfResult)

# COMMAND ----------

dfResult.write.mode("append").parquet("/mnt" + dbutils.widgets.get("output"))
