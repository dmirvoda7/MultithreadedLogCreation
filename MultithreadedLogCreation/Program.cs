

namespace MultithreadedLogCreation;


internal class Program
{
    const int NThreadCount = 11;
    const string Path = @"c:\junk\log";
    const string FileName = "out.txt";
    const int NTimesToExecute = 9;
    
    static void Main(string[] args)
    {
        var qualifiedFileName = $"{Path}\\{FileName}";
        try
        {            
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
        CheckExistanceOfDirectoryAndFile(Path, fullyQualifiedFileName);

        Thread[] workerThreads = new Thread[NThreadCount];            

        for (var k = 0; k < NThreadCount; ++k)
        {                           
            workerThreads[k] = new Thread(() => ProcessWrite(fullyQualifiedFileName));
            workerThreads[k].Start();                
        }

        /* Wait for all Threads to finish */
        for (var k = 0; k < NThreadCount; ++k)
        {
            workerThreads[k].Join();
        }
    }

    private static void ProcessWrite(string fileName)
    {
        for (var k = 0; k < NTimesToExecute; ++k)
        {           
            WriteToFile(fileName, Environment.CurrentManagedThreadId);
        }       
    }

    // Check existence of directory and file, if not found create it.
    public static void CheckExistanceOfDirectoryAndFile(string path, string fileName)
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

    private static void WriteToFile( string filePathAndName, int threadId)
    {
        using var mutex = new Mutex(false, filePathAndName.Replace("\\", ""));
        var hasHandle = false;
        try
        {
            hasHandle = mutex.WaitOne(Timeout.Infinite, false);                
            var fileContents = $"{GlobalCount.Count} {threadId} {DateTime.Now:HH:mm:ss.fff}";
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
