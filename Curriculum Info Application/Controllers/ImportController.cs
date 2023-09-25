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

namespace Curriculum_Info_Application.Controllers
{
    public class ImportController : Controller
    {
        private readonly string connectionString = "Server=tcp:ict302database.database.windows.net,1433;Initial Catalog=Testing;User ID=testadmin;Password=@Testing;Encrypt=True;"; // Replace with your Azure SQL Database connection string

        [HttpPost]
        public async Task<IActionResult> ProcessImport(List<IFormFile> files)
        {
            try
            {
                List<List<string>> records = new List<List<string>>();

                foreach (var file in files)
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

                if (records.Any())
                {
                    await SaveDataToAzureDatabaseAsync(records);
                }

                return Ok("Data imported and saved successfully.");
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

            foreach (var record in records)
            {
                if (record != null)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (var header in headerNames)
                    {
                        row[header] = record.GetType().GetProperty(header).GetValue(record, null);
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



    }

}

