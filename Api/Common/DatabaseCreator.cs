using System.IO;

namespace Api.Common
{
    public class DatabaseCreator
    {
        public DatabaseCreator()
        {
        }

        public void CreateDatabase()
        {
            var createDatabaseScript = GetCreateDatabaseScript();
        }

        private string GetCreateDatabaseScript()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            return File.ReadAllText($"{currentDirectory}/Common/CreateDatabase.sql");
        }
    }
}