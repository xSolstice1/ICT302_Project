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
        private ImportModel model = new ImportModel();
        private char[] invalidChars = { '.', ' ', '(', ')', '/', '[', ']' };
        private char validChar = '_';

        public IActionResult Index()
        {
            TempData["SuccessMessage"] = null;
            //TempData["ImportError"] = null;
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
                            if(i == 0)
                            {
                                foreach (var invalidChar in invalidChars)
                                {
                                    csv.GetField(i).Replace(invalidChar, validChar);
                                }
                            }
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
                        headerRecord.Add(worksheet.Cells[1, col].Text); // Assuming header is in the first row
                    }

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var record = new List<string>();
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            if(row == 1)
                            {
                                foreach (var invalidChar in invalidChars)
                                {
                                    worksheet.Cells[row, col].Text.Replace(invalidChar, validChar);
                                }
                            }
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
                                     record1.Elements(),
                                     record2.Elements());

                // Process the XML to add numerical suffixes to duplicate element names
                var processedXml = ProcessXml(joinedData);

                // Save the processed XML data to a new XML file
                processedXml.Save("JoinedData.xml");


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

        private XDocument ProcessXml(IEnumerable<XElement> joinedData)
        {
            XDocument processedXml = new XDocument(new XElement("JoinedData"));

            foreach (var record in joinedData)
            {
                var joinedRecord = new XElement("JoinedRecord");

                Dictionary<string, int> elementCounts = new Dictionary<string, int>();
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
