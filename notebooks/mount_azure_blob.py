# Databricks notebook source
# MAGIC %md ## This Notebook only needs to be executed once

# COMMAND ----------

dbutils.widgets.text("account", "")
dbutils.widgets.text("container", "")
dbutils.widgets.text("key", "")

# COMMAND ----------

container = dbutils.widgets.get("container")
mountPoint = "/mnt/" + container
mounts = dbutils.fs.mounts()
mountExists = [x for x in mounts if x.mountPoint == mountPoint]

if not mountExists:
  account = dbutils.widgets.get("account")
  dbutils.fs.mount(
    source = "wasbs://" + container + "@" + account + ".blob.core.windows.net",
    mount_point = mountPoint,
    extra_configs = {"fs.azure.account.key." + account + ".blob.core.windows.net": dbutils.widgets.get("key")})
else:
  print(mountPoint + " already mounted.")

# COMMAND ----------

dbutils.fs.ls("/mnt/" + container)
