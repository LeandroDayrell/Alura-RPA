using System.Threading.Tasks;
using DesafioAlura.RPA.Helpers;
using DesafioAlura.RPA.Infraestrutura.Repositorios;

namespace DesafioAlura.RPA.Aplicacao
{
    public static class Consulta
    {
        public static async Task InserirCursoLog(long cursoId, string cursoNome, string cursoDescricao)
        {
            var cursoRepository = new CursoRepository(ConfigurationHelper.GetConnectionString());
            await cursoRepository.InserirCursoLog(cursoId, cursoNome, cursoDescricao);
        }
    }
}