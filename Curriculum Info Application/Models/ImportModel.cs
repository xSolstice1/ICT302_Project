namespace Curriculum_Info_Application.Models
{
    public class ImportModel
    {
        public List<List<String>> columnHeadersList {get; set;}

        public ImportModel()
        {
            columnHeadersList = new List<List<String>>();
        }
    }
}
