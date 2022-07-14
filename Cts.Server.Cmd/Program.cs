using Gemelo.Components.Cts.WebApiHost;
using System;

namespace Gemelo.Applications.Cts.Server.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            CtsWebApiHost.MediaDirectoryPath = @"C:\+Daten\Cts\Media";

            Console.WriteLine("Deutsches Auswandererhaus - CTS-Server");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("(C)opyright 2021 gemelo GmbH");
            Console.WriteLine();
            Console.WriteLine($"Media directory: {CtsWebApiHost.MediaDirectoryPath}");
            Console.WriteLine();

            CtsWebApiHost.Start();

            Console.ReadLine();
        }
    }
}
