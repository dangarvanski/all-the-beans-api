using CreateSQLiteDatabase;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        string jsonPath = "AllTheBeans.json";
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("JSON file not found!");
            return;
        }

        string jsonData = File.ReadAllText(jsonPath);
        var beans = JsonConvert.DeserializeObject<List<BeanDbRecord>>(jsonData);

        using (var context = new AppDbContext())
        {
            context.Database.EnsureCreated();
            context.Beans.AddRange(beans);
            context.SaveChanges();
            Console.WriteLine("Database created and populated successfully!");
        }
    }
}