using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace DesafioAlura.RPA.Infraestrutura.Repositorios
{
    public class CursoRepository
    {
        private readonly string _connectionString;

        public CursoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InserirCursoLog(string cursoTitulo, string cursoDescricao, string cursoProfessor, string cursoCargaHoraria, DateTime cursoUltimaAtualizacao, string cursoPublicoAlvo)
        {
            using (var dbConn = new SqlConnection(_connectionString))
            {
                try
                {
                    await dbConn.OpenAsync();
                    var parametros = new DynamicParameters();
                    parametros.Add("@CURSO_TITULO", cursoTitulo);
                    parametros.Add("@CURSO_DESCRICAO", cursoDescricao);
                    parametros.Add("@CURSO_PROFESSOR", cursoProfessor);
                    parametros.Add("@CURSO_CARGAHORARIO", cursoCargaHoraria);
                    parametros.Add("@CURSO_ULTIMAATUALIZACAO", cursoUltimaAtualizacao);
                    parametros.Add("@CURSO_PUBLICOALVO", cursoPublicoAlvo);

                    await dbConn.ExecuteAsync("SP_INSERIR_CURSO", parametros, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao executar a stored procedure: {ex.Message}");
                }
                finally
                {
                    if (dbConn.State == ConnectionState.Open)
                    {
                        await dbConn.CloseAsync();
                    }
                    SqlConnection.ClearAllPools();
                }
            }
        }
    }
}