namespace PartialViews.Models
{
    public class ProgListModel
    {
        public string ListTitle { get; set; }
        public List<string> ProgLangs { get; set; }

        public ProgListModel(string title, List<string> langs) 
        { 
            ListTitle = title;
            ProgLangs = langs;
        }
    }
}
