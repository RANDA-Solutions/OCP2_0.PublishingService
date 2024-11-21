## Azure Function App Environment Settings

### Application Settings

| Key                                        | Value                                                                                              | Description |
|--------------------------------------------|----------------------------------------------------------------------------------------------------|-------------|
| `AccessKeyUrl`                             | `https://opencredentialpublisher.org/credentials/connect?Issuer={0}&Scope={1}&Method={2}&Endpoint={3}&Payload={4}` | URL used for accessing credential data with specified parameters. |
| `AppBaseUri`                               | `https://api.opencredentialpublisher.org`                                                          | Base URI for the application API. |
| `APPINSIGHTS_INSTRUMENTATIONKEY`           | `<APPLICATION_INSIGHTS_KEY>`                                                                       | Key used to configure Azure Application Insights for monitoring and diagnostics. |
| `ApplicationDbConnectionString`            | `Server=tcp:<SQL_SERVER_NAME>.database.windows.net,1433;Initial Catalog=<DB_NAME>;Persist Security Info=False;User ID=<DB_USER>;Password=<DB_PASSWORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` | Connection string for the application's SQL database.  This should be the same as what is used by the API. |
| `APPLICATIONINSIGHTS_CONNECTION_STRING`    | `InstrumentationKey=<APPLICATION_INSIGHTS_KEY>;IngestionEndpoint=https://<REGION>.in.applicationinsights.azure.com/` | Connection string for Azure Application Insights, including the ingestion endpoint. |
| `AzureBlob__StorageConnectionString`       | `DefaultEndpointsProtocol=https;AccountName=<STORAGE_ACCOUNT_NAME>;AccountKey=<STORAGE_ACCOUNT_KEY>;EndpointSuffix=core.windows.net` | Connection string for Azure Blob Storage used by the application.  This should be the same as what is used by the API. |
| `AzureKeyVault__AzureAppClientId`          | `<AZURE_APP_CLIENT_ID>`                                                                            | Client ID for accessing Azure Key Vault. |
| `AzureKeyVault__AzureAppClientSecret`      | `<AZURE_APP_CLIENT_SECRET>`                                                                        | Client secret for accessing Azure Key Vault. |
| `AzureKeyVault__KeyVaultBaseUri`           | `https://<KEYVAULT_NAME>.vault.azure.net/`                                                         | Base URI for Azure Key Vault where secrets are stored. |
| `AzureQueue__StorageConnectionString`      | `DefaultEndpointsProtocol=https;AccountName=<STORAGE_ACCOUNT_NAME>;AccountKey=<STORAGE_ACCOUNT_KEY>;EndpointSuffix=core.windows.net` | Connection string for Azure Queue Storage used by the application. |
| `AzureWebJobs.PublishPackageClrQueueTrigger.Disabled` | `0`                                                                                               | Indicates whether the package publish queue trigger for Azure WebJobs is disabled (0 = enabled). |
| `AzureWebJobs.PublishSignPackageQueueTrigger.Disabled` | `0`                                                                                               | Indicates whether the sign package queue trigger for Azure WebJobs is disabled (0 = enabled). |
| `AzureWebJobsStorage`                      | `DefaultEndpointsProtocol=https;AccountName=<STORAGE_ACCOUNT_NAME>;AccountKey=<STORAGE_ACCOUNT_KEY>;EndpointSuffix=core.windows.net` | Connection string for storage used by Azure WebJobs. |
| `FUNCTIONS_EXTENSION_VERSION`              | `~4`                                                                                              | Specifies the version of the Azure Functions runtime to use. |
| `FUNCTIONS_WORKER_RUNTIME`                 | `dotnet`                                                                                          | Indicates the runtime used for Azure Functions (e.g., dotnet, node, python). |
| `QueueConnectionString`                    | `DefaultEndpointsProtocol=https;AccountName=<STORAGE_ACCOUNT_NAME>;AccountKey=<STORAGE_ACCOUNT_KEY>;EndpointSuffix=core.windows.net` | Connection string for Azure Queue Storage.  This should be same connection string as used by the API. |
| `WEBSITE_ENABLE_SYNC_UPDATE_SITE`          | `true`                                                                                            | Indicates whether to enable site synchronization updates. |
| `WEBSITE_RUN_FROM_PACKAGE`                 | `1`                                                                                               | Indicates whether the website should run from a package file. |

Feel free to adjust these descriptions to better suit your needs! If there's anything else you need help with, just let me know.
