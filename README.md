# Azure AD Authentication Service

## Scope

* Build two Azure functions with Http triggers: `authenticate` and `helloWorld` in the same repo in ***different folders***
* Programming language: dotnet core 2.2 c#
* `az cli` one script per function to deploy as a new deployment or as an update to an existing deployment 
* Any settings/configuration required on Azure AD
* Unit tests
* Readme.md documentation describing how to deploy and test the deliverable
* Clone this repo, commit to your own repo and submit a pull request when ready for review

### Authentication Function

#### Request

* Verb: POST 
* Route: `/`
* Body:
```
{
   "username": "azure-ad email address",
   "password": "azure-ad password"
}
```

#### Response Success

* Http Code: 200
* Body:
```
{
   "token": "token representing username/password identity"
}
```

#### Response Failure

* Http Code: 401
* Body:
```
{
   "message": "Invalid username and/or password"
}
```

### helloWorld Function

Function requires authentication, not anonymous!

#### Request

* Verb: GET 
* Route: `/`
* Http Header x-token: value returned from authentication function

#### Response Success

* Http Code: 200
* Body:
```
{
   "message": "Hello @username retrived from the authentication token"
}
```

#### Response Failure

* Http Code: 401
* Body:
```
{
   "message": "Invalid or expired token"
}
```

## Azure Setup

* Proceed to Azure AD from Azure Portal
* Register a new application
* Select a native/desktop application instead of the default web application
* Copy `Application Client ID`
* Select `View API Permissions`
* Select `Grant Admin Consent`
* There is a delay until consent is granting, wait for the green icon before proceeding
* Select `Authentication` on the left hand menu
* Scroll down to `Default client type`
* Select `yes` for: `Treat application as a public client`
* Select `Save` on the top of the screen

The template deployment 'Microsoft.Web-FunctionApp-Portal-7dcb4b74-b9a2' is not valid according to the validation procedure. The tracking id is '7253776d-38e5-4d82-b8fe-f1ab9b39424a'. See inner errors for details.