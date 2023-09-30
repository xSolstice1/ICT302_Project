using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace Curriculum_Info_Application.Controllers
{
    public class ExportController : Controller
    {
        public IActionResult Index()
        {
            TempData["ExportSuccess"] = null;
            TempData["ImportError"] = null;

            // Load the XML data from the joined XML file
            XDocument joinedXml = XDocument.Load("JoinedData.xml");

            // Get the headers from the first record (assuming all records have the same structure)
            var headers = joinedXml.Descendants("JoinedRecord").FirstOrDefault()
                                   ?.Elements()
                                   .Select(e => e.Name.LocalName)
                                   .ToList();

            if (headers == null)
            {
                TempData["ImportError"] = "Unable to merge the files.";
                return RedirectToAction("Index", "Home");
            }

            // Initialize a dictionary to store the header names and their corresponding values for each record
            Dictionary<string, List<string>> tableRecord = new Dictionary<string, List<string>>();

            // Iterate over each joined record and extract values for each header
            int recordIndex = 1;
            foreach (var record in joinedXml.Descendants("JoinedRecord"))
            {
                List<string> recordValues = new List<string>();
                foreach (var header in headers)
                {
                    recordValues.Add((string)record.Element(header));
                }

                tableRecord.Add(recordIndex.ToString(), recordValues);
                recordIndex++;
            }

            ViewBag.TableHeaders = headers.ToDictionary(header => header, header => header);

            // Populate ViewBag.TableRecord with the record values
            ViewBag.TableRecord = tableRecord;

            TempData["ExportSuccess"] = "Data loaded successfully.";

            return View("~/Views/Home/Export.cshtml");
        }

        public IActionResult ExportToCsv()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Load the XML file
                XDocument xmlDocument = XDocument.Load("JoinedData.xml");

                // Create a new Excel package
                using (ExcelPackage package = new ExcelPackage())
                {
                    // Create a worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Data");

                    // Get the headers from the XML (distinct element names)
                    var headers = xmlDocument.Descendants("JoinedRecord")
                                              .Elements()
                                              .Select(e => e.Name.LocalName)
                                              .Distinct()
                                              .ToList();

                    // Write headers to the Excel worksheet
                    for (int i = 0; i < headers.Count; i++)
                        worksheet.Cells[1, i + 1].Value = headers[i];

                    // Get the data for each record
                    var records = xmlDocument.Descendants("JoinedRecord")
                                              .Select(record =>
                                                  headers.Select(header => (string)record.Element(header)).ToList())
                                              .ToList();

                    // Write records to the Excel worksheet
                    for (int i = 0; i < records.Count; i++)
                    {
                        for (int j = 0; j < headers.Count; j++)
                            worksheet.Cells[i + 2, j + 1].Value = records[i][j];
                    }

                    // Save the Excel package to a stream
                    MemoryStream stream = new MemoryStream(package.GetAsByteArray());

                    // Return the Excel file as a FileStreamResult
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "exportedData.xlsx");
                }
            }
            catch (Exception ex)
            {
                TempData["ExportError"] = "An error occurred while exporting data: " + ex.Message;
            }

            return RedirectToAction("Index", "Home"); // Redirect to the home page
        }


    }
}

