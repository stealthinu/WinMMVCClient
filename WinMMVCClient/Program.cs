using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace WinMMVCClient
{
    internal static class Program
    {
        public static IConfiguration conf { get; private set; }

        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            conf = builder.Build();
            Application.Run(new MainForm(conf));
        }
    }
}