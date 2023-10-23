using System.Data.OleDb;

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
        public int filesize1 { get; set; }
        public string filetype1 { get; set; }
        public string filename2 { get; set; }
        public int filesize2 { get; set; }
        public string filetype2 { get; set; }
        public int merged_filesize { get; set; }
        public string joinkey1 { get; set; }
        public string joinkey2 { get; set;}

        public static bool insertTransaction4Upload(string connection, Transaction info)
        {
            OleDbConnection conn = new OleDbConnection(connection);

            try
            {
                conn.Open();

                OleDbCommand cmdInsert = new OleDbCommand("INSERT INTO TRANSACTION (" +
                    "TRANSACTION_DATE_TIME, " +
                    "USER, " +
                    "IMPORT_START_TIME, " +
                    "IMPORT_END_TIME," +
                    "IMPORT_DURATION, " +
                    "FILE_QTY, " +
                    "FILENAME_1, " +
                    "FILESIZE_1, " +
                    "FILETYPE_1, " +
                    "FILENAME_2, " +
                    "FILESIZE_2, " +
                    "FILETYPE_2, " +
                    "MERGED_FILESIZE, " +
                    "JOINKEY_1, " +
                    "JOINKEY_2) " +
                    "VALUES " +
                    "('" + DateTime.Now + "', " +
                    "'" + info.user + "', " +
                    "'" + DateTime.Now + "', " +
                    "" + null + ", " +
                    "" + null + ", " +
                    "" + info.file_qty + ", " +
                    "'" + info.filename1 + "', " +
                    "" + info.filesize1 + ", " +
                    "'" + info.filetype1 + "', " +
                    "'" + info.filename2 + "', " +
                    "" + info.filesize2 + ", " +
                    "'" + info.filetype2 + "', " +
                    "'" + DateTime.Now + "', " +
                    "'" + DateTime.Now + "', " +
                    "'" + DateTime.Now + "');"
                    , conn);

                // execute
                int records = cmdInsert.ExecuteNonQuery();


                if (records > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
