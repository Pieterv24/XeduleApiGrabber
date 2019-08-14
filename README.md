# AiXeduleApiGrabber
A program to grab schedules from the XeduleAPI and offer them on a API endpoint that produces an ICal file

### Requirements to build
- .net core SDK 2.1+

### Setup with .net core CLI
`cd XeduleApiGrabber`
`dotnet restore`

### Setup with Visual Studio
simply open the .sln file with visual studio

### Provide credentials for Xedule api
For the api grabber to function credentials for xedule are required.

#### Get credentials
1. log into xedule with an account that has access to the schedules you want to make available
2. open the developer console of your browser
3. go to the cookie explorer of your developer console
4. note down the value of `ASP.NET_SessionId` as `SessionID` and `User` as `User` you'll need them later

#### Set credentials
to set credentials on starup of the application enter the noted down credentials into the `appsettings.Development.json` file

to set credentials while the application is running use the `/api/credentials/setcredentials` endpoint with fields `User` and `Session`

### Known issues
- the api sometimes randomly invilidates the session. the only present workaround is resubmitting valid credentials to the `setcredentials` endpoint
