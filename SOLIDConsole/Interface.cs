namespace Interface 
{
    public interface IPDFParsing
    {
        string Parse(string path);
    }

    public interface IProcess
    {
        void Write(string PathPDF);
    }

    public interface IOrderNumber
    {
        int? ResultNumber(string text);
    }

    public interface IPDFReader
    {
        string Read(byte[] filePDF);
    }

    public interface IDataBase
    {
        void WriteDB(int number, byte[] fileData);
    }

    public interface ILOG
    {
        void Log(string path, string error);
    }
}