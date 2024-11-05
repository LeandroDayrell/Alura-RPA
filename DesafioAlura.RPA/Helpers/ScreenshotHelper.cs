using System;
using System.IO;
using OpenQA.Selenium;

namespace DesafioAlura.RPA.Helpers
{
    public class ScreenshotHelper
    {
        private static readonly string _diretorioCapturas = Path.Combine(Directory.GetCurrentDirectory(), "Capturas");

        static ScreenshotHelper()
        {
            if (!Directory.Exists(_diretorioCapturas))
            {
                Directory.CreateDirectory(_diretorioCapturas);
            }
        }

        public static void CapturarScreenshot(IWebDriver driver, string nomeArquivo)
        {
            try
            {
                if (driver is ITakesScreenshot screenshotDriver)
                {
                    string caminhoCompleto = Path.Combine(_diretorioCapturas, $"{nomeArquivo}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                    Screenshot screenshot = screenshotDriver.GetScreenshot();
                    byte[] imagemBytes = screenshot.AsByteArray;

                    File.WriteAllBytes(caminhoCompleto, imagemBytes);

                    Console.WriteLine($"Screenshot salva em: {caminhoCompleto}");
                }
                else
                {
                    Console.WriteLine("Erro: O driver fornecido não suporta captura de tela.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao capturar screenshot: {ex.Message}");
            }
        }
    }
}