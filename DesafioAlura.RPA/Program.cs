using System.Threading.Tasks;
using DesafioAlura.RPA.Aplicacao.Servicos;
using DesafioAlura.RPA.Helpers;
using OpenQA.Selenium;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine(InfoHelper.Intro);
        Console.WriteLine(string.Format("Versão: {0}", InfoHelper.GetBuildVersion()));
        Console.Title = InfoHelper.Title;

        await VerificarInternet();

        // Inicializa o driver com ChromeDriverManager
        IWebDriver driver = await ChromeDriverManager.GetConfiguredChromeDriver();

        try
        {
            var configuracao = ConfigurationHelper.GetConfiguration();
            string urlBusca = configuracao["Configuracoes:UrlBusca"];
            string palavraChave = configuracao["Configuracoes:PalavraChave"];

            // Instancia o serviço com a URL, palavra-chave e o driver inicializado
            var cursoServico = new CursoServico(urlBusca, palavraChave, driver);

            // Executa o processo de busca
            await cursoServico.RealizarBuscaCursos();
        }
        finally
        {
            // Fecha o driver no final da execução
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