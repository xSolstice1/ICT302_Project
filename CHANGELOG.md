# ICT302 Project Changelog

Update 9/23/2023 [Jinwei]

- Added process function for import
- Added !important to self.css under nav hover color to override style

Database Info:
Running on Azure Database
Server: ict302database.database.windows.net
Port: 1433
Username: testadmin
Password: @Testing

- Added package EPPlus for excel handling
- Added package Microsoft.Data.SqlClient for SQL connection

Update 9/26/2023 [Jinwei]

- Added in import logic by Tay
- Fixed SQL connection
- Fixed query for creating table
- Fixed adding rows to Data Table
- Fixed reading of CSV File (need to read header before accessing headerrecord)
- Importing to database generally works now
- Added temporary check for duplicate table name

Update 10/3/2023 [CK]

- Added Pager for export screen
- Update from using SQL to LINQ

Update 10/28/2023 [CK, Jinwei]

- Added dashboard
- Added login functionalities
- Added different kinds of join
- UI update
