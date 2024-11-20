using Interface;
using MySql.Data.MySqlClient;

namespace DateBase 
{
    public class Loggi : ILOG
    {
        public void Log(string path, string error)
        {
            string connectionString = "server=localhost; user=root; password=root; database=logs";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO info (Path, Error) VALUES (@Path, @Error)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Path", path);
                        command.Parameters.AddWithValue("@Error", error);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error: {ex}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }

    public class DB : IDataBase
    {
        private readonly ILOG _error;

        public DB(ILOG error)
        {
            _error = error;
        }

        public void WriteDB(int number, byte[] fileData)
        {

            string connectionString = "server=localhost;user=root;password=root;database=files";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string check = "SELECT COUNT(*) FROM info WHERE Name = @Number";

                    using (MySqlCommand command = new MySqlCommand(check, connection))
                    {
                        command.Parameters.AddWithValue("@Number", number);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            return;
                        }
                    }

                    string query = "INSERT INTO info (Name, Path) VALUES (@Name, @Path)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", number);
                        command.Parameters.AddWithValue("@Path", fileData);
                        //command.Parameters["@Path"].MySqlDbType = MySqlDbType.Blob;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    _error.Log(number.ToString(), ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}