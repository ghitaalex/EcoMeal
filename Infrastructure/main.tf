terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "rg" {
  name     = "ecomeal-resources"
  location = "Spain Central"
}

resource "azurerm_storage_account" "storage" {
  name                     = "ecomealstorage"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "businesses" {
  name                  = "ecomeal-businesses"
  storage_account_id    = azurerm_storage_account.storage.id
  container_access_type = "blob"
}

resource "azurerm_storage_container" "packages" {
  name                  = "ecomeal-packages"
  storage_account_id    = azurerm_storage_account.storage.id
  container_access_type = "blob"
}

resource "azurerm_mssql_server" "sql" {
  name                         = "ecomeal-sql"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = var.db_password
}

resource "azurerm_mssql_database" "db" {
  name                 = "ecomealdb"
  server_id            = azurerm_mssql_server.sql.id
  sku_name             = "Basic"
  max_size_gb          = 2
  storage_account_type = "Local"
}

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

variable "db_password" {
  type      = string
  sensitive = true
}

variable "mailjet_api_key" {
  type      = string
  sensitive = true
}

variable "mailjet_secret_key" {
  type      = string
  sensitive = true
}

variable "mailjet_sender_email" {
  type    = string
  default = "no-reply@ecomeal.site"
}

variable "routing_api_key" {
  type      = string
  sensitive = true
}

resource "azurerm_key_vault" "kv" {
  name                = "ecomeal-vault"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
}

resource "azurerm_key_vault_access_policy" "current_user" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = ["Get", "List", "Set", "Delete", "Purge"]
}

resource "azurerm_key_vault_access_policy" "app_service" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.app_service.identity[0].principal_id

  secret_permissions = ["Get", "List"]
}

resource "azurerm_key_vault_secret" "db_connection" {
  name         = "ConnectionStrings--DefaultConnection"
  value        = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Database=ecomealdb;User ID=sqladmin;Password=${var.db_password};Encrypt=True;TrustServerCertificate=False;"
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
}

resource "azurerm_key_vault_secret" "blob_connection" {
  name         = "ConnectionStrings--AzureBlobStorage"
  value        = azurerm_storage_account.storage.primary_connection_string
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
}

resource "azurerm_key_vault_secret" "mailjet_api_key" {
  name         = "MailjetApiKey"
  value        = var.mailjet_api_key
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
}

resource "azurerm_key_vault_secret" "mailjet_secret_key" {
  name         = "MailjetSecretKey"
  value        = var.mailjet_secret_key
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
}

resource "azurerm_key_vault_secret" "routing_api_key" {
  name         = "Routing--ApiKey"
  value        = var.routing_api_key
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [azurerm_key_vault_access_policy.current_user]
}

resource "azurerm_mssql_firewall_rule" "allow_local" {
  name             = "AllowLocalDev"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = "128.127.113.223"
  end_ip_address   = "128.127.113.223"
}

resource "azurerm_service_plan" "app_service_plan" {
  name                = "ecomeal-appserviceplan"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = "S1"
}

resource "azurerm_linux_web_app" "app_service" {
  name                = "ecomeal-app-service"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.app_service_plan.id

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }
  }

  logs {
    detailed_error_messages = false
    failed_request_tracing  = false

    http_logs {
      file_system {
        retention_in_days = 0
        retention_in_mb   = 35
      }
    }
  }

  app_settings = {
    "MailjetApiKey"      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.mailjet_api_key.versionless_id})"
    "MailjetSecretKey"   = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.mailjet_secret_key.versionless_id})"
    "MailjetSenderEmail" = var.mailjet_sender_email
    "Routing__ApiKey"    = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.routing_api_key.versionless_id})"
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLServer"
    value = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.db_connection.versionless_id})"
  }

  connection_string {
    name  = "AzureBlobStorage"
    type  = "Custom"
    value = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.blob_connection.versionless_id})"
  }
}

resource "azurerm_linux_web_app" "client_service" {
  name                = "ecomeal-client"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.app_service_plan.id

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }
  }
}
