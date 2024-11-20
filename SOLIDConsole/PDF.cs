using Interface;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using System.Text;
using DateBase;

namespace PDF 
{
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

    public class PRead : IPDFReader
    {
        public string Read(byte[] filePDF)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                using (PdfReader reader = new PdfReader(filePDF))
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
}