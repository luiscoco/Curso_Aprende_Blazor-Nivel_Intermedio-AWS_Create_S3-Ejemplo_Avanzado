# How to manage ASW S3 buckets from Blazor Web App

## 1. Before running this application you have to comply with the following requisites

Create a AWS Free Tier account

Create a new user with Admin permission

Download and Install AWS CLI

Create an Access Key and Secret Key

Configure your AWS Account with AWS CLI (runnign the "aws configure" command)

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

This is the whole middleware (Program.cs) code:

```csharp
using BlazorAWSSample.Components;
using BlazorAWSSample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register your S3 service
builder.Services.AddSingleton<S3Service>();

// Enable detailed errors for development mode
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddServerSideBlazor()
           .AddCircuitOptions(options => { options.DetailedErrors = true; });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

## 4. Component for creating a new AWS S3 bucket (child component AWS_Create_S3.razor)

We inject the AWS S3 client service in the top of the new component

```
@inject S3Service s3Service
```

We create a new instance of AmazonS3Client for invoking the function to create the new S3 bucket:

```
var response = await s3Service.GetClient().PutBucketAsync(request); 
```

This is the new component whole code:

```csharp
﻿@using Amazon.S3
@using Amazon.S3.Model
@using BlazorAWSSample.Services
@using Microsoft.AspNetCore.Components.Forms

@inject S3Service s3Service

<!-- Form Group for Bucket Name Input -->
<div class="container mt-4">
    <div class="row mb-3">
        <div class="col-md-6">
            <label for="bucketNameInput" class="form-label">Enter S3 Bucket Name:</label>
            <input id="bucketNameInput" @bind="bucketName" placeholder="Bucket Name" class="form-control" />
        </div>
    </div>

    <!-- Button to Create S3 Bucket with Reduced Width -->
    <div class="row mb-3">
        <div class="col-md-6">
            <button @onclick="CreateS3Bucket" class="btn btn-primary w-auto">Create S3 Bucket</button>
        </div>
    </div>

    <!-- Display Success/Error Message -->
    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="row">
            <div class="col-md-6">
                @if (errorMessage == "The bucket was successfully created")
                {
                    <div class="alert alert-success">@errorMessage</div>
                }
                else
                {
                    <div class="alert alert-danger">@errorMessage</div>
                }
            </div>
        </div>
    }
</div>

@code {
    private string bucketName = "";
    private string errorMessage = "";

    [Parameter]
    public EventCallback<string> OnBucketNameCreated { get; set; }

    private async Task CreateS3Bucket()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(bucketName))
            {
                var request = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true,
                    };

                var response = await s3Service.GetClient().PutBucketAsync(request);  // Llamar a PutBucketAsync desde el cliente
                errorMessage = "The bucket was successfully created";
                await OnBucketNameCreated.InvokeAsync(bucketName);
            }
            else
            {
                errorMessage = "Bucket name cannot be empty.";
            }
        }
        catch (AmazonS3Exception ex)
        {
            errorMessage = $"Error creating S3 bucket: {ex.Message}";
        }
        catch (Exception ex)
        {
            errorMessage = $"An unexpected error occurred: {ex.Message}";
        }
    }
}
```

## 5. Component for creating a new AWS S3 bucket and Uploading files (parent component AWS_Create_S3_Upload_Files.razor)

We first invoke the child component to create a new S3 bucket

```
<AWS_Create_S3 OnBucketNameCreated="HandleBucketNameCreated"></AWS_Create_S3>
```

This is the parent component whole code:

```csharp
﻿@using Amazon.S3
@using Amazon.S3.Model
@using BlazorAWSSample.Services
@using Microsoft.AspNetCore.Components.Forms

@inject S3Service s3Service

<div class="container mt-4">
    <!-- AWS_Create_S3 component to create bucket -->
    <AWS_Create_S3 OnBucketNameCreated="HandleBucketNameCreated"></AWS_Create_S3>

    <!-- Spacer -->
    <div class="my-4"></div>

    <!-- File Upload Section -->
    <div class="row mb-3">
        <div class="col-md-6">
            <label for="fileInput" class="form-label">Upload a file:</label>
            <InputFile OnChange="OnFileSelected" class="form-control" />
        </div>
    </div>

    <!-- Display Selected File Name -->
    @if (!string.IsNullOrEmpty(fileName))
    {
        <div class="row mb-3">
            <div class="col-md-6">
                <p class="form-text">Selected file: @fileName</p>
            </div>
        </div>
    }

    <!-- Upload Button with Reduced Width -->
    <div class="row mb-3">
        <div class="col-md-6">
            <button @onclick="UploadFileToS3" class="btn btn-success w-auto" disabled="@(!fileSelected || string.IsNullOrEmpty(bucketName))">
                Upload File to S3
            </button>
        </div>
    </div>

    <!-- Display Success/Error Message -->
    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="row">
            <div class="col-md-6">
                @if (errorMessage == "The file was successfully uploaded")
                {
                    <div class="alert alert-success">@errorMessage</div>
                }
                else
                {
                    <div class="alert alert-danger">@errorMessage</div>
                }
            </div>
        </div>
    }
</div>

@code {
    private string bucketName = "";
    private string errorMessage = "";
    private IBrowserFile? selectedFile;
    private string fileName = "";
    private bool fileSelected = false;

    private void HandleBucketNameCreated(string createdBucketName)
    {
        bucketName = createdBucketName;
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
        fileName = selectedFile.Name;
        fileSelected = true;
    }

    private async Task UploadFileToS3()
    {
        try
        {
            if (selectedFile != null && !string.IsNullOrEmpty(bucketName))
            {
                var key = fileName;
                using var stream = selectedFile.OpenReadStream();

                var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = stream,
                        ContentType = selectedFile.ContentType
                    };

                var response = await s3Service.GetClient().PutObjectAsync(putRequest);
                errorMessage = "The file was successfully uploaded";
            }
            else
            {
                errorMessage = "Please select a file and provide a bucket name.";
            }
        }
        catch (AmazonS3Exception ex)
        {
            errorMessage = $"Error uploading file to S3: {ex.Message}";
        }
        catch (Exception ex)
        {
            errorMessage = $"An unexpected error occurred: {ex.Message}";
        }
    }
}
```

## 6. 

