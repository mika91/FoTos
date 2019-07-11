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
                @"C:\Users\mika\Documents\PhotoBox\FoTos\output\private\credentials.json", 
                @"C:\Users\mika\Documents\PhotoBox\FoTos\output\private\tokenStore",
                "photomaton");

            var albums = client.GetAllAlbums().Result;

            // exit code
            Console.WriteLine("press a key to exit...");
            Console.Read();
        }
    }
}
