using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using DesafioAlura.RPA.Dominio.Entidades;
using DesafioAlura.RPA.Aplicacao;

namespace DesafioAlura.RPA.Aplicacao.Servicos
{
    public class CursoServico
    {
        private readonly string _urlBusca;

        public CursoServico(string urlBusca)
        {
            _urlBusca = urlBusca;
        }

        public async Task RealizarBuscaCursos()
        {
            using IWebDriver driver = new ChromeDriver();

            // Navega até a pagina que defini dentro do appsettings
            driver.Navigate().GoToUrl(_urlBusca);


            //driver.Quit();
        }
    }
}