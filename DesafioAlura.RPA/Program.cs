using System.Threading.Tasks;
using DesafioAlura.RPA.Aplicacao.Servicos;
using DesafioAlura.RPA.Helpers;

class Program
{
    static async Task Main(string[] args)
    {
        // Carrega a URL e a palavra-chave do appsettings.json
        var configuracao = ConfigurationHelper.GetConfiguration();
        string urlBusca = configuracao["Configuracoes:UrlBusca"];
        string palavraChave = configuracao["Configuracoes:PalavraChave"];

        // Instancia o serviço com a URL e a palavra-chave
        var cursoServico = new CursoServico(urlBusca, palavraChave);

        // Executa o processo de busca
        await cursoServico.RealizarBuscaCursos();
    }
}