 ![.NET Core](https://github.com/trakx/dotnet-utils/workflows/.NET%20Core/badge.svg) 
 [![Codacy Badge](https://app.codacy.com/project/badge/Grade/29be1a8cd89c4e18aca52d8e96c5c91f)](https://www.codacy.com/gh/trakx/dotnet-utils/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=trakx/dotnet-utils&amp;utm_campaign=Badge_Grade) 
 [![Codacy Badge](https://app.codacy.com/project/badge/Coverage/29be1a8cd89c4e18aca52d8e96c5c91f)](https://www.codacy.com/gh/trakx/dotnet-utils/dashboard?utm_source=github.com&utm_medium=referral&utm_content=trakx/dotnet-utils&utm_campaign=Badge_Coverage)

# dotnet-utils

A .Net library meant to have no dependencies on other Trakx projects, and simply contain small utilities used repeatedly.

## AWS Parameters
In order to be able to run some integration tests you should ensure that you have access to the following AWS parameters :
```awsParams
/Trakx/Utils/Emails/EmailServiceConfiguration/SendGridApiKey
```

## Creating your local .env file
In order to be able to run some integration tests, you should create a `.env` file in the `src` folder with the following variables:
```secretsEnvVariables
EmailServiceConfiguration__SendGridApiKey=********
```
