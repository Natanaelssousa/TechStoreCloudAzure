targetScope = 'resourceGroup'

@description('Região dos recursos.')
param location string = resourceGroup().location

@description('Identificação do projeto nas tags.')
param projectName string = 'TechStoreCloudAzure'

var storageName = 'sttechstore${uniqueString(resourceGroup().id)}'

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  tags: {
    projeto: projectName
    ambiente: 'dev'
  }
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

output storageAccountName string = storage.name
