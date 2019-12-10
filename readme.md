# Azure AD Authentication Service

## Authentication Function

### Request

* Verb: POST 
* Route: `/`
* Body:
```
{
   "username": "azure-ad email address",
   "password": "azure-ad password"
}
```

### Response Success

* Status Code: 200
* Body:
```
{
   "token": "token representing username/password identity"
}
```

### Response Invalid Username/Password

* Status Code: 401

### Response Bad Request

* Status Code: 400
* Body:
```
{
   "message": "Missing username and/or password"
}
```

## Setup on [Visual Studio Online](https://online.visualstudio.com/)

* [Reference](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-azure-function-azure-cli)
* Install [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#v2):
```
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-bionic-prod bionic main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update
sudo apt-get install azure-functions-core-tools -y
```
* Login to az cli: `az login`
* One time per Azure subscription configuration and resources:
```
namePrefix=authpoc
nameResourceGroup=${namePrefix}-resource-group
nameStorageAccount=${namePrefix}storage
location=centralus
az group create --name $nameResourceGroup --location $location
az storage account create --name $nameStorageAccount --location $location --resource-group $nameResourceGroup
```
* Create function:
```
az functionapp create --resource-group $nameResourceGroup --consumption-plan-location $location \
--name trgos-authentication --storage-account  $nameStorageAccount --runtime dotnet
```

## Development

* Clone the repo
* Modify the code
* Publish the function from AuthN folder: `func azure functionapp publish trgos-authentication`
* [Access function logs](https://markheath.net/post/three-ways-view-error-logs-azure-functions)
* Advanced logging future [options](https://stackify.com/logging-azure-functions/)

## Azure AD Setup

* Proceed to Azure AD from Azure Portal
* Register a new application: `Add your own application`
* Select a `native/desktop application` instead of the default web application
* Copy `Application Client ID` you will need it to configure the function variables
* Select `View API Permissions`
* Select `Grant Admin Consent`
* There is a delay until consent is granting, wait for the green icon before proceeding
* Select `Authentication` on the left hand menu
<!-- * Scroll down to `Default client type`
* Select `yes` for: `Treat application as a public client` -->
* Select `Certificates & Secrets` from the left hand
* Generate a new client secret and save it in a save place - you won't see it ever again

## Function Configuration

* In order to authenticate against active directory, client id and secret will be required
* These two are stored in env variables
* To configure the environment variables for existing function:
```
az functionapp config appsettings set --name trgos-authentication --resource-group authpoc-resource-group \
   --settings "AppClientID=--app client id--" \
   --settings "ClientSecret=--client-secret--"
```
* To configure [CORS](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#cors):
```
az functionapp cors remove -g authpoc-resource-group -n trgos-authentication --allowed-origins

az functionapp cors add --name trgos-authentication \
--resource-group authpoc-resource-group \
--allowed-origins "*"
```

## To test the deployment

* Create a user or two using Azure AD Portal
* Post a request as if from a server-side:
```
curl -XPOST -H "content-type:application/json" https://trgos-authentication.azurewebsites.net/api/authenticate -d '{
   "username": "email address",
   "password": "password"
}'
```
* Post a request using https://restninja.io/ (or alike) to simulate a browser request

