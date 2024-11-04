using System.Collections.Generic;
using DesafioAlura.RPA.Dominio.Entidades;

namespace DesafioAlura.RPA.Dominio.Interfaces
{
    public interface ICursoRepositorio
    {
        void AdicionarCurso(Curso curso);
        IEnumerable<Curso> ObterCursos();
    }
}