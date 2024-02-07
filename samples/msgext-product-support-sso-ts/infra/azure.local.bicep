@maxLength(20) 

@minLength(4) 

@description('Used to generate names for all resources in this file') 

param resourceBaseName string 

 

@description('Required when create Azure Bot service') 

param botAadAppClientId string 

@maxLength(42) 

param botDisplayName string 

param botAppDomain string 
param graphEntraAppClientId string 

@secure() 

param graphEntraAppClientSecret string 

param connectionName string 

 

module azureBotRegistration './botRegistration/azurebot.bicep' = { 

  name: 'Azure-Bot-registration' 

  params: { 

    resourceBaseName: resourceBaseName 

    botAadAppClientId: botAadAppClientId 

    botAppDomain: botAppDomain 

    botDisplayName: botDisplayName 
    graphEntraAppClientId: graphEntraAppClientId 

    graphEntraAppClientSecret: graphEntraAppClientSecret 

    connectionName: connectionName 

  } 

} 