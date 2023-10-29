﻿
using Curriculum_Info_Application.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Curriculum_Info_Application.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Index()
        {
            Transaction.DeleteOldTransactions();

            // Deserialize JSON data into a list of TransactionHistory objects
            List<Transaction> transactions = ReadTransactionDataFromJson();

            // Pass the list of transactions to the view
            return View("~/Views/Home/Dashboard.cshtml", transactions);
        }

        public IActionResult ImportDurationMatrix()
        {
            var transactions = ReadTransactionDataFromJson(); // Read JSON data

            // Process data for the Import Duration Matrix chart
            var importDurations = transactions.Select(t => t.import_duration).ToList();
            var fileSizes = transactions.Select(t => t.merged_filesize).ToList();

            // Create the chart data in the format required by Plotly
            var chartData = new
            {
                x = fileSizes,
                y = importDurations,
                type = "scatter",
                mode = "markers",
                marker = new { size = 10 }
            };

            return Json(chartData); // Return the chart data as JSON
        }

        public IActionResult ImportRateByFileType()
        {
            var transactions = ReadTransactionDataFromJson(); // Read JSON data

            // Process data for the Import Rate by File Type Pie Chart
            var fileTypes = transactions.Select(t => t.filetype1).ToList();
            fileTypes.AddRange(transactions.Select(t => t.filetype2));

            // Count the occurrences of each file type
            var fileTypeCounts = fileTypes.GroupBy(type => type)
                .Select(group => new { FileType = group.Key, Count = group.Count() });

            // Create the chart data in the format required by Plotly
            var chartData = new
            {
                labels = fileTypeCounts.Select(item => item.FileType),
                values = fileTypeCounts.Select(item => item.Count)
            };

            return Json(chartData); // Return the chart data as JSON
        }

        public IActionResult LoadUserUsage()
        {
            var transactions = ReadTransactionDataFromJson(); // Read JSON data

            var userCounts = transactions.GroupBy(t => t.user)
                .Select(group => new
                {
                    User = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(item => item.Count);

            var chartData = new
            {
                labels = userCounts.Select(item => item.User),
                values = userCounts.Select(item => item.Count)
            };

            return Json(chartData);
        }


        private List<Transaction> ReadTransactionDataFromJson()
        {
            string json = null;

            if (System.IO.File.Exists("transaction.json"))
            {
                json = System.IO.File.ReadAllText("transaction.json");
            }

            // Deserialize JSON data into a list of TransactionHistory objects
            List<Transaction> transactions = JsonConvert.DeserializeObject<List<Transaction>>(json);

            return transactions;
        }

    }
}