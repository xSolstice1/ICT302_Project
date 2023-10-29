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
using Curriculum_Info_Application.Models;
using Microsoft.Net.Http.Headers;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using System.Drawing.Drawing2D;
using System.Data.OleDb;
using ServiceStack.Text;
using CsvReader = CsvHelper.CsvReader;
using ServiceStack;
using System.Collections;

namespace Curriculum_Info_Application.Controllers
{
    public class HomeController : Controller
    {
        private ImportModel model = new ImportModel();
        private char[] invalidChars = { '.', ' ', '(', ')', '/', '[', ']' ,'>','<','&','\'','"'};
        private char validChar = '_';
        private Dictionary<string, int> elementCounts = new Dictionary<string, int>();

        public IActionResult Import()
        {
            TempData["SuccessMessage"] = null;
            TempData["CurrentPage"] = null;
            ViewBag.ColumnsList1 = new SelectList(new List<SelectListItem>(), "Value", "Text");
            ViewBag.ColumnsList2 = new SelectList(new List<SelectListItem>(), "Value", "Text");
            DeleteIfFileExists("Data1.xml");
            DeleteIfFileExists("Data2.xml");
            DeleteIfFileExists("JoinedData.xml");
            elementCounts = new Dictionary<string, int>();
            if (!System.IO.File.Exists("login.json"))
            {
                TempData["LoginWarningMessage"] = "Please Login";
                return View("Index");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessImport(List<IFormFile> files)
        {
            try
            {
                TempData["LoginInfoMessage"] = null;
                List<List<string>> records = new List<List<string>>();
                LoginModel loginModel = new LoginModel();
                Transaction newTransaction = new Transaction();
                DateTime start = DateTime.Now;
                newTransaction.transaction_datetime = start;
                newTransaction.user = loginModel.GetCurrentUsername();
                newTransaction.import_starttime = start;
                newTransaction.file_qty = files.Count;
                newTransaction.filename1 = files[0].FileName;
                newTransaction.filesize1 = Math.Round((files[0].Length / 1024.0), 1);
                newTransaction.filetype1 = Path.GetExtension(files[0].FileName).ToLower();
                if(files.Count > 1)
                {
                    newTransaction.filename2 = files[1].FileName;
                    newTransaction.filesize2 = Math.Round((files[1].Length / 1024.0), 1);
                    newTransaction.filetype2 = Path.GetExtension(files[1].FileName).ToLower();
                }

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
                newTransaction.import_endtime = DateTime.Now;
                Transaction.InsertTransaction(newTransaction);

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
                    ViewBag.TableHeaders = new Dictionary<string, string>();
                    ViewBag.TableRecord = new Dictionary<string, List<string>>();
                    return RedirectToAction("Index", "Export");
                }
                TempData["CurrentPage"] = "Merge";
                return View("Merge");

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
                            /*foreach (var header in csv.HeaderRecord)
                            {
                                header.Replace(invalidChar, validChar).Replace(" ", "_").Replace("(", "_").Replace(")", "_").Replace("/", "_");
                            }*/
                            model.columnHeadersList.Add(csv.HeaderRecord.ToList());
                            check = true;
                        }
                        var record = new List<string>();
                        
                        for (int i = 0; i < csv.HeaderRecord.Length; i++)
                        {
                            string line = csv.GetField(i);
                            if (i == 0)
                            {
                                foreach (var invalidChar in invalidChars)
                                {
                                    line = line.Replace(invalidChar, validChar);
                                }
                            }
                            record.Add(line);
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
                        headerRecord.Add(worksheet.Cells[1, col].Text); // Assuming header is in the first row
                    }

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var record = new List<string>();
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            string line = worksheet.Cells[row, col].Text;
                            if (row == 1)
                            {
                                // Replace all invalid characters with validChar
                                foreach (var invalidChar in invalidChars)
                                {
                                    line = line.Replace(invalidChar, validChar);
                                }
                            }
                            record.Add(line);
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
        public IActionResult JoinTables(string selectedColumn1, string selectedColumn2, string selectedColumn3, string selectedColumn4, string joinType)
        {
            try
            {
                // Load the XML data from the two XML files
                XDocument data1Xml = XDocument.Load("Data1.xml");
                XDocument data2Xml = XDocument.Load("Data2.xml");
                IEnumerable<XElement> joinedData = null;

                foreach (var invalidChar in invalidChars)
                {
                    selectedColumn1 = selectedColumn1.Replace(invalidChar, validChar);
                    selectedColumn2 = selectedColumn2.Replace(invalidChar, validChar);
                    if (!string.IsNullOrEmpty(selectedColumn3) || !string.IsNullOrEmpty(selectedColumn4)) {
                        selectedColumn3 = selectedColumn3.Replace(invalidChar, validChar);
                        selectedColumn4 = selectedColumn4.Replace(invalidChar, validChar);
                    }
                }

                // Join the XML data based on the specified columns // jointype selected
                if (joinType.ToLower().Equals("innerjoin")) {
                    joinedData = from record1 in data1Xml.Descendants("Record")
                                    join record2 in data2Xml.Descendants("Record")
                                    on (string)record1.Element(selectedColumn1) equals (string)record2.Element(selectedColumn2)
                                    select new XElement("Record",
                                        record1.Elements(),
                                        record2.Elements());
                } else if (joinType.ToLower().Equals("leftjoin")) {
                    joinedData = from record1 in data1Xml.Descendants("Record")
                                    join record2 in data2Xml.Descendants("Record")
                                    on (string)record1.Element(selectedColumn1) equals (string)record2.Element(selectedColumn2) into leftJoin
                                    from record2 in leftJoin.DefaultIfEmpty()
                                    select new XElement("Record",
                                        record1.Elements(),
                                        record2 != null ? record2.Elements() : null);
                } else if (joinType.ToLower().Equals("rightjoin")) {
                    joinedData = from record2 in data2Xml.Descendants("Record")
                                    join record1 in data1Xml.Descendants("Record")
                                    on (string)record2.Element(selectedColumn2) equals (string)record1.Element(selectedColumn1) into rightJoin
                                    from record1 in rightJoin.DefaultIfEmpty()
                                    select new XElement("Record",
                                        record1 != null ? record1.Elements() : null,
                                        record2.Elements());
                } else if (joinType.ToLower().Equals("fulljoin")) {
                    var leftJoinResult = from record1 in data1Xml.Descendants("Record")
                                        join record2 in data2Xml.Descendants("Record")
                                        on (string)record1.Element(selectedColumn1) equals (string)record2.Element(selectedColumn2) into leftJoin
                                        from record2 in leftJoin.DefaultIfEmpty()
                                        select new
                                        {
                                            Record1 = record1,
                                            Record2 = record2
                                        };

                    var rightJoinResult = from record2 in data2Xml.Descendants("Record")
                                        join record1 in data1Xml.Descendants("Record")
                                        on (string)record2.Element(selectedColumn2) equals (string)record1.Element(selectedColumn1) into rightJoin
                                        from record1 in rightJoin.DefaultIfEmpty()
                                        select new
                                        {
                                            Record1 = record1,
                                            Record2 = record2
                                        };

                    var fullJoinResult = leftJoinResult.Union(rightJoinResult);

                    joinedData = from result in fullJoinResult
                                    select new XElement("Record",
                                        result.Record1 != null ? result.Record1.Elements() : null,
                                        result.Record2 != null ? result.Record2.Elements() : null);

                } else if (joinType.ToLower().Equals("antijoin")) {
                    joinedData = from record1 in data1Xml.Descendants("Record")
                                        join record2 in data2Xml.Descendants("Record")
                                        on (string)record1.Element(selectedColumn1) equals (string)record2.Element(selectedColumn2) into leftJoin
                                        where leftJoin.All(r => r == null)
                                        select record1;
                } else {
                    //idk what else, maybe throw error if jointype not equals to any stated above(?)
                }

                // Process the XML to add numerical suffixes to duplicate element names
                var processedXml = ProcessXml(joinedData);

                // Save the processed XML data to a new XML file
                processedXml.Save("JoinedData.xml");

                //Check if Join Key 3 & 4 selected
                if (!string.IsNullOrEmpty(selectedColumn3) || !string.IsNullOrEmpty(selectedColumn4)) {
                    XDocument xmlJoin = XDocument.Load("JoinedData.xml");
                    XDocument xmlJoin2 = XDocument.Load("Data2.xml");
                    joinedData = from record1 in xmlJoin.Descendants("Record")
                                join record2 in xmlJoin2.Descendants("Record")
                                on (string)record1.Element(selectedColumn3) equals (string)record2.Element(selectedColumn4)
                                select new XElement("Record",
                                    record1.Elements(),
                                    record2.Elements());   

                     processedXml = ProcessXml(joinedData);

                    processedXml.Save("JoinedData.xml");             
                }

                Transaction updateTransaction = new Transaction();
                updateTransaction.merged_filesize = Math.Round((GetFileSize("JoinedData.xml") / 1024.0), 1);
                updateTransaction.joinkey1 = selectedColumn1 + " | " + selectedColumn2;
                if (!string.IsNullOrEmpty(selectedColumn3) || !string.IsNullOrEmpty(selectedColumn4))
                {
                    updateTransaction.joinkey2 = selectedColumn3 + " | " + selectedColumn3;
                }
                Transaction.UpdateLastTransaction(updateTransaction);

                TempData["SuccessMessage"] = "Data joined and saved successfully.";
                ViewBag.ColumnsList1 = new SelectList(new List<string>());
                ViewBag.ColumnsList2 = new SelectList(new List<string>());
                ViewBag.TableHeaders = new Dictionary<string, string>();
                ViewBag.TableRecord = new Dictionary<string, List<string>>();
                elementCounts = new Dictionary<string, int>();
                ViewBag.CurrentPage = 0;
                ViewBag.TotalPages = 0;
                TempData["CurrentPage"] = "Export";
                return RedirectToAction("Index","Export");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error joining and saving data to XML: {ex.Message}");
            }
        }

        public long GetFileSize(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    long fileSizeInBytes = fileInfo.Length;
                    return fileSizeInBytes;
                }
                else
                {
                    // Handle the case where the file does not exist
                    return -1; // or any other appropriate value
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur when accessing the file
                Console.WriteLine($"Error: {ex.Message}");
                return -1; // or any other appropriate value
            }
        }

        private XDocument ProcessXml(IEnumerable<XElement> joinedData)
        {
            XDocument processedXml = new XDocument(new XElement("Data"));

            foreach (var record in joinedData)
            {
                var joinedRecord = new XElement("Record");
                
                foreach (var element in record.Elements())
                {
                    string elementName = element.Name.LocalName;

                    if (elementCounts.ContainsKey(elementName))
                    {
                        int count = elementCounts[elementName];
                        elementCounts[elementName]++;

                        // Add a numerical suffix to the element name
                        var newElementName = $"{elementName}{count}";
                        joinedRecord.Add(new XElement(newElementName, element.Value));
                    }
                    else
                    {
                        elementCounts[elementName] = 1;
                        joinedRecord.Add(new XElement(elementName, element.Value));
                    }
                }

                processedXml.Root?.Add(joinedRecord);
            }

            return processedXml;
        }

        private void DeleteIfFileExists(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        public IActionResult Export()
        {
            ViewBag.TableHeaders = new Dictionary<string, string>();
            ViewBag.TableRecord = new Dictionary<string, List<string>>();
            return RedirectToAction("Index", "Export");
        }
        public IActionResult Index()
        {
            TempData["SuccessMessage"] = null;
            TempData["CurrentPage"] = null;
            TempData["LoginErrorMessage"] = null;
            TempData["LoginSuccessMessage"] = null;
            TempData["LoginWarningMessage"] = null;
            TempData["LoginInfoMessage"] = null;
            TempData["ExportSuccess"] = null;
            TempData["ImportError"] = null;
            TempData["ExportError"] = null;
            if (System.IO.File.Exists("login.json"))
            {
                return View("~/Views/Home/Import.cshtml");
            }
            return View();
        }
        public IActionResult Logout()
        {
            System.IO.File.Delete("login.json");
            return View("Index");
        }
        public IActionResult Signup()
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            LoginModel loginModel = new LoginModel();
            if(!System.IO.File.Exists("login.json"))
            {
                TempData["LoginWarningMessage"] = "Please Login";
                return View("Index");
            }

            List<Transaction> transactions = new List<Transaction>();
            return RedirectToAction("Index", "Transaction");
        }

    }
}
