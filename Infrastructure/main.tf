terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

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
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "blob"
}

resource "azurerm_storage_container" "packages" {
  name                  = "ecomeal-packages"
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "blob"
}