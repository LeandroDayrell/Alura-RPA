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

        public async Task InserirCursoLog(long cursoId, string cursoNome, string cursoDescricao)
        {
            using (var dbConn = new SqlConnection(_connectionString))
            {
                try
                {
                    await dbConn.OpenAsync();
                    var parametros = new DynamicParameters();
                    parametros.Add("@CURSO_ID", cursoId);
                    parametros.Add("@CURSO_NOME", cursoNome);
                    parametros.Add("@CURSO_DESCRICAO", cursoDescricao);

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