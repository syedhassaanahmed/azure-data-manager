# azure-data-manager

## Introduction

Organizations which produce large volumes of data are increasingly investing in exploring better ways to analyze and extract key insights from this data. These organizations face the challenge of diving into *data ponds* and often struggle to make the right dataset available to the prime beneficiaries i.e. Data Analysts/Data Scientists.

This project aims to provide a template for exploring, ingesting, transforming, analyzing and showcasing data using Azure Data platform. We've leveraged Azure Cosmos DB SQL API as storage layer for data catalog. Azure Blob Storage serves as the defacto store for all semi-structured data (e.g. JSON, CSV, Parquet files). Azure Data Factory (ADF) v2 performs the orchestration duties with Azure Databricks providing the compute for all transformation. The front-end interface of this project is an ASP.NET Core web app which reads catalog definitions and creates ADF entities using the [.NET Core SDK](https://docs.microsoft.com/en-us/azure/data-factory/quickstart-create-data-factory-dot-net).

## Architecture

![architecture.png](docs/architecture.png)

## Getting Started

In this solution our [catalog definition](DataManager.Web/SampleData) consists of 2 data sources;

a) Time series JSON files from IoT sensors

b) SQL Database Table containing Sensors Metadata

Our data pipeline simply extracts the metadata from SQL Database into tabular form, joins it with time series data and finally publishes it to a REST endpoint.

![architecture.png](docs/adf-pipeline.png)

### Configuration

For configuring the ASP.NET Core web app [please follow this document](docs/configuration.md).

### Security

To authenticate the web app we create an [Azure AD application](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-1). This application also has to be [assigned contributor role](https://docs.microsoft.com/en-us/azure/data-factory/quickstart-create-data-factory-dot-net#create-an-application-in-azure-active-directory) on the resource group so that ADF and its entities can be provisioned.

In order to reach the credentials of our data sources, ADF relies on Azure Key Vault. When an ADF resource is provisioned in Azure, a [Service Identity](https://docs.microsoft.com/en-us/azure/data-factory/data-factory-service-identity#retrieve-service-identity) is automatically generated. That Service Identity has to be granted `Get` permission in the [Key Vault access policies](https://docs.microsoft.com/en-us/azure/data-factory/store-credentials-in-key-vault#steps).

### Deployment

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fsyedhassaanahmed%2Fazure-data-manager%2Fmaster%2Fazuredeploy.json)

## TODO

- Leverage ADF [Data Flow](https://github.com/kromerm/adfdataflowdocs) for common transforms.
- Use Azure Data Lake Store [Gen2](https://docs.microsoft.com/en-us/azure/storage/data-lake-storage/introduction) as the underlying storage layer.

## Resources

- [Create a trigger that runs a pipeline in response to an event](https://docs.microsoft.com/en-us/azure/data-factory/how-to-create-event-trigger)
- [Run a Databricks notebook with the Databricks Notebook Activity in ADF](https://docs.microsoft.com/en-us/azure/data-factory/transform-data-using-databricks-notebook)

## Team

[Matthieu Lefebvre](https://www.linkedin.com/in/matthieu-lefebvre-92166728/)

[Sofiane Yahiaoui](https://www.linkedin.com/in/sofiane-yahiaoui-7006b915/)

[Igor Pagliai](https://github.com/igorpag)

[Engin Polat](https://github.com/polatengin)

[Christopher Harrison](https://github.com/GeekTrainer)

[Syed Hassaan Ahmed](https://twitter.com/hasssaaannn)