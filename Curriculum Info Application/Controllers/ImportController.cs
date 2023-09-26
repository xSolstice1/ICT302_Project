using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Curriculum_Info_Application.Controllers
{
    public class ImportController : Controller
    {
        private readonly string connectionString = "Server=tcp:ict302database.database.windows.net,1433;Initial Catalog=Testing;User ID=testadmin;Password=@Testing;Encrypt=True;"; // Replace with your Azure SQL Database connection string
        private List<List<String>> columnHeadersList = new List<List<String>>();

        public IActionResult Index()
        {
            ViewBag.ColumnsList1 = new SelectList(new List<SelectListItem>(), "Value", "Text");
            ViewBag.ColumnsList2 = new SelectList(new List<SelectListItem>(), "Value", "Text");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessImport(List<IFormFile> files)
        {
            //temporary solution for duplicate table
            if (TableExists("myDynamicTable",connectionString) == true)
            {
                using (SqlConnection connect = new SqlConnection(connectionString))
                {
                    connect.OpenAsync();
                    string dropQuery = "DROP TABLE myDynamicTable";

                    using (SqlCommand command = new SqlCommand(dropQuery,connect))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            try
            {
                List<List<string>> records = new List<List<string>>();
                
                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".xlsx" && ext != ".xls" && ext != ".csv")
                    {
                        throw new Exception("Invalid file format!"); //file validation
                    }
                    else
                    {
                        if (file.Length > 0)
                        {
                            string fileExtension = Path.GetExtension(file.FileName).ToLower();

                            if (fileExtension == ".csv")
                            {
                                records.AddRange(await ReadCsvAsync(file));
                            }
                            else if (fileExtension == ".xlsx" || fileExtension == ".xls")
                            {
                                records.AddRange(await ReadExcelAsync(file));
                            }
                        }
                    }
                }

                if (records.Any())
                {
                    await SaveDataToAzureDatabaseAsync(records);
                }

                // Check files quantity
                if (files.Count >= 2)
                {
                    ViewBag.ColumnsList1 = new SelectList(columnHeadersList[0]);
                    ViewBag.ColumnsList2 = new SelectList(columnHeadersList[1]);
                }
                else
                {
                    ViewBag.ColumnsList1 = new SelectList(new List<string>());
                    ViewBag.ColumnsList2 = new SelectList(new List<string>());
                }

                //return View("Index");

                return Ok("Data imported and saved successfully.");
                //return View();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private async Task<List<List<string>>> ReadCsvAsync(IFormFile file)
        {
            List<List<string>> records = new List<List<string>>();

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                while (csv.Read())
                {
                    var record = new List<string>();
                    csv.ReadHeader(); //read csv header
                    columnHeadersList.Add(csv.HeaderRecord.ToList());
                    for (int i = 0; i < csv.HeaderRecord.Length; i++)
                    {
                        record.Add(csv.GetField(i));
                    }
                    records.Add(record);
                }
            }

            return records;
        }

        private async Task<List<List<string>>> ReadExcelAsync(IFormFile file)
        {
            List<List<string>> records = new List<List<string>>();

            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assuming you're working with the first worksheet
                var rowCount = worksheet.Dimension.Rows;
                var headerRecord = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    headerRecord.Add(worksheet.Cells[1, col].Text); // Assuming header is in the first row
                }

                // Add the header to columnHeadersList
                columnHeadersList.Add(headerRecord);
                for (int row = 1; row <= rowCount; row++)
                {
                    var record = new List<string>();
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        record.Add(worksheet.Cells[row, col].Text);
                    }
                    records.Add(record);
                }
            }

            return records;
        }

        private async Task SaveDataToAzureDatabaseAsync(List<List<string>> records)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Assuming your data has headers in the first row
                    var headerNames = records.FirstOrDefault();

                    // Create a table in the database dynamically based on the header names
                    CreateTableInDatabase(connection, "MyDynamicTable", headerNames);

                    // Insert data into the dynamically created table
                    InsertDataIntoDatabase(connection, "MyDynamicTable", headerNames, records);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving data to the Azure SQL Database: {ex.Message}");
            }
        }


        private void CreateTableInDatabase(SqlConnection connection, string tableName, List<string> headerNames)
        {
            // Remove invalid characters from query
            List<string> sanitizedHeaderNames = headerNames
                .Select(h => $"[{h.Replace(" ", "_").Replace("[", "").Replace("]", "")}] NVARCHAR(MAX)")
                .ToList();
            
            string createTableQuery = $"CREATE TABLE {tableName} ({string.Join(", ", sanitizedHeaderNames)})";

            using (SqlCommand command = new SqlCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void InsertDataIntoDatabase(SqlConnection connection, string tableName, List<string> headerNames, List<List<string>> records)
        {
            // Convert List<MyClass> to DataTable
            DataTable dataTable = new DataTable();
            foreach (var header in headerNames)
            {
                dataTable.Columns.Add(header);
            }

            records.RemoveAt(0); //remove header from record
            foreach (var record in records)
            {
                if (record != null)
                {
                    DataRow row = dataTable.NewRow();
                    for (int i=0; i<headerNames.Count; i++)
                    {
                        row[i] = record[i];
                    }
                    dataTable.Rows.Add(row);
                }
            }

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.WriteToServer(dataTable);
            }
        }

        public bool TableExists(string tableName, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    int tableCount = (int)command.ExecuteScalar();

                    return tableCount > 0;
                }
            }
        }

        [HttpPost]
        public IActionResult JoinTables(string selectedColumn1, string selectedColumn2)
        {
            try
            {
                // Assuming table1 and table2 are the tables you want to join
                // Replace these with your actual tables and column names
                string table1 = "Table1";
                string table2 = "Table2";

                // Join logic based on the selected columns
                string joinQuery = $@"
                    SELECT *
                    FROM {table1}
                    INNER JOIN {table2} ON {table1}.{selectedColumn1} = {table2}.{selectedColumn2}";

                List<string> joinResults = new List<string>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(joinQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Modify this part based on your actual result processing
                                string resultRow = $"{reader[selectedColumn1]} - {reader[selectedColumn2]}";
                                joinResults.Add(resultRow);
                            }
                        }
                    }
                }

                // For demonstration purposes, let's pass the join results to the view
                ViewBag.JoinResults = joinResults;

                return View("JoinResults"); // Create a view to display the join results
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

    }
}

