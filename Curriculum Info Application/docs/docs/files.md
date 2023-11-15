# Files

<hr class="solid">

## HomeController.cs

### Import()
**Description:** Runs DeleteIfFileExist() to delete XML files if they already exist.<br/>
**Return Type:** IActionResults<br/>
### ProcessImport(List<IFormFile> files)<br/>
**Description:** Processes the uploaded files, reads CSV or Excel files, saves data to XML, and updates transaction information.<br/>
**Parameters:**
List<IFormFile> files: List of uploaded files.<br/>
### ReadCsvAsync(IFormFile file)<br/>
**Description:** Reads data from the uploaded CSV file asynchronously and returns a list of records.<br/>
**Parameters:**
IFormFile file: Uploaded CSV file.<br/>
### ReadExcelAsync(IFormFile file)<br/>
**Description:** Reads data from the uploaded Excel file asynchronously and returns a list of records.<br/>
**Parameters:**
IFormFile file: Uploaded Excel file.<br/>
### SaveDataToXmlAsync(List<List<string>> records, int i)<br/>
**Description:** Saves the provided data records to an XML file asynchronously.<br/>
**Parameters:**
List<List<string>> records: List of data records.
int i: Index for naming the XML file.<br/>
### JoinTables(string selectedColumn1, string selectedColumn2, string selectedColumn3, string selectedColumn4, string joinType)<br/>
**Description:** Joins XML data based on specified columns and join type, processes XML, and saves to a new XML file.<br/>
**Parameters:**<br/>
string selectedColumn1: First selected column for the join.<br/>
string selectedColumn2: Second selected column for the join.<br/>
string selectedColumn3: Third selected column for the join (optional).<br/>
string selectedColumn4: Fourth selected column for the join (optional).<br/>
string joinType: Type of join (InnerJoin, LeftJoin, RightJoin, FullJoin, AntiJoin).<br/>
### GetFileSize(string filePath)<br/>
**Description:** Gets the size of the specified file.<br/>
**Parameters:**
string filePath: Path of the file.<br/>
### ProcessXml(IEnumerable<XElement> joinedData)<br/>
**Description:** Processes XML data, adds numerical suffixes to duplicate element names, and returns the processed XML.<br/>
**Parameters:**
IEnumerable<XElement> joinedData: Joined XML data.<br/>
### DeleteIfFileExists(string filePath)<br/>
**Description:** Deletes the specified file if it exists.<br/>
**Parameters:**
string filePath: Path of the file.<br/>
### Export()<br/>
**Description:** Redirects to the "Export" page.<br/>
### User()<br/>
**Description:** Redirects to the "Login" page.<br/>
### Index()<br/>
**Description:** Displays the "Import" page or the "Index" page based on the login status.<br/>
### Logout()<br/>
**Description:** Deletes the login file and redirects to the "Index" page.<br/>
### Signup()<br/>
**Description:** Displays the "Signup" page.<br/>
### Dashboard()<br/>
**Description:** Displays the "Dashboard" page, showing transaction history if logged in.<br/>

<hr class="solid">

## ExportController.cs

### Index(int? page, string? filters)<br/>
**Description:** Displays the "Export" page, loads data from the joined XML file, applies pagination and filtering, and shows the data on the page.<br/>

**Parameters:**<br/>
- `int? page`: Current page number (optional).<br/>
- `string? filters`: JSON-formatted filters for data (optional).<br/>

### ExportToCsv(string selectedColumns, string filters)<br/>
**Description:** Exports selected columns of data to a CSV file based on provided filters.<br/>

**Parameters:**<br/>
- `string selectedColumns`: Comma-separated list of selected column names.<br/>
- `string filters`: JSON-formatted filters for data.<br/>

### MatchesFilters(XElement record, Dictionary<string, string>? filters)<br/>
**Description:** Checks if the given XML record matches the provided filters.<br/>

**Parameters:**<br/>
- `XElement record`: XML record to check.<br/>
- `Dictionary<string, string>? filters`: Filters for the record.<br/>

<hr class="solid">

## LoginController.cs

### Index()<br/>
**Description:** Displays the "User" page, loads user data from a JSON file, and shows user information on the page.<br/>

### DeleteUser(string email)<br/>
**Description:** Deletes a user with the specified email and redirects to the "Index" page.<br/>

**Parameters:**<br/>
- `string email`: Email of the user to be deleted.<br/>

### ToggleAdminStatus(string email)<br/>
**Description:** Toggles the admin status of a user with the specified email and redirects to the "Index" page.<br/>

**Parameters:**<br/>
- `string email`: Email of the user to toggle admin status.<br/>

### GetUserList()<br/>
**Description:** Reads user data from a JSON file and returns a list of `LoginModel` objects.<br/>

**Return Type:** List<LoginModel><br/>

### ToggleAdminStatusByEmail(string email)<br/>
**Description:** Toggles the admin status of a user with the specified email and updates the JSON file.<br/>

**Parameters:**<br/>
- `string email`: Email of the user to toggle admin status.<br/>

### Login(LoginModel model)<br/>
**Description:** Processes user login, checks credentials, and redirects to the "Import" page if successful.<br/>

**Parameters:**<br/>
- `LoginModel model`: User login credentials.<br/>

### Signup(LoginModel model)<br/>
**Description:** Processes user registration, inserts a new user, and redirects to the "Index" page.<br/>

**Parameters:**<br/>
- `LoginModel model`: User registration data.<br/>

<hr class="solid">

## TransactionController.cs

### Index()<br/>
**Description:** Deletes old transactions and displays the dashboard page with transaction history.<br/>

### ImportDurationMatrix()<br/>
**Description:** Reads transaction data from JSON, processes data for the Import Duration Matrix chart, and returns the chart data as JSON.<br/>

**Return Type:** JsonResult<br/>

### ImportRateByFileType()<br/>
**Description:** Reads transaction data from JSON, processes data for the Import Rate by File Type Pie Chart, and returns the chart data as JSON.<br/>

**Return Type:** JsonResult<br/>

### LoadUserUsage()<br/>
**Description:** Reads transaction data from JSON, processes data for user usage, and returns the chart data as JSON.<br/>

**Return Type:** JsonResult<br/>

### ReadTransactionDataFromJson()<br/>
**Description:** Reads transaction data from the JSON file and deserializes it into a list of `Transaction` objects.<br/>

**Return Type:** List<Transaction><br/>
