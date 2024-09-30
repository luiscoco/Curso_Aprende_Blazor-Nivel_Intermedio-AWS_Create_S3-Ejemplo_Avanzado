# How to manage ASW S3 buckets from Blazor Web App

## 1. Before running this application you have to comply with the following requisites

Create a AWS Free Tier account

Create a new user with Admin permission

Download and Install AWS CLI

Create an Access Key and Secret Key

Configure your AWS Account with AWS CLI

## 2. Create a Blazor Web App with Visual Studio 2022 Community Edition




## 3. Create a AWS S3 Client Service

We create a new folder **Services**

We create new C# class file called **S3_Service.cs**

We input this code in the **S3_Service.cs** file:

```csharp
﻿using Amazon.S3;

namespace BlazorAWSSample.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _client;

        public S3Service()
        {
            _client = new AmazonS3Client();
        }

        public IAmazonS3 GetClient()
        {
            return _client;
        }
    }
}
```

## 3. Modify the application middleware (Program.cs)

We include a Singleton Service for interacting with AWS S3

```
// Register your S3 service
builder.Services.AddSingleton<S3Service>();
```

