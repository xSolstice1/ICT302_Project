using System.Data.OleDb;
using Newtonsoft.Json;

namespace Curriculum_Info_Application.Models
{
    public class Transaction
    {
        public DateTime transaction_datetime { get; set; }
        public string user { get; set; }
        public DateTime import_starttime { get; set; }
        public DateTime import_endtime { get; set; }
        public int import_duration { get; set;}
        public int file_qty { get; set;}
        public string filename1 { get; set; }
        public double filesize1 { get; set; }
        public string filetype1 { get; set; }
        public string filename2 { get; set; }
        public double filesize2 { get; set; }
        public string filetype2 { get; set; }
        public double merged_filesize { get; set; }
        public string joinkey1 { get; set; }
        public string joinkey2 { get; set;}

        private static readonly string _filePath = SystemConstant.TRANSACTION_FILEPATH;

        private static readonly int _dataKeepDays = 10;

        public static bool InsertTransaction(Transaction transaction)
        {
            try
            {
                // Load existing transactions
                var existingTransactions = LoadTransactions();

                // Add the new transaction
                existingTransactions.Add(transaction);

                // Serialize and save the updated transactions
                string json = JsonConvert.SerializeObject(existingTransactions, Formatting.Indented);
                File.WriteAllText(_filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void DeleteOldTransactions()
        {
            try
            {
                var transactions = LoadTransactions();

                if (transactions != null)
                {
                    var currentDate = DateTime.Now;

                    // Remove transactions older than 'days' days
                    transactions.RemoveAll(t => (currentDate - t.transaction_datetime).TotalDays > _dataKeepDays);

                    // Serialize and save the updated transactions
                    string json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
                    File.WriteAllText(_filePath, json);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
            }
        }

        public static bool UpdateLastTransaction(Transaction updatedTransaction)
        {
            try
            {
                var transactions = LoadTransactions();

                if (transactions.Count > 0)
                {
                    // Get the last transaction
                    var lastTransaction = transactions[transactions.Count - 1];

                    lastTransaction.merged_filesize = updatedTransaction.merged_filesize;
                    lastTransaction.joinkey1 = updatedTransaction.joinkey1;
                    lastTransaction.joinkey2 = updatedTransaction.joinkey2;
                    TimeSpan timeDifference = DateTime.Now - lastTransaction.import_starttime;
                    lastTransaction.import_duration = (int)timeDifference.TotalSeconds;

                    // Serialize and save the updated transactions
                    string json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
                    File.WriteAllText(_filePath, json);

                    return true;
                }
                return false; // No transactions to update
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private static List<Transaction> LoadTransactions()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<Transaction>>(json);
            }
            return new List<Transaction>();
        }

    }
}
