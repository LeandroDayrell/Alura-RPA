using Microsoft.Extensions.Configuration;
using System.IO;

namespace DesafioAlura.RPA.Helpers
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _configuracao;

        static ConfigurationHelper()
        {
            _configuracao = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static string GetConnectionString()
        {
            return _configuracao["Configuracoes:StringConexaoBanco"];
        }

        public static IConfiguration GetConfiguration()
        {
            return _configuracao;
        }
    }
}