# Databricks notebook source
# MAGIC %md ## This Notebook only needs to be executed once

# COMMAND ----------

dbutils.widgets.text("account", "")
dbutils.widgets.text("container", "")
dbutils.widgets.text("key", "")

# COMMAND ----------

container = dbutils.widgets.get("container")
account = dbutils.widgets.get("account")

dbutils.fs.mount(
  source = "wasbs://" + container + "@" + account + ".blob.core.windows.net",
  mount_point = "/mnt/" + container,
  extra_configs = {"fs.azure.account.key." + account + ".blob.core.windows.net": dbutils.widgets.get("key")})

# COMMAND ----------

dbutils.fs.ls("/mnt/" + container)
