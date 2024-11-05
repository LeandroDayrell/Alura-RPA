using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace DesafioAlura.RPA.Helpers
{
    public static class ChromeDriverManager
    {
        private static readonly string ChromeDriverPath = Path.Combine(Directory.GetCurrentDirectory(), "Drivers");

        // Mantém a configuração do ChromeDriver conforme solicitado
        public static async Task<IWebDriver> GetConfiguredChromeDriver()
        {
            string chromeVersion = CheckChromeVersion();
            if (chromeVersion == null) throw new Exception("Google Chrome não está instalado.");

            // Obtenha a versão do ChromeDriver correspondente ou faça o download se necessário
            string driverVersion = await GetChromeDriverCorrespondente(chromeVersion);
            string driverPath = Path.Combine(ChromeDriverPath, driverVersion, "chromedriver.exe");

            // Verifica se o ChromeDriver existe no caminho especificado
            if (!File.Exists(driverPath))
            {
                // Se não existir, faz o download e extrai
                await BaixarDriver(driverVersion, false);
            }

            // Verifica novamente se o driver foi baixado corretamente
            if (!File.Exists(driverPath))
            {
                throw new FileNotFoundException($"ChromeDriver não encontrado no caminho esperado: {driverPath}");
            }

            // Configuração do ChromeDriverService
            ChromeDriverService chromeService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(driverPath), Path.GetFileName(driverPath));
            chromeService.SuppressInitialDiagnosticInformation = true;
            chromeService.HideCommandPromptWindow = true;

            // Configurações do Chrome
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--lang=pt-BR");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--profile-directory=RPA Ktech VIA");
            options.AddArgument("--silent");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--log-level=3");
            options.AddArgument("--log-path=");

            return new ChromeDriver(chromeService, options);
        }

        // Obtém apenas a versão principal
        private static string VersaoMajor(string versao)
        {
            return versao.Split('.')[0];
        }

        // Verifica a versão do Chrome instalada no sistema
        public static string CheckChromeVersion()
        {
            const string EmptyChromeVersion = null;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Google\\Chrome\\BLBeacon"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("Version");
                        if (val != null)
                        {
                            return val.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar a versão do Chrome: {ex.Message}");
            }
            return EmptyChromeVersion;
        }

        // Obtém a versão do ChromeDriver compatível com a versão do Chrome
        public static async Task<string> GetChromeDriverCorrespondente(string versaoChrome)
        {
            string result = string.Empty;
            var client = new HttpClient();
            try
            {
                var response = await client.GetAsync("https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions.json");
                response.EnsureSuccessStatusCode();

                string resultJson = await response.Content.ReadAsStringAsync();
                var jsonObjeto = JObject.Parse(resultJson);

                // Obtém a versão estável mais recente do Chrome
                string versaoStable = (string)jsonObjeto["channels"]["Stable"]["version"];

                if (VersaoMajor(versaoStable) == VersaoMajor(versaoChrome))
                {
                    return versaoStable;
                }
                return versaoStable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter a versão do ChromeDriver: {ex.Message}");
            }
            return result;
        }

        public static async Task<string> BaixarDriver(string versaoDriver, bool driverNaRaiz = true)
        {
            string filePath = driverNaRaiz
                ? $"{AppDomain.CurrentDomain.BaseDirectory}/chromedriver.zip"
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Drivers", versaoDriver, "chromedriver.zip");

            if (!driverNaRaiz)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Erro ao criar o diretório do ChromeDriver."));
            }

            Uri driverUrl = new Uri(GetUrlDriver(versaoDriver));
            var client = new HttpClient();

            try
            {
                var response = await client.GetAsync(driverUrl);
                response.EnsureSuccessStatusCode();

                await using var file = File.OpenWrite(filePath);
                await response.Content.CopyToAsync(file);

                Console.WriteLine($"ChromeDriver {versaoDriver} baixado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar o ChromeDriver: {ex.Message}");
            }

            // Extrai o arquivo baixado diretamente na pasta da versão
            string extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Drivers", versaoDriver);
            ZipFile.ExtractToDirectory(filePath, extractPath, true);

            // Remove subpastas como "chromedriver-win64" e deixa apenas "chromedriver.exe" na pasta da versão
            var subDir = Path.Combine(extractPath, "chromedriver-win64");
            if (Directory.Exists(subDir))
            {
                foreach (var file in Directory.GetFiles(subDir))
                {
                    File.Move(file, Path.Combine(extractPath, Path.GetFileName(file)), true);
                }
                Directory.Delete(subDir, true);
            }

            File.Delete(filePath);

            return extractPath;
        }

        private static string GetUrlDriver(string versao)
        {
            string base_url = "https://storage.googleapis.com/chrome-for-testing-public/";
            string platform = "win";
            string architecture = "64";

            return $"{base_url}{versao}/{platform}{architecture}/chromedriver-{platform}{architecture}.zip";
        }
    }
}