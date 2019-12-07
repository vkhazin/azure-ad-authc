# azure-ad-authc

## Scope

* Build two Azure functions with Http triggers: authenticate and helloWorld in the same repo in different folders

### Authentication Function

#### Request

Verb: POST 
Route: `/`
Body:
```
{
   "username": "azure-ad email address",
   "password": "azure-ad password"
}
```

#### Response Success

Http Code: 200
Body:
```
{
   "token": "token representing username/password identity"
}
```

#### Response Failure

Http Code: 401
Body:
```
{
   "message": "Invalid username and/or password"
}
```

* helloWorld will expose a single GET route `/`

