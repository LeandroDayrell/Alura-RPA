using System;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using DesafioAlura.RPA.Helpers;
using DesafioAlura.RPA.Aplicacao;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DesafioAlura.RPA.Aplicacao.Servicos
{
    public class CursoServico
    {
        private readonly string _urlBusca;
        private readonly string _palavraChave;
        private readonly IWebDriver _driver;
        private WebDriverWait _wait;

        public CursoServico(IConfiguration configuration, IWebDriver driver)
        {
            _urlBusca = configuration["Configuracoes:UrlBusca"];
            _palavraChave = configuration["Configuracoes:PalavraChave"];
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        public async Task RealizarBuscaCursos()
        {
            try
            {
                NavegarParaPaginaDeBusca();
                if (await ExecutarBuscaPalavraChave())
                {
                    await AdicionarFiltro(_driver);
                    await VerificarEProcessarResultados();
                }
            }
            catch (Exception ex)
            {
                await RegistrarErro("Erro ao processar busca", ex);
            }
        }

        private void NavegarParaPaginaDeBusca()
        {
            _driver.Navigate().GoToUrl(_urlBusca);
            Console.WriteLine("Navegando para a página de busca.");
        }

        private async Task<bool> ExecutarBuscaPalavraChave()
        {
            try
            {
                var campoBusca = _wait.Until(drv => drv.FindElement(By.Id("header-barraBusca-form-campoBusca")));
                campoBusca.SendKeys(_palavraChave);

                var botaoBusca = _driver.FindElement(By.CssSelector(".header__nav--busca-submit"));
                botaoBusca.Click();

                _wait.Until(drv => drv.FindElement(By.CssSelector("h2.busca__title")).Displayed);
                Console.WriteLine("Busca executada com a palavra-chave.");
                return true;
            }
            catch (Exception ex)
            {
                await RegistrarErro("Erro ao executar a busca", ex);
                return false;
            }
        }

        private async Task VerificarEProcessarResultados()
        {
            var mensagemSemResultados = _driver.FindElements(By.CssSelector("#busca-resultados > ul")).FirstOrDefault();
            if (mensagemSemResultados == null)
            {
                await RegistrarMensagemNenhumResultado();
                return;
            }

            ScreenshotHelper.CapturarScreenshot(_driver, "Resultados_Busca");
            Console.WriteLine("Cursos encontrados.");

            await ProcessarCursosEncontrados();
        }

        private async Task RegistrarMensagemNenhumResultado()
        {
            ScreenshotHelper.CapturarScreenshot(_driver, "Nenhum_Resultado_Encontrado");
            File.AppendAllText("Log.txt", $"[{DateTime.Now}] Nenhum resultado encontrado para a palavra-chave: {_palavraChave}\n");
            Console.WriteLine("Nenhum resultado encontrado, log registrado.");
        }

        private async Task ProcessarCursosEncontrados()
        {
            var cursosEncontrados = _driver.FindElements(By.CssSelector("#busca-resultados .busca-resultado"));
            Console.WriteLine($"Cursos encontrados: {cursosEncontrados.Count}");

            for (int i = 0; i < cursosEncontrados.Count; i++)
            {
                try
                {
                    cursosEncontrados = _driver.FindElements(By.CssSelector("#busca-resultados .busca-resultado"));
                    var linkCurso = cursosEncontrados[i].FindElement(By.CssSelector("a.busca-resultado-link"));
                    string urlCurso = linkCurso.GetAttribute("href");
                    Console.WriteLine($"Acessando curso: {urlCurso}");

                    CapturarInformacoesPaginaPrincipal();

                    ((IJavaScriptExecutor)_driver).ExecuteScript("window.open();");
                    _driver.SwitchTo().Window(_driver.WindowHandles.Last());
                    _driver.Navigate().GoToUrl(urlCurso);

                    _wait.Until(drv => drv.FindElement(By.CssSelector("h1")));

                    CapturarInformacoesDentroPagina();

                    _driver.Close();
                    _driver.SwitchTo().Window(_driver.WindowHandles.First());

                    _wait.Until(drv => drv.FindElement(By.CssSelector("h2.busca__title")).Displayed);
                }
                catch (Exception ex)
                {
                    await RegistrarErro("Erro ao processar curso", ex);
                }
            }
        }

        private async Task CapturarInformacoesPaginaPrincipal()
        {
            try
            {
                // Localiza o título e a descrição do curso na página principal de resultados
                var titulo = _driver.FindElement(By.CssSelector("h4.busca-resultado-nome")).Text;
                var descricao = _driver.FindElement(By.CssSelector("p.busca-resultado-descricao")).Text;

                Console.WriteLine($"Título: {titulo}");
                Console.WriteLine($"Descrição: {descricao}");
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Erro ao capturar título ou descrição: Elemento não encontrado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado ao capturar informações: {ex.Message}");
            }
        }

        private async Task CapturarInformacoesDentroPagina()
        {
            var cargaHoraria = _driver.FindElement(By.CssSelector(".carga-horaria")).Text;
            var preco = _driver.FindElement(By.CssSelector(".preco")).Text;

            Console.WriteLine($"Carga Horária: {cargaHoraria}");
            Console.WriteLine($"Preço: {preco}");
        }

        private async Task AdicionarFiltro(IWebDriver driver)
        {
            var configuracao = ConfigurationHelper.GetConfiguration();
            string tipoDeCurso = configuracao["Configuracoes:TipoDeCurso"];

            try
            {
                var botaoFiltro = _driver.FindElement(By.CssSelector("#busca-form > span"));
                botaoFiltro.Click();

                var filtros = _driver.FindElements(By.CssSelector("#busca--filtros--tipos .busca--filtro--nome-filtro"));
                var filtroSelecionado = filtros.FirstOrDefault(f => f.Text.Trim().Equals(tipoDeCurso, StringComparison.OrdinalIgnoreCase));

                if (filtroSelecionado != null)
                {
                    filtroSelecionado.Click();
                    Console.WriteLine($"Filtro aplicado: {tipoDeCurso}");
                }
                else
                {
                    Console.WriteLine($"Filtro '{tipoDeCurso}' não encontrado.");
                }

                var botaoPesquisarComFiltro = _driver.FindElement(By.CssSelector("input.busca-form-botao.--desktop"));
                botaoPesquisarComFiltro.Click();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao aplicar filtro: {ex.Message}");
                await RegistrarErro("Erro ao aplicar filtro", ex);
            }
        }

        private async Task RegistrarErro(string mensagem, Exception ex)
        {
            ScreenshotHelper.CapturarScreenshot(_driver, "ErroExecucao");
            File.AppendAllText("Log.txt", $"[{DateTime.Now}] {mensagem}: {ex.Message}\n");
            Console.WriteLine($"{mensagem} - Erro registrado e screenshot capturada.");
        }
    }
}
