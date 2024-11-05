using System.Threading.Tasks;
using DesafioAlura.RPA.Aplicacao.Servicos;
using DesafioAlura.RPA.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using System.IO;

class Program
{
    static async Task Main(string[] args)
    {
        // Configuração do IConfigurationBuilder para carregar as configurações do appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // Configuração do ServiceProvider com IConfiguration e demais serviços
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<ChromeDriverManager>()
            .AddSingleton<ConfigurationHelper>()
            .AddSingleton<InfoHelper>()
            .AddSingleton<ScreenshotHelper>()
            .AddSingleton<IWebDriver>(provider => ChromeDriverManager.GetConfiguredChromeDriver().Result)
            .AddScoped<CursoServico>()
            .BuildServiceProvider();

        Console.WriteLine(InfoHelper.Intro);
        Console.WriteLine($"Versão: {InfoHelper.GetBuildVersion()}");
        Console.Title = InfoHelper.Title;

        await VerificarInternet();

        // Resolve as dependências através do ServiceProvider
        var cursoServico = serviceProvider.GetRequiredService<CursoServico>();

        try
        {
            // Executa o processo de busca usando injeção de dependência
            await cursoServico.RealizarBuscaCursos();
        }
        finally
        {
            // Fecha o driver no final da execução
            var driver = serviceProvider.GetRequiredService<IWebDriver>();
            driver.Quit();
        }
    }

    static async Task VerificarInternet()
    {
        while (!InfoHelper.Internet().Result)
        {
            Console.WriteLine("Sem acesso a internet, tentando novamente em 10 segundos...");
            await Task.Delay(10 * 1000);
        }
    }
}