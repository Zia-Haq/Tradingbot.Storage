using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tradingbot.StorageTests
{
    public static class ConfigHelper
    {
        //"appsettings.test.json"
        public static IConfiguration InitConfiguration(string fileToLoad)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                AddJsonFile(fileToLoad, false, true).Build();

            return config;

        }
    }
}
