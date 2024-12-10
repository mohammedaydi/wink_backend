namespace wink.Models
{
    public class WinkDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string UsersCollectionName { get; set; } = null!;
        public string ItemsCollectionName { get; set; } = null!;

    }
}
