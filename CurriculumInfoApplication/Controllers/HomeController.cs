using System.Diagnostics;
using Curriculum_Info_Application.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Data.SqlClient;

namespace Curriculum_Info_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Import()
        {
            return View();
        }

        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ProcessImport(IFormFile file)
        {
            System.Console.WriteLine("Starting Import to Database");
            if (file != null)
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];

                            // Connect to SQL Server
                            var connectionString = "Server=tcp:ict302database.database.windows.net,1433;Initial Catalog=Testing;User ID=testadmin;Password=@Testing;Encrypt=True;";
                            System.Console.WriteLine("Connection String initialized"); //testing purposes

                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                System.Console.WriteLine("Connected to SQL Server"); //testing purposes

                                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                                {
                                    string column1 = worksheet.Cells[row, 1].Text;
                                    string column2 = worksheet.Cells[row, 2].Text;
                                    // Testing with 2 columns

                                    // Insert Query
                                    string insertSql = "INSERT INTO dbo.testing (Column1, Column2) VALUES (@Column1, @Column2)";
                                    using (var command = new SqlCommand(insertSql, connection))
                                    {
                                        command.Parameters.AddWithValue("@Column1", column1);
                                        command.Parameters.AddWithValue("@Column2", column2);

                                        int executeQuery = command.ExecuteNonQuery();

                                        if (executeQuery < 0)
                                        {
                                            System.Console.WriteLine("Error importing data!");
                                        }
                                    }
                                }
                            }

                            System.Console.WriteLine("Successfully imported data!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Error!");
                }
            }
            else
            {
                System.Console.WriteLine("Invalid Excel File!");
            }

            return RedirectToAction("Import");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}