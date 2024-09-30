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

We define the **S3_Service.cs** with the following code:

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

This is the whole middleware (**Program.cs**) code:

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

```razor
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

If we run the application we see this component output:

![image](https://github.com/user-attachments/assets/daeee6f3-7601-4a39-8855-306cac9002c0)

We can verify in AWS console we created a new S3 bucket

![image](https://github.com/user-attachments/assets/e63d3aae-2b2e-4396-ab73-2361049e3fec)

![image](https://github.com/user-attachments/assets/c0636e3c-03e0-48f2-815c-c3787ba9226b)

## 5. Component for creating a new AWS S3 bucket and Uploading files (parent component AWS_Create_S3_Upload_Files.razor)

We first invoke the child component to create a new S3 bucket

```
<AWS_Create_S3 OnBucketNameCreated="HandleBucketNameCreated"></AWS_Create_S3>
```

This is the parent component whole code:

```razor
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

If we run the application we can navigate to this component for creating S3 buckets and upload files

![image](https://github.com/user-attachments/assets/afbf47b8-c914-4cb8-8551-90229a86fddb)


## 6. Component for listing S3 buckets in AWS Account

We use the S3Service and with the following code we list all the S3 buckets:

```
var response = await s3Service.GetClient().ListBucketsAsync();
```

This is the new component whole code:

```razor
@page "/ListS3Buckets"

@using BlazorAWSSample.Services
@using Amazon.S3.Model

@inject S3Service s3Service

<h3>S3 Buckets List</h3>

@if (buckets == null)
{
    <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
}
else if (buckets.Count == 0)
{
    <div class="alert alert-info" role="alert">
        No S3 Buckets found in this account.
    </div>
}
else
{
    <table class="table table-striped table-bordered">
        <thead class="table-dark">
            <tr>
                <th>Bucket Name</th>
                <th>Creation Date</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var bucket in buckets)
            {
                <tr>
                    <td>@bucket.BucketName</td>
                    <td>@bucket.CreationDate.ToString("yyyy-MM-dd HH:mm:ss")</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<S3Bucket> buckets;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await s3Service.GetClient().ListBucketsAsync();
            buckets = response.Buckets;
        }
        catch (Exception ex)
        {
            // Log error or show a user-friendly message
            Console.WriteLine($"Error fetching buckets: {ex.Message}");
        }
    }
}
```

We run the application and we navigate to this component to see all the S3 buckets in my AWS account

![image](https://github.com/user-attachments/assets/cbfb5014-d731-4fdb-90dc-0d044d9ad8a0)

## 7. Create a component for managing S3 buckets and files from Blazor Web App

This component include some features:

List S3 buckets in AWS Account

Delete S3 bucket

List files inside S3 bucket

Upload file to S3 bucket

View file

Download file

Delete file

This is the component whole code:

```razor
@page "/ListS3AndFilesBuckets"

@using BlazorAWSSample.Services
@using Amazon.S3.Model
@using Amazon.S3
@using Microsoft.JSInterop
@using Microsoft.AspNetCore.Components.Forms

@inject S3Service s3Service
@inject IJSRuntime jsRuntime

<h3>S3 Buckets List</h3>

<p>@statusMessage</p> <!-- Este párrafo mostrará los mensajes de estado -->
@if (buckets == null)
{
    <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
}
else if (buckets.Count == 0)
{
    <div class="alert alert-info" role="alert">
        No S3 Buckets found in this account.
    </div>
}
else
{
    <table class="table table-striped table-bordered">
        <thead class="table-dark">
            <tr>
                <th>Bucket Name</th>
                <th>Creation Date</th>
                <th>Select</th>
                <th>Upload File</th>
                <th>Delete</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var bucket in buckets)
            {
                <tr>
                    <td>@bucket.BucketName</td>
                    <td>@bucket.CreationDate.ToString("yyyy-MM-dd HH:mm:ss")</td>
                    <td>
                        <button class="btn btn-primary" @onclick="() => SelectBucket(bucket.BucketName)">
                            View Files
                        </button>
                    </td>
                    <td>
                        <InputFile OnChange="@(e => OnFileSelected(e, bucket.BucketName))" />
                        <button class="btn btn-success" @onclick="() => UploadFile(bucket.BucketName)">
                            Upload
                        </button>
                    </td>
                    <td>
                        <button class="btn btn-danger" @onclick="() => DeleteBucket(bucket.BucketName)">
                            Delete Bucket
                        </button>
                    </td>
                </tr>
            }
        </tbody>
    </table>

    @if (selectedBucket != null)
    {
        <h4>Files in '@selectedBucket'</h4>
        @if (files == null)
        {
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        }
        else if (files.Count == 0)
        {
            <div class="alert alert-info" role="alert">
                No files found in this bucket.
            </div>
        }
        else
        {
            <table class="table table-striped table-bordered">
                <thead class="table-dark">
                    <tr>
                        <th>File Name</th>
                        <th>Size (Bytes)</th>
                        <th>Last Modified</th>
                        <th>View</th>
                        <th>Download</th>
                        <th>Delete</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var file in files)
                    {
                        <tr>
                            <td>@file.Key</td>
                            <td>@file.Size</td>
                            <td>@file.LastModified.ToString("yyyy-MM-dd HH:mm:ss")</td>
                            <td>
                                <button class="btn btn-info" @onclick="() => ViewFile(file.Key)">
                                    View
                                </button>
                            </td>
                            <td>
                                <button class="btn btn-success" @onclick="() => DownloadFile(file.Key)">
                                    Download
                                </button>
                            </td>
                            <td>
                                <button class="btn btn-danger" @onclick="() => DeleteFile(file.Key)">
                                    Delete
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        }
    }

    @if (!string.IsNullOrEmpty(deletionWarningMessage))
    {
        <div class="alert alert-warning" role="alert">
            @deletionWarningMessage
        </div>
    }
}

@code {
    private List<S3Bucket> buckets;
    private List<S3Object> files;
    private string selectedBucket;
    private string deletionWarningMessage;
    private IBrowserFile selectedFile;
    private string statusMessage; // Variable para los mensajes de estado

    protected override async Task OnInitializedAsync()
    {
        await LoadBuckets();
    }

    private async Task LoadBuckets()
    {
        try
        {
            var response = await s3Service.GetClient().ListBucketsAsync();
            buckets = response.Buckets;
            statusMessage = "S3 buckets loaded successfully."; // Mensaje de éxito
        }
        catch (Exception ex)
        {
            statusMessage = $"Error fetching buckets: {ex.Message}"; // Mensaje de error
        }
    }

    private async Task SelectBucket(string bucketName)
    {
        selectedBucket = bucketName;
        files = null; // Reinicia la lista de archivos antes de cargar nuevos datos
        statusMessage = $"Selected bucket: {bucketName}"; // Actualizar mensaje cuando se selecciona un bucket

        try
        {
            var request = new ListObjectsV2Request
                {
                    BucketName = selectedBucket
                };
            var response = await s3Service.GetClient().ListObjectsV2Async(request);
            files = response.S3Objects;
            statusMessage = $"Files in bucket '{bucketName}' loaded successfully."; // Mensaje de éxito
        }
        catch (Exception ex)
        {
            statusMessage = $"Error fetching files: {ex.Message}"; // Mensaje de error
        }
    }

    private void OnFileSelected(InputFileChangeEventArgs e, string bucketName)
    {
        selectedFile = e.File;
        selectedBucket = bucketName; // Asigna el bucket seleccionado para subir el archivo
        statusMessage = $"Selected file: {selectedFile.Name} for bucket: {bucketName}"; // Actualizar mensaje cuando se selecciona un archivo
    }

    private async Task UploadFile(string bucketName)
    {
        if (selectedFile == null)
        {
            statusMessage = "No file selected for upload."; // Mensaje de error si no hay archivo seleccionado
            return;
        }

        try
        {
            var stream = selectedFile.OpenReadStream();
            var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = selectedFile.Name,
                    InputStream = stream
                };

            await s3Service.GetClient().PutObjectAsync(request);
            statusMessage = $"File '{selectedFile.Name}' uploaded to bucket '{bucketName}' successfully."; // Mensaje de éxito

            // Recarga los archivos después de la subida
            await SelectBucket(bucketName);
        }
        catch (AmazonS3Exception ex)
        {
            statusMessage = $"Error uploading file: {ex.Message}"; // Mensaje de error
        }
        catch (Exception ex)
        {
            statusMessage = $"Error uploading file: {ex.Message}"; // Mensaje de error
        }
    }

    private async Task DeleteBucket(string bucketName)
    {
        deletionWarningMessage = string.Empty; // Limpia cualquier advertencia previa

        try
        {
            var listResponse = await s3Service.GetClient().ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = bucketName
                });

            if (listResponse.S3Objects.Count > 0)
            {
                deletionWarningMessage = $"The bucket '{bucketName}' contains files. Please delete all files before deleting the bucket.";
                return;
            }

            await s3Service.GetClient().DeleteBucketAsync(new DeleteBucketRequest
                {
                    BucketName = bucketName
                });

            statusMessage = $"Bucket '{bucketName}' deleted successfully."; // Mensaje de éxito

            // Recargar la lista de buckets después de la eliminación
            await LoadBuckets();
        }
        catch (AmazonS3Exception ex)
        {
            statusMessage = $"Error deleting bucket: {ex.Message}"; // Mensaje de error
        }
        catch (Exception ex)
        {
            statusMessage = $"Error deleting bucket: {ex.Message}"; // Mensaje de error
        }
    }

    private async Task DeleteFile(string fileKey)
    {
        try
        {
            await s3Service.GetClient().DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = selectedBucket,
                    Key = fileKey
                });

            statusMessage = $"File '{fileKey}' deleted successfully."; // Mensaje de éxito

            // Recarga la lista de archivos después de la eliminación
            await SelectBucket(selectedBucket);
        }
        catch (AmazonS3Exception ex)
        {
            statusMessage = $"Error deleting file: {ex.Message}"; // Mensaje de error
        }
        catch (Exception ex)
        {
            statusMessage = $"Error deleting file: {ex.Message}"; // Mensaje de error
        }
    }

    private async Task ViewFile(string fileKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
                {
                    BucketName = selectedBucket,
                    Key = fileKey,
                    Expires = DateTime.UtcNow.AddMinutes(10) // La URL expirará en 10 minutos
                };

            string url = s3Service.GetClient().GetPreSignedURL(request);

            // Abre el archivo en una nueva pestaña del navegador usando JS interop
            await jsRuntime.InvokeVoidAsync("open", url, "_blank");

            statusMessage = $"File '{fileKey}' opened in a new tab."; // Mensaje de éxito
        }
        catch (AmazonS3Exception ex)
        {
            statusMessage = $"Error generating view link: {ex.Message}"; // Mensaje de error
        }
        catch (Exception ex)
        {
            statusMessage = $"Error generating view link: {ex.Message}"; // Mensaje de error
        }
    }

    private async Task DownloadFile(string fileKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
                {
                    BucketName = selectedBucket,
                    Key = fileKey,
                    Expires = DateTime.UtcNow.AddMinutes(10) // La URL expirará en 10 minutos
                };

            request.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename=\"{fileKey}\"";

            string url = s3Service.GetClient().GetPreSignedURL(request);

            // Inicia la descarga del archivo en el navegador del usuario usando JS interop
            await jsRuntime.InvokeVoidAsync("open", url, "_self");

            statusMessage = $"File '{fileKey}' downloaded."; // Mensaje de éxito
        }
        catch (AmazonS3Exception ex)
        {
            statusMessage = $"Error generating download link: {ex.Message}"; // Mensaje de error
        }
        catch (Exception ex)
        {
            statusMessage = $"Error generating download link: {ex.Message}"; // Mensaje de error
        }
    }
}
```

See this component features:

![image](https://github.com/user-attachments/assets/2b5af686-da76-46d4-a8bf-5f159128037a)

