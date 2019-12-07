# azure-ad-authc

## Scope

* Build two Azure functions with Http triggers: authenticate and helloWorld in the same repo in ***different folders***

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

### helloWorld Function

Function requires authentication, not anonymous!

#### Request

Verb: GET 
Route: `/`
Header x-token: value returned from authentication function

#### Response Success

Http Code: 200
Body:
```
{
   "message": "Hello username retrived from the authentication token"
}
```

#### Response Failure

Http Code: 401
Body:
```
{
   "message": "Invalid or expired token"
}
```
