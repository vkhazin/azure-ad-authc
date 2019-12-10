Azure Setup

## Deployment

### Create resource group and storage account

Replace `<resource_group>`, `<storage_name>` (and `location`, if necessary) with your values: 

    az group create --name <resource_group> --location northeurope
    az storage account create --name <storage_name> --location northeurope --resource-group <resource_group> --sku Standard_LRS
    
### Create functions

Replace `<funcNameAuthenticate>` and `<funcNameHelloWorld>` with your values:

    az functionapp create --resource-group <resource_group> --consumption-plan-location northeurope \
    --name <funcNameAuthenticate> --storage-account <storage_name> --runtime dotnet

    az functionapp create --resource-group <resource_group> --consumption-plan-location northeurope \
    --name <funcNameHelloWorld> --storage-account <storage_name> --runtime dotnet

### Settings

Settings are required for `authenticate` function only:

* Proceed to Function App from Azure Portal
* Select `<funcNameAuthenticate>`
* Click **Configuration** under Configured features on the Overview tab
* Use **New Application Setting** button to add `ClientId` and `DirectoryName` settings

### Deploy

Execute from each function folders:

```
func azure functionapp publish <funcNameAuthenticate>
```

```
func azure functionapp publish <funcNameHelloWorld>
```

## Running Local

### local.settings.json

Create `local.settings.json` files in the function folders.
For `authenticate` function settings file should include ClientId and DirectoryName:

    {
      "IsEncrypted": false,
      "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "ClientId": "00000000-aaaa-bbbb-cccc-dddddddddddd",
        "DirectoryName": "zzzzzz.onmicrosoft.com"
      }
    }
    
For `helloWorld` function:

    {
      "IsEncrypted": false,
      "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"
      }
    }

### Run

    cd authenticate
    func start --build

To run `helloWorld` at the same time use different port:
    
    cd ../helloWorld
    func start --build -p 7072
