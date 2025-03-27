namespace ViewComponentsExample.Models
{
    public class PersonTableModel
    {
        public string Title { get; set; }
        public List<Person> Persons { get; set; }
    }
    public class Person
    {
        public string Name { get; set; }
        public string JobTitle { get; set; }
    }

}
