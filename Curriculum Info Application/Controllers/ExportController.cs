using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace Curriculum_Info_Application.Controllers
{
    public class ExportController : Controller
    {
        public IActionResult Index()
        {
            try
            {
                TempData["ExportSuccess"] = null;
                TempData["ImportError"] = null;
                TempData["ExportError"] = null;

                string filename = System.IO.File.Exists("JoinedData.xml") ? "JoinedData.xml" : "Data1.xml";

                // Load the XML data from the joined XML file
                XDocument joinedXml = XDocument.Load(filename);

                // Get the headers from the first record (assuming all records have the same structure)
                var headers = joinedXml.Descendants("Record").FirstOrDefault()
                                       ?.Elements()
                                       .Select(e => e.Name.LocalName)
                                       .ToList();

                if (headers == null)
                {
                    TempData["ImportError"] = "Unable to merge the files.";
                    return View("~/Views/Home/Export.cshtml");
                }

                // Initialize a dictionary to store the header names and their corresponding values for each record
                Dictionary<string, List<string>> tableRecord = new Dictionary<string, List<string>>();

                // Iterate over each joined record and extract values for each header
                int recordIndex = 1;
                foreach (var record in joinedXml.Descendants("Record"))
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
            catch (Exception ex)
            {
                TempData["ImportError"] = "Unable to proceed export. Please re-import the files.";
                return View("~/Views/Home/Export.cshtml");
            }
        }

        public IActionResult ExportToCsv(string selectedColumns, string filters)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Split the selected column names
                var selectedColumnList = selectedColumns.Split(',');

                string filename = System.IO.File.Exists("JoinedData.xml") ? "JoinedData.xml" : "Data1.xml";
                // Load the XML file
                XDocument xmlDocument = XDocument.Load(filename);

                // Parse the filters into a dictionary
                var filterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(filters);

                // Create a new Excel package
                using (ExcelPackage package = new ExcelPackage())
                {
                    // Create a worksheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Data");

                    // Write headers for selected columns to the Excel worksheet
                    for (int i = 0; i < selectedColumnList.Length; i++)
                        worksheet.Cells[1, i + 1].Value = selectedColumnList[i];

                    // Get the data for each record and write records for selected columns to the Excel worksheet
                    var records = xmlDocument.Descendants("Record")
                                              .Where(record => MatchesFilters(record, filterDictionary))
                                              .Select(record =>
                                                  selectedColumnList.Select(header => (string)record.Element(header)).ToArray())
                                              .ToArray();

                    for (int i = 0; i < records.Length; i++)
                    {
                        for (int j = 0; j < selectedColumnList.Length; j++)
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

            return View("~/Views/Home/Export.cshtml");
        }

        private bool MatchesFilters(XElement record, Dictionary<string, string> filters)
        {
            foreach (var filter in filters)
            {
                var header = filter.Key;
                var value = filter.Value.ToLower();

                var cellValue = ((string)record.Element(header))?.ToLower();
                if (cellValue != null && !cellValue.Contains(value))
                {
                    return false; // Does not match the filter, so exclude this record
                }
            }

            return true; // Matches all filters
        }

    }
}

