using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using MySql.Data.MySqlClient;

class Program 
{
    static void Main(string[] args) 
    {
        string pathPDF = @"C:\Users\vladi\Desktop\pdf";
        string[] arrayPDF = Directory.GetFiles(pathPDF, "*.pdf");

        PDFParsing parsing = new PDFParsing();
        OrderNumber orderNumber = new OrderNumber();
        DateBase dateBase = new DateBase();
        ErrorLog errorLog = new ErrorLog();

        PDFProcessor pdfProcessor = new PDFProcessor(parsing, orderNumber, dateBase, errorLog);

        foreach (string str in arrayPDF) 
        {
            pdfProcessor.Process(str);
        }

    }

    public class PDFProcessor
    {
        private readonly PDFParsing _parsing;
        private readonly OrderNumber _orderNumber;
        private readonly DateBase _dateBase;
        private readonly ErrorLog _errorLog;

        public PDFProcessor(PDFParsing parsing, OrderNumber orderNumber, DateBase dateBase, ErrorLog errorLog)
        {
            _parsing = parsing;
            _orderNumber = orderNumber;
            _dateBase = dateBase;
            _errorLog = errorLog;
        }

        public void Process(string PathPDF) 
        {
            try
            {
                byte[] byteArray = File.ReadAllBytes(PathPDF);
                string text = _parsing.Parse(PathPDF);
                int? number = _orderNumber.ResultNumber(text);

                if (number != null) 
                {
                    _dateBase.WriteDB(number.Value, byteArray);
                }
            }
            catch (Exception ex) 
            {
                _errorLog.Log(PathPDF, ex.ToString());
            }
        }
    }

    public class PDFParsing 
    {
        ErrorLog ErrorLog = new ErrorLog();
        public string Parse(string path) 
        {
            try
            {
                using (PdfReader reader = new PdfReader(path))
                {
                    StringWriter result = new StringWriter();
                    string textPage = string.Empty;

                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        textPage = PdfTextExtractor.GetTextFromPage(reader, i);
                        result.Write(textPage);
                    }

                    return result.ToString();
                }
            }
            catch (Exception error)
            {
                ErrorLog.Log(path, error.ToString());
                return $"Ошибка в {path}";
            }
        }
    }

    public class OrderNumber 
    {
        public int? ResultNumber(string text) 
        {
            string searchNumber = @"Номер заказа\s*:\s*(\d+)";
            Match match = Regex.Match(text, searchNumber);

            if (match.Success) 
            {
                return int.Parse(match.Groups[1].Value);
            }

            return null;
        }
    }

    public class DateBase
    {
        ErrorLog errorDB = new ErrorLog();

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
                    errorDB.Log(number.ToString(), ex.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }


    public class ErrorLog 
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
}