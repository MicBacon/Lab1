using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PT_Lab7;
[Serializable]
public class Program : IComparer<string>
{
    private static SortedDictionary<string, long> directElements;

    public static void Main(string[] args)
    {
        IComparer<string> comparer = new Program();
        directElements = new SortedDictionary<string, long>(comparer);
        var info = new FileInfo(args[0]);
        
        Console.WriteLine($"{Path.GetFileName(args[0])} ({Directory.GetFiles(args[0]).Length + Directory.GetDirectories(args[0]).Length}) {info.GetDOSAttributes()}");
        GetPrintFiles(args[0], "\t");
        PrintOldest(args[0]);
        LoadDirectElements(args[0]);
        SerializeData();
        DeserializeData();
    }

    private static void GetPrintFiles(string path, string indentation)
    {
        var currentDirectoryFiles = Directory.GetFiles(path);
        var currentDirectoryDirectories = Directory.GetDirectories(path);
        
        foreach (var d in currentDirectoryDirectories)
        {
            var info = new FileInfo(d);
            Console.Write(indentation);
            Console.WriteLine($"{Path.GetFileName(d)} ({Directory.GetFiles(d).Length + Directory.GetDirectories(d).Length}) {info.GetDOSAttributes()}");
            GetPrintFiles(d, indentation+"\t");
        }

        foreach (var f in currentDirectoryFiles)
        {
            var info = new FileInfo(f);
            Console.Write($"{indentation}\t");
            Console.WriteLine($"{Path.GetFileName(f)} {info.Length}B {info.GetDOSAttributes()}");
        }
    }

    private static void PrintOldest(string path)
    {
        var info = new DirectoryInfo(path);
        Console.WriteLine($"\nOldest file: {info.GetOldest()}");
    }

    private static void LoadDirectElements(string path)
    {
        var currentDirectoryFiles = Directory.GetFiles(path);
        var currentDirectoryDirectories = Directory.GetDirectories(path);

        foreach (var f in currentDirectoryFiles)
        {
            var info = new FileInfo(f);
            directElements.Add(Path.GetFileName(f), info.Length);
        }

        foreach (var d in currentDirectoryDirectories)
        {
            directElements.Add(Path.GetFileName(d), Directory.GetFiles(d).Length + Directory.GetDirectories(d).Length);
        }
    }

    private static void SerializeData()
    {
        var fs = new FileStream("SerializedData.dat", FileMode.Create);

        var formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(fs, directElements);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to serialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
    }

    private static void DeserializeData()
    {
        IComparer<string> comparer = new Program();
        var sortedDictionary = new SortedDictionary<string, long>(comparer);

        var fs = new FileStream("SerializedData.dat", FileMode.Open);
        try
        {
            var formatter = new BinaryFormatter();
            sortedDictionary = (SortedDictionary<string, long>)formatter.Deserialize(fs);
        }
        catch (SerializationException e)
        {
            Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
        
        Console.WriteLine();
        foreach (var f in sortedDictionary)
        {
            Console.WriteLine("{0} -> {1}", f.Key, f.Value);
        }
    }
    
    

    public int Compare(string? x, string? y)
    {
        if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
        {
            throw new ArgumentNullException();
        }

        if (x.Length > y.Length)
        {
            return 1;
        }

        if (x.Length != y.Length) return -1;
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] == y[i]) continue;
            if (x[i] - 48 < y[i] - 48)
            {
                return 1;
            }

            return -1;
        }

        return 0;
    }
}

public static class ExtensionMethods
{
    public static DateTime GetOldest(this DirectoryInfo dir)
    {
        var currentDirectoryFiles = dir.GetFiles();
        var currentDirectoryDirectories = dir.GetDirectories();
        var oldest = DateTime.MaxValue;

        foreach (var f in currentDirectoryFiles)
        {
            if (f.CreationTime < oldest)
            {
                oldest = f.CreationTime;
            }
        }
        
        foreach (var d in currentDirectoryDirectories)
        {
            if (d.CreationTime < oldest)
            {
                oldest = d.CreationTime;
            }
        }

        foreach (var d in currentDirectoryDirectories)
        {
            var o = GetOldest(d);
            if (o < oldest)
            {
                oldest = o;
            }
        }

        return oldest;
    }

    public static string GetDOSAttributes(this FileSystemInfo fsi)
    {
        var retStr = "";
        if ((fsi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        {
            retStr += "r";
        }
        else
        {
            retStr += "-";
        }
        
        if ((fsi.Attributes & FileAttributes.Archive) == FileAttributes.Archive)
        {
            retStr += "a";
        }
        else
        {
            retStr += "-";
        }
        
        if ((fsi.Attributes & FileAttributes.System) == FileAttributes.System)
        {
            retStr += "s";
        }
        else
        {
            retStr += "-";
        }
        
        if ((fsi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
        {
            retStr += "h";
        }
        else
        {
            retStr += "-";
        }

        return retStr;
    }
    
}

