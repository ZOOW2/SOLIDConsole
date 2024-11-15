using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using MySql.Data.MySqlClient;
using static System.Net.Mime.MediaTypeNames;

class Program 
{
    static void Main(string[] args) 
    {
        string pathPDF = @"C:\Users\vladi\Desktop\pdf";
        string[] arrayPDF = Directory.GetFiles(pathPDF, "*.pdf");

        // Создание зависимостей
        ILOG errorLog = new Loggi();
        IPDFParsing parsing = new PDFParsing(errorLog);
        IOrderNumber orderNumber = new OrderNumber();
        IDataBase dateBase = new DB(errorLog);

        // Создание PDFProcessor с использованием интерфейсов
        PDFProcessor pdfProcessor = new PDFProcessor(parsing, orderNumber, dateBase, errorLog);

        foreach (string str in arrayPDF)
        {
            pdfProcessor.Process(str);
        }
    }

}

public class PDFProcessor
{
    private readonly IPDFParsing _parsing;
    private readonly IOrderNumber _orderNumber;
    private readonly IDataBase _dateBase;
    private readonly ILOG _errorLog;

    public PDFProcessor(IPDFParsing parsing, IOrderNumber orderNumber, IDataBase dateBase, ILOG errorLog)
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


public interface IProcess 
{
    void Write(string PathPDF);
}

public class Process 
{
    private readonly IPDFParsing _parsing;
    private readonly OrderNumber _orderNumber;
    private readonly DB _dateBase;
    private readonly ILOG _errorLog;

    public Process(IPDFParsing parsing, OrderNumber orderNumber, DB dateBase, ILOG errorLog)
    {
        _parsing = parsing;
        _orderNumber = orderNumber;
        _dateBase = dateBase;
        _errorLog = errorLog;
    }

    public void Write(string PathPDF) 
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


public interface IPDFParsing 
{
    string Parse(string path);
}

public class PDFParsing : IPDFParsing
{

    private readonly ILOG _error;

    public PDFParsing(ILOG error)
    {
        _error = error;
    }

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
            _error.Log(path, error.ToString());
            return $"Ошибка в {path}";
        }
    }
}

public interface IOrderNumber 
{
    int? ResultNumber(string text);
}

public class OrderNumber : IOrderNumber
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

// Парсинг PDF
public interface IPDFReader 
{
    string Read(byte[] filePDF);
}

public class PRead : IPDFReader 
{
    public string Read(byte[] filePDF) 
    {
        StringBuilder sb = new StringBuilder();

        try
        {
            using(PdfReader reader = new PdfReader(filePDF)) 
            {
                for (int i = 1; i <= reader.NumberOfPages; i++) 
                {
                    sb.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Error: {ex}");
        }
        return sb.ToString();
    }
}

// База данных
public interface IDataBase 
{
    void WriteDB(int number, byte[] fileData);
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

// Логирование
public interface ILOG 
{
    void Log(string path, string error);
}

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