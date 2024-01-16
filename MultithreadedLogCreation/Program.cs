using System.Configuration;

namespace MultithreadedLogCreation;

internal class Program
{
    private static string? _path;
    private static string? _fileName;
    static int _nTimesToExecute = 10;
    static int _nThreadCount = 11;

    static void Main(string[] args)
    {
        try
        {
            _path = ConfigurationManager.AppSettings["path"]!;
            _fileName = ConfigurationManager.AppSettings["fileName"]!;
            _nTimesToExecute = int.Parse(ConfigurationManager.AppSettings["timesToExecute"]!);
            _nThreadCount = int.Parse(ConfigurationManager.AppSettings["threadCount"]!);

            var qualifiedFileName = $"{_path}\\{_fileName}";
            StartThreads(qualifiedFileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("All threads complete");
    }

    public static void StartThreads(string fullyQualifiedFileName)
    {
        CheckExistenceOfDirectoryAndFile(_path, fullyQualifiedFileName);

        var workerThreads = new Thread[_nThreadCount];

        for (var k = 0; k < _nThreadCount; ++k)
        {
            workerThreads[k] = new Thread(() => ProcessWrite(fullyQualifiedFileName));
            workerThreads[k].Start();
        }

        /* Wait for all Threads to finish */
        for (var k = 0; k < _nThreadCount; ++k)
        {
            workerThreads[k].Join();
        }
    }

    private static void ProcessWrite(string fileName)
    {
        for (var k = 0; k < _nTimesToExecute; ++k)
        {
            WriteToFile(fileName, Environment.CurrentManagedThreadId);
        }
    }

    // Check existence of directory and file, if not found create it.
    public static void CheckExistenceOfDirectoryAndFile(string? path, string fileName)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (File.Exists(fileName))
        {
            DeleteFileIfFound(fileName);
        }

        File.Create(fileName).Close();
    }

    public static void DeleteFileIfFound(string fileName)
    {
        File.Delete(fileName);
    }

    private static void WriteToFile(string filePathAndName, int threadId)
    {
        using var mutex = new Mutex(false, filePathAndName.Replace("\\", ""));
        var hasHandle = false;
        try
        {
            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
            var fileContents = $"{GlobalCount.Count} {threadId} {DateTime.Now:HH:mm:ss.fff}";
            if (GlobalCount.Count > 100)
                return;
            File.AppendAllText(filePathAndName, fileContents + Environment.NewLine);
            GlobalCount.Count += 1;
        }
        finally
        {
            if (hasHandle)
                mutex.ReleaseMutex();
        }
    }
}
