using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Curriculum_Info_Application.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Index()
        {
            string json = null; 

            if (System.IO.File.Exists("transaction.json"))
            {
                json = System.IO.File.ReadAllText("transaction.json");
            }

            // Deserialize JSON data into a list of TransactionHistory objects
            List<Transaction> transactions = JsonConvert.DeserializeObject<List<Transaction>>(json);

            // Pass the list of transactions to the view
            return View(transactions);
        }
    }
}
