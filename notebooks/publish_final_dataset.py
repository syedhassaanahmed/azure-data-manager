# Databricks notebook source
dbutils.widgets.text("finalDataset", "")

# COMMAND ----------

dfFinal = spark.read.parquet("/mnt" + dbutils.widgets.get("finalDataset"))
display(dfFinal)

# COMMAND ----------

import requests
response = requests.post("http://postb.in/api/bin")
binId = response.json()["binId"]
binId

# COMMAND ----------

finalJson = dfFinal.toJSON().collect()
response = requests.post("http://postb.in/" + binId, json=finalJson)
print("http://postb.in/api/bin/" + binId + "/req/" + response.text)
