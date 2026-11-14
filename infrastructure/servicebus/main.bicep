@description('Azure region for the Service Bus namespace.')
param location string = resourceGroup().location

@description('Globally unique Service Bus namespace name.')
param namespaceName string

@description('Service Bus namespace SKU.')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param skuName string = 'Standard'

var topicName = 'order-events'
var subscriptions = [
  'email-notification'
  'inventory-management'
  'order-audit'
]

resource namespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  sku: {
	name: skuName
	tier: skuName
  }
  properties: {
	publicNetworkAccess: 'Enabled'
	minimumTlsVersion: '1.2'
	disableLocalAuth: false
  }
}

resource topic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: namespace
  name: topicName
  properties: {
	enableBatchedOperations: true
	requiresDuplicateDetection: true
	duplicateDetectionHistoryTimeWindow: 'PT10M'
	defaultMessageTimeToLive: 'P14D'
	maxSizeInMegabytes: 1024
  }
}

resource subscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = [for subscriptionName in subscriptions: {
  parent: topic
  name: subscriptionName
  properties: {
	defaultMessageTimeToLive: 'P14D'
	deadLetteringOnMessageExpiration: true
	maxDeliveryCount: 10
	enableBatchedOperations: true
  }
}]

output fullyQualifiedNamespace string = '${namespaceName}.servicebus.windows.net'
output topic string = topicName
output subscriptionNames array = subscriptions
