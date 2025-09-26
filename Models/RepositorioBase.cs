using MySql.Data.MySqlClient;

namespace Inmobiliaria.Models
{
    public abstract class RepositorioBase
    {
        protected string connectionString = "server=localhost;database=InmobiliariaDB;user=root;password=;";

        // Constructor que permite leer la cadena desde appsettings.json
        public RepositorioBase(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
