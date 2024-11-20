using Interface;
using PDF;
using DateBase;

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
