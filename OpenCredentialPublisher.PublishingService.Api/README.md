## Azure Function App Environment Settings

### Application Settings

| Key                                        | Value                                                                                              | Description |
|--------------------------------------------|----------------------------------------------------------------------------------------------------|-------------|
| `AccessKeyUrl`                             | `https://opencredentialpublisher.org/credentials/connect`                                          | URL of Wallet used for accessing credential data. No longer used by newer versions of the wallet. |
| `AppBaseUri`                               | `https://api.opencredentialpublisher.org`                                                          | Base URI for the application API. |
| `APPINSIGHTS_INSTRUMENTATIONKEY`           | `<APPLICATION_INSIGHTS_KEY>`                                                                       | Key used to configure Azure Application Insights for monitoring and diagnostics. |
| `ApplicationDbConnectionString`            | `Server=tcp:<SQL_SERVER_NAME>.database.windows.net,1433;Initial Catalog=<DB_NAME>;Persist Security Info=False;User ID=<DB_USER>;Password=<DB_PASSWORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` | Connection string for the application's SQL database. Shared by the function app. |
| `APPLICATIONINSIGHTS_CONNECTION_STRING`    | `InstrumentationKey=<APPLICATION_INSIGHTS_KEY>;IngestionEndpoint=https://<REGION>.in.applicationinsights.azure.com/` | Connection string for Azure Application Insights, including the ingestion endpoint. |
| `ApplicationInsightsAgent_EXTENSION_VERSION`| `~2`                                                                                               | Specifies the version of the Application Insights Agent extension to use. |
| `AzureBlob__StorageConnectionString`       | `DefaultEndpointsProtocol=https;AccountName=<STORAGE_ACCOUNT_NAME>;AccountKey=<STORAGE_ACCOUNT_KEY>;EndpointSuffix=core.windows.net` | Connection string for Azure Blob Storage used by the application.  Shared with function app. |
| `AzureKeyVault__AzureAppClientId`          | `<AZURE_APP_CLIENT_ID>`                                                                            | Client ID for accessing Azure Key Vault. |
| `AzureKeyVault__AzureAppClientSecret`      | `<AZURE_APP_CLIENT_SECRET>`                                                                        | Client secret for accessing Azure Key Vault. |
| `AzureKeyVault__AzureTenantId`             | `<AZURE_TENANT_ID>`                                                                                | Tenant ID for accessing Azure Key Vault. |
| `AzureKeyVault__CertificateName`           | `<CERTIFICATE_NAME>`                                                                               | Name of the certificate used in Azure Key Vault by IdentityServer4 for signing purposes. |
| `AzureKeyVault__KeyVaultBaseUri`           | `https://<KEYVAULT_NAME>.vault.azure.net/`                                                         | Base URI for Azure Key Vault where secrets are stored. |
| `AzureQueue__StorageConnectionString`      | `DefaultEndpointsProtocol=https;AccountName=<STORAGE_ACCOUNT_NAME>;AccountKey=<STORAGE_ACCOUNT_KEY>;EndpointSuffix=core.windows.net` | Connection string for Azure Queue Storage used by the application. Shared with function app. |
| `DIAGNOSTICS_AZUREBLOBCONTAINERSASURL`     | `<BLOB_CONTAINER_SAS_URL>`                                                                         | SAS URL for the Azure Blob container used for diagnostics logging. |
| `DIAGNOSTICS_AZUREBLOBRETENTIONINDAYS`     | `3`                                                                                                | Number of days to retain logs in the Azure Blob container. |
| `IdentityDbConnectionString`               | `Server=tcp:<SQL_SERVER_NAME>.database.windows.net,1433;Initial Catalog=<DB_NAME>;Persist Security Info=False;User ID=<DB_USER>;Password=<DB_PASSWORD>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` | Connection string for the identity database. |
| `WEBSITE_HTTPLOGGING_RETENTION_DAYS`       | `1`                                                                                                | Number of days to retain HTTP logs. |
| `WEBSITE_LOAD_CERTIFICATES`                | `<CERTIFICATE_THUMBPRINT>`                                                                         | Certificate thumbprint for the signing certificate used by IdentityServer4 to be loaded by the website. |
| `WEBSITE_LOAD_USER_PROFILE`                | `1`                                                                                                | Indicates whether to load the user profile. |
| `WEBSITE_RUN_FROM_PACKAGE`                 | `1`                                                                                                | Indicates whether the website should run from a package file. |
| `XDT_MicrosoftApplicationInsights_Mode`    | `default`                                                                                          | Specifies the mode for the Application Insights extension. |

