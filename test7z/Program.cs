using System;

namespace test7z
{
    class Program
    {
        static void Main(string[] args)
        {
            var bytes = System.IO.File.ReadAllBytes("d:\\neo\\2.7z");
            SevenZip.Pack.SevenzipFile file = new SevenZip.Pack.SevenzipFile(bytes);
            var unpack = file.Unpack();
            System.IO.File.WriteAllBytes("d:\\neo\\2.json", unpack);
            Console.WriteLine("Hello World!");
        }
    }
}
