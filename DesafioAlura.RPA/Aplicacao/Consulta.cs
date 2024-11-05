using System.Threading.Tasks;
using DesafioAlura.RPA.Helpers;
using DesafioAlura.RPA.Infraestrutura.Repositorios;

namespace DesafioAlura.RPA.Aplicacao
{
    public static class Consulta
    {
        public static async Task InserirCursoLog(string cursoTitulo, string cursoDescricao, string cursoProfessor, string cursoCargaHoraria, DateTime cursoUltimaAtualizacao, string cursoPublicoAlvo)
        {
            var cursoRepository = new CursoRepository(ConfigurationHelper.GetConnectionString());
            await cursoRepository.InserirCursoLog(cursoTitulo, cursoDescricao, cursoProfessor, cursoCargaHoraria, cursoUltimaAtualizacao, cursoPublicoAlvo);
        }
    }
}