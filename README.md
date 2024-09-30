# How to create S3 bucket from Blazor Web App

## 1. Create AWS Free Account

Follow the steps explained in the URL for creating a new **AWS Free Account**:

https://github.com/luiscoco/Curso_Aprende_AWS_SDK_para_dotNet-Leccion1

When you create a new AWS Account you are provided a **Root User**

## 2. Create a new User with Admin permissions

See this youtube video: https://www.youtube.com/watch?v=En5rnbmX7U8

## 3. Generate AWS Access Key and Secret Key

See this youtube video: https://www.youtube.com/watch?v=Fxflt0v2Mfc

## 4. Install AWS CLI and Configure AWS Account

See this youtube video: https://www.youtube.com/watch?v=u0JyzUGzvJA

## 5. Create a new Blazor Web Application with Visual Studio 2022 Community Edition


## 6. Load the package with Nuget

Load the 

![image](https://github.com/user-attachments/assets/8ac06453-1c03-42e5-921d-f6180e85427f)

## 7. Create a new Razor Component for creating AWS S3 Bucket  

This is a Blazor component that provides a form for creating an S3 bucket in AWS

It collects the bucket name from the user, and upon submission, it interacts with the AWS S3 service using the AWS SDK to create the bucket

Success and error messages are conditionally displayed based on the result of the bucket creation

For creating a new Razor Component right click on the Pages folder and then select the menu option Add New Razor Component, then set then new component name 

![image](https://github.com/user-attachments/assets/4cc5b4ff-2d95-42b6-8733-2a5c89151dbf)

![image](https://github.com/user-attachments/assets/717bc5ee-65d0-4a01-a6ad-1a68561f74bd)

![image](https://github.com/user-attachments/assets/3967ea8a-8fee-4850-972a-d92701404b87)

In the AWS_Component.razor file input the following code:

```razor
@page "/createS3Bucket"

@using Amazon.S3
@using Amazon.S3.Model

<h3>Azure Services</h3>

<label for="bucketNameInput">Enter S3 Bucket Name:</label>
<input id="bucketNameInput" @bind="bucketName" placeholder="Bucket Name" />

<p>Bucket Name: @bucketName</p>

<button @onclick="CreateS3Bucket">Create S3 Bucket</button>

@if (!string.IsNullOrEmpty(errorMessage))
{
    if (errorMessage == "The bucket was sucessfully created")
    {
        <p style="color:green">@errorMessage</p>
    }
    else 
    { 
        <p style="color:red">@errorMessage</p>
    }
}

@code {
    private IAmazonS3? _client;
    private string bucketName = "";
    private string errorMessage = "";

    protected override async Task OnInitializedAsync()
    {
        _client = new AmazonS3Client();
    }

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

                var response = await _client.PutBucketAsync(request);
                errorMessage = "The bucket was sucessfully created"; // Clear error message on success
            }
            else
            {
                errorMessage = "Bucket name cannot be empty.";
            }
        }
        catch (AmazonS3Exception ex)
        {
            // AmazonS3Exception will catch specific S3 errors
            errorMessage = $"Error creating S3 bucket: {ex.Message}";
        }
        catch (Exception ex)
        {
            // Catch any other general exceptions
            errorMessage = $"An unexpected error occurred: {ex.Message}";
        }
    }
}
```

This Razor component is designed to allow users to create an **Amazon S3 bucket** through a web-based interface built using Blazor, a framework for building interactive web UIs in .NET

**Page Route**: This directive defines the URL at which this page will be accessible. In this case, navigating to /createS3Bucket will render this component

```
@page "/createS3Bucket"
```

**Using Directives**: These directives import the necessary namespaces from **AWS SDK**, specifically for interacting with Amazon S3 services and models like PutBucketRequest

```
@using Amazon.S3
@using Amazon.S3.Model
```

**UI Elements**: A label and input field where the user can enter the name of the S3 bucket:

```
<label for="bucketNameInput">Enter S3 Bucket Name:</label>
<input id="bucketNameInput" @bind="bucketName" placeholder="Bucket Name" />
```

The **@bind="bucketName"** directive binds the value of the input to the bucketName variable in the backend logic, enabling two-way data binding

A **paragraph** showing the entered bucket name:

```
<p>Bucket Name: @bucketName</p>
```

A **button** that triggers the creation of the S3 bucket when clicked: The **@onclick** directive binds the button's click event to the **CreateS3Bucket** method

```
<button @onclick="CreateS3Bucket">Create S3 Bucket</button>
```

**Conditional Error/Succes Messages**:

If errorMessage is set (i.e., it's not null or empty), the code conditionally displays either a success or error message based on the value of **errorMessage**:

```
@if (!string.IsNullOrEmpty(errorMessage))
{
    if (errorMessage == "The bucket was sucessfully created")
    {
        <p style="color:green">@errorMessage</p>
    }
    else 
    { 
        <p style="color:red">@errorMessage</p>
    }
}
```

**Backend Logic (@code Block)**: This block contains the C# code that drives the functionality of the component

**Variables**:

**_client**: an instance of IAmazonS3 to interact with S3

**bucketName**: holds the name of the bucket entered by the user

**errorMessage**: stores error or success messages to be displayed to the user

**Lifecycle Method (OnInitializedAsync)**:

Initializes the **_client** with an instance of **AmazonS3Client** when the component is loaded

**CreateS3Bucket** Method: When the user clicks the button, this method is triggered

It checks if the **bucketName** is not empty and proceeds to create the bucket using the AWS SDK’s **PutBucketRequest**

If the bucket is created successfully, it sets a **success message in green**

If there is an error (either an AWS-specific AmazonS3Exception or any general exception), it catches the error and displays an appropriate **error message in red**

## 8. Create a new Menu item for Navigating to the new Razor Component

Modify the **NavMenu.razor** compoent

![image](https://github.com/user-attachments/assets/a6d7ff44-805f-4e0c-8aaf-017e068475a2)

Add this code for creating a new **NavLink** to access the new Component:

```
<div class="nav-item px-3">
    <NavLink class="nav-link" href="createS3Bucket">
       <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> CreateS3Bucket
    </NavLink>
</div>
```

## 9. Run the application and verify with AWS Console the S3 Bucket was created

![image](https://github.com/user-attachments/assets/c8277144-e817-4dd7-a22f-8504997b5f2a)

![image](https://github.com/user-attachments/assets/b90b6e06-3079-4233-b2c0-30fe54a93c42)

![image](https://github.com/user-attachments/assets/9c924ca2-3acd-4f69-94c5-4f8098027bc1)

We confirme in AWS Console the new S3 bucket was successfully created 

![image](https://github.com/user-attachments/assets/50c87fab-7b6b-45ba-8549-865d23ad0ec3)

