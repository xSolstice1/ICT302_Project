# User Guide

<hr class="solid">

## Deploying Locally

Clone the project

```bash
  git clone https://github.com/xSolstice1/ICT302_Project.git
```

Go to the project directory

```bash
  cd my-project
```

Install dependencies

```bash
  https://dotnet.microsoft.com/en-us/download [.NET 6.0]
```

Start the server

```bash
  dotnet watch run
```
<hr class="solid">

## **Login Screen**

![Login Screen](images/login.png)

#####The Login Screen is the gateway to accessing the application's features. 
#####Follow the steps below to log in:
<br/>
### 1. Enter Credentials:
#####Provide your registered email address and password in the designated fields.
*<label class="form-label">Email</label>*
<input class = "form-control", type="email", placeholder = "Enter your email", required = "required">
*<label for="login-password" class="form-label">Password</label>*
<input class = "form-control", type="password", placeholder = "Enter your password", required = "required">

### 2. Submit the Form: 
#####Click the "Login" button to submit the login form:
<button type="submit" class="btn btn-primary btn-new">Login</button>

### 3. Error Handling:
#####If there's an issue with the provided credentials, an error message will be displayed:
<div class="alert alert-danger">
Invalid username or password.
</div>

### 4. Account Creation:
#####If you don't have an account yet, you can sign up by clicking the link below the login form:
*<p>Still don't have an account? <a href="javascript:void(0);" id="toggle-signup">Click here to sign up</a></p>*

<hr class="solid">

## **Sign Up Screen**

![Sign Up Screen](images/signup.png)

##### If you have not register your account before, 
##### Follow the steps below to sign up:
<br/>

### 1. Navigate to the Sign Up Screen:
##### Visit the application and click on the "Sign Up" link on the Login Screen.
*<p>Still don't have an account? <a href="javascript:void(0);" id="toggle-signup">Click here to sign up</a></p>*

### 2. Fill in Registration Details

*<label class="form-label">Email</label>*
<input class = "form-control", type="email", placeholder = "Enter your email", required = "required">
*<label class="form-label">Username</label>*
<input class = "form-control", type="text", placeholder = "Enter your username", required = "required">
*<label for="login-password" class="form-label">Password</label>*
<input class = "form-control", type="password", placeholder = "Enter your password", required = "required">

### 3. Submit the Form: 
#####Click the "Sign Up" button to submit the sign up form:
<button type="submit" class="btn btn-primary btn-new">Sign Up</button>

### 4. Confirmation Messages:
##### If the account creation is successful, you will see a success message:
<div class="alert alert-success">
Account created.
</div>

### 5. Error Handling:
##### If there's an issue with the provided credentials, an error message will be displayed:
<div class="alert alert-danger">
Registration failed. Please try again.
</div>

### 6. Returning Users
##### If you already have an account, you can easily switch back to the Login Screen:
*<p>Already have an account? <a href="javascript:void(0);" id="toggle-login">Click here to login</a></p>*

<hr class="solid">

## **Import Screen**

![Import Screen](images/import.png)

##### The Import Data feature allows you to upload and process CSV, XLS, or XLSX files to populate your application with data.

<br/>

### 1. Access the Import Page
##### Navigate to the Import Data page by clicking on the "Import" option in the application.
![Import Tab](images/importTab.png)

### 2. Upload Files
##### I. Click on the "Choose Files" button.
![Import Upload button](images/importUpload.png)
##### II. Choose the CSV, XLS, or XLSX files you want to import. You can select up to 2 files.
##### III. You may also drag and drop your files into the column.
##### IV. Click "Upload Files" to start the import process.
![Import Upload button](images/importUploadFile.png)

### 3. Wait for Processing
##### The application will process the uploaded files. This may take some time depending on the file size and complexity.

<br/>

### 4. Review Results
##### I. Once the import is complete, review the results.
##### II. If successful, you will see a success message.
##### III. If there are errors, an error message will be displayed. Correct the issues and try importing again.

<br/>

### 5. Proceed to Merge (Optional)
##### I. If you have uploaded two files, you will be prompted to proceed to the "Merge" page.
##### II. Here, you can join the imported data based on selected columns.
![Merge Screen](images/merge.png)

*Supported File Formats*<br/>
- CSV (Comma-Separated Values<br/>
- XLS (Excel 97-2003)<br/>
- XLSX (Excel 2007 or later)<br/>

*Important Notes*<br/>
- Ensure that your files adhere to the supported file formats.<br/>
- Application only allowed import max 2 files.<br/>
- The application may enforce specific rules or constraints during import. Review error messages for guidance.<br/>

*For additional assistance, contact support or refer to the application documentation.*

<hr class="solid">

## **Merge Screen**

![Merge Screen](images/merge.png)

##### The Merge Data feature allows you to join two XML files based on specified columns and perform various types of joins. This user guide provides step-by-step instructions on using the Merge Data functionality.
<br/>

#### Prerequisites
*Before you begin, make sure you have the following:*<br/>
- *Two XML files to be merged (`File 1` and `File 2`).*<br/>
- *Understanding of the columns you want to join on (`Join Key 1` and optionally `Join Key 2`).*<br/>

<br/>

### 1. Selecting Join Keys
![Join Key](images/joinKey.png)
#### Join Key 1:
##### Choose a column from `File 1` and `File 2` to serve as the primary join key.
<br/>

#### Join Key 2 (Optional):
##### If needed, select additional columns from both files for a more complex join. But, the join type will be only allowed concatenated join.
![Join Key](images/joinKey2.png)

### 3: Choosing Join Type
##### Select the type of join you want to perform:
###### *Inner Join*
###### *Left Join*
###### *Right Join*
###### *Full Join*
###### *Anti Join*
###### *Concatenate Join* (For join key 2 selected)

<br/>

### 4: Merging Data
###### - Click the "Merge Files" button to initiate the merge process.
###### - The application will redirect to export page to display the merged data.

<br/>

*Additional Notes*<br/>
- Invalid characters in column names are automatically replaced with underscores (`_`).<br/>
- Duplicate element names are resolved by adding numerical suffixes.<br/>
- The application provides options for various types of joins and concatenation.<br/>

*Troubleshooting*<br/>
If you encounter any issues during the merge process, an error message will be displayed. 
Please review the error message and ensure that you have followed the instructions correctly.<br/>
For further assistance, contact the system administrator.<br/>

<hr class="solid">

## Export Screen

![App Screenshot](https://i.ibb.co/LCLNKkR/app4.jpg)

In the export screen, you can then manipulate the data by filtering them, selecting columns for export. <br>
Once done, click on Export to Excel button to save the data.

<hr class="solid">

## Video Tutorial

<video controls width="600" height="400">
    <source src="video.mp4" type="video/mp4">
    Your browser does not support the video tag.
</video>
