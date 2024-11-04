using System.Threading.Tasks;
using DesafioAlura.RPA.Aplicacao.Servicos;
using DesafioAlura.RPA.Helpers;

class Program
{
    static async Task Main(string[] args)
    {
        string urlBusca = ConfigurationHelper.GetConfiguration()["Configuracoes:UrlBusca"];
        var cursoServico = new CursoServico(urlBusca);

        await cursoServico.RealizarBuscaCursos();
    }
}