namespace DesafioAlura.RPA.Dominio.Entidades
{
    public class Curso
    {
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Professor { get; set; }
        public string CargaHoraria { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
        public string PublicoAlvo { get; set; }
    }
}