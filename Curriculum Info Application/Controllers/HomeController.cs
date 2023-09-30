﻿using CsvHelper;
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
using Curriculum_Info_Application.Models;
using Microsoft.Net.Http.Headers;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace Curriculum_Info_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connectionString = "Server=tcp:ict302database.database.windows.net,1433;Initial Catalog=Testing;User ID=testadmin;Password=@Testing;Encrypt=True;"; // Replace with your Azure SQL Database connection string
        private List<List<String>> columnHeadersList = new List<List<String>>();
        private ImportModel model = new ImportModel();

        public IActionResult Index()
        {
            TempData["SuccessMessage"] = null;
            ViewBag.ColumnsList1 = new SelectList(new List<SelectListItem>(), "Value", "Text");
            ViewBag.ColumnsList2 = new SelectList(new List<SelectListItem>(), "Value", "Text");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessImport(List<IFormFile> files)
        {
            try
            {
                List<List<string>> records = new List<List<string>>();

                for (int i = 0; i < files.Count; i++)
                {
                    string ext = Path.GetExtension(files[i].FileName).ToLower();
                    if (ext != ".xlsx" && ext != ".xls" && ext != ".csv")
                    {
                        throw new Exception("Invalid file format!"); //file validation
                    }
                    else
                    {
                        if (files[i].Length > 0)
                        {
                            records = new List<List<string>>();
                            string fileExtension = Path.GetExtension(files[i].FileName).ToLower();

                            if (fileExtension == ".csv")
                            {
                                records.AddRange(await ReadCsvAsync(files[i]));
                            }
                            else if (fileExtension == ".xlsx" || fileExtension == ".xls")
                            {
                                records.AddRange(await ReadExcelAsync(files[i]));
                            }

                            if (records.Any())
                            {
                                SaveDataToXmlAsync(records, i);
                            }
                        }
                    }
                }

                TempData["SuccessMessage"] = "Data imported and saved successfully.";

                // Check files quantity
                if (files.Count >= 2)
                {
                    ViewBag.ColumnsList1 = new SelectList(model.columnHeadersList[0]);
                    ViewBag.ColumnsList2 = new SelectList(model.columnHeadersList[1]);

                }
                else
                {
                    ViewBag.ColumnsList1 = new SelectList(new List<string>());
                    ViewBag.ColumnsList2 = new SelectList(new List<string>());
                    return View("Export");
                }

                return View("Index");

                //return Ok("Data imported and saved successfully.");
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

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    bool check = false; //check if header read already

                    while (csv.Read())
                    {
                        if (check == false)
                        {
                            csv.ReadHeader(); //read csv header
                            foreach (var header in csv.HeaderRecord)
                            {
                                header.Replace(".","_");
                            }
                            model.columnHeadersList.Add(csv.HeaderRecord.ToList());
                            check = true;
                        }
                        var record = new List<string>();
                        for (int i = 0; i < csv.HeaderRecord.Length; i++)
                        {
                            record.Add(csv.GetField(i));
                        }
                        records.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading CSV file: " + ex.Message);
            }

            return records;
        }


        private async Task<List<List<string>>> ReadExcelAsync(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                List<List<string>> records = new List<List<string>>();

                using (var package = new ExcelPackage(file.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Assuming you're working with the first worksheet
                    var rowCount = worksheet.Dimension.Rows;
                    var headerRecord = new List<string>();
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        headerRecord.Add(worksheet.Cells[1, col].Text.Replace(".","_")); // Assuming header is in the first row
                    }

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var record = new List<string>();
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            record.Add(worksheet.Cells[row, col].Text);
                        }
                        records.Add(record);
                    }

                    model.columnHeadersList.Add(headerRecord);
                }

                return records;
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading Excel file.", ex);
            }
        }

        private async Task SaveDataToXmlAsync(List<List<string>> records, int i)
        {
            try
            {
                var headerNames = records.FirstOrDefault();
                i += 1;

                // Create XML document
                XDocument xmlDocument = new XDocument(
                    new XElement("Data",
                        records.Skip(1).Select(record =>
                            new XElement("Record",
                                record.Select((field, index) =>
                                    new XElement(headerNames[index], field))))));

                // Save to XML file
                string xmlFilePath = $"Data{i}.xml";
                xmlDocument.Save(xmlFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving data to XML: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult JoinTables(string selectedColumn1, string selectedColumn2)
        {
            try
            {
                // Load the XML data from the two XML files
                XDocument data1Xml = XDocument.Load("Data1.xml");
                XDocument data2Xml = XDocument.Load("Data2.xml");

                // Join the XML data based on the specified columns
                var joinedData = from record1 in data1Xml.Descendants("Record")
                                 join record2 in data2Xml.Descendants("Record")
                                 on (string)record1.Element(selectedColumn1) equals (string)record2.Element(selectedColumn2)
                                 select new XElement("JoinedRecord",
                                     new XElement(selectedColumn1, (string)record1.Element(selectedColumn1)),
                                     new XElement(selectedColumn2, (string)record2.Element(selectedColumn2)),
                                     record1.Elements().Where(e => e.Name != selectedColumn1),
                                     record2.Elements().Where(e => e.Name != selectedColumn2));

                // Create a new XML document for the joined data
                XDocument joinedXml = new XDocument(new XElement("JoinedData", joinedData));

                // Save the joined XML data to a new XML file
                joinedXml.Save("JoinedData.xml");

                TempData["SuccessMessage"] = "Data joined and saved successfully.";
                ViewBag.ColumnsList1 = new SelectList(new List<string>());
                ViewBag.ColumnsList2 = new SelectList(new List<string>());
                ViewBag.TableHeaders = new Dictionary<string, string>();
                ViewBag.TableRecord = new Dictionary<string, List<string>>();

                return RedirectToAction("Index","Export");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error joining and saving data to XML: {ex.Message}");
            }
        }


        /*private async Task SaveDataToAzureDatabaseAsync(List<List<string>> records, int i)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Assuming your data has headers in the first row
                    var headerNames = records.FirstOrDefault();
                    i+=1;
                    // Create a table in the database dynamically based on the header names
                    CreateTableInDatabase(connection, "MyDynamicTable" + i, headerNames);

                    // Insert data into the dynamically created table
                    InsertDataIntoDatabase(connection, "MyDynamicTable" + i, headerNames, records);
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
                .Select(h => $"[{h.Replace(".","_").Replace(" ", "_").Replace("[", "").Replace("]", "")}] NVARCHAR(MAX)")
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
                string table1 = "MyDynamicTable1";
                string table2 = "MyDynamicTable2";
                string jointable = "MyJoinTable";

                string joinQuery = $@"
            SELECT *
            FROM {table1}
            INNER JOIN {table2} ON {table1}.{selectedColumn1} = {table2}.{selectedColumn2}";

                DataTable dt = new DataTable();
                List<string> headerNames = new List<string>();
                List <List<string>> tableRecords = new List<List<string>>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (var join = new SqlCommand(joinQuery, connection))
                using (var data = new SqlDataAdapter(join))
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    connection.Open();
                    data.Fill(dt);

                    //read column names in datatable
                    foreach (DataColumn column in dt.Columns)
                    {
                        headerNames.Add(column.ColumnName);
                        System.Console.WriteLine(column.ColumnName);
                    }
                    // Read the records
                    foreach (DataRow row in dt.Rows)
                    {
                        List<string> recordValues = row.ItemArray.Select(item => item.ToString()).ToList();
                        tableRecords.Add(recordValues);
                    }
                    CreateTableInDatabase(connection, jointable, headerNames);
                    bulkCopy.DestinationTableName = jointable;
                    bulkCopy.WriteToServer(dt);
                }

                // Populate ViewBag.TableHeaders with appropriate values
                ViewBag.TableHeaders = headerNames.Select(header => new KeyValuePair<string, string>(header, header)).ToDictionary(x => x.Key, x => x.Value);

                // Populate ViewBag.TableRecord with appropriate values
                ViewBag.TableRecord = tableRecords.Select((record, index) => new KeyValuePair<string, List<string>>(index.ToString(), record)).ToDictionary(x => x.Key, x => x.Value);

                // Redirect to the ExportController's Export action
                return View("Export");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }*/


        public IActionResult Import()
        {
            return View();
        }
        public IActionResult Export()
        {
            ViewBag.TableHeaders = new Dictionary<string, string>();
            ViewBag.TableRecord = new Dictionary<string, List<string>>();
            return RedirectToAction("Index", "Export");
        }

    }
}
