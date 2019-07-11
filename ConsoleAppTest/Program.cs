using GPhotosClientApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new GPhotosClient(
                @"C:\tmp\credentials.json", 
                @"C:\tmp\tokenStore",
                "photomaton");

            var albums = client.GetAllAlbums().Result;
            albums.ForEach(a => Console.WriteLine(a.title));

            // exit code
            Console.WriteLine("press a key to exit...");
            Console.Read();
        }
    }
}
