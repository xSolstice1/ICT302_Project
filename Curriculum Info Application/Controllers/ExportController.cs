using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

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
    }
}

