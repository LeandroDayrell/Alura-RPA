using System;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using DesafioAlura.RPA.Helpers;
using DesafioAlura.RPA.Aplicacao;
using System.Linq;
using System.Collections.Generic;

namespace DesafioAlura.RPA.Aplicacao.Servicos
{
    public class CursoServico
    {
        private readonly string _urlBusca;
        private readonly string _palavraChave;
        private IWebDriver _driver;
        private WebDriverWait _wait;

        public CursoServico(string urlBusca, string palavraChave)
        {
            _urlBusca = urlBusca;
            _palavraChave = palavraChave;
        }

        public async Task RealizarBuscaCursos()
        {
            using (_driver = new ChromeDriver())
            {
                _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

                try
                {
                    NavegarParaPaginaDeBusca();
                    if (await ExecutarBuscaPalavraChave())
                    {
                        await VerificarEProcessarResultados();
                    }
                }
                catch (Exception ex)
                {
                    await RegistrarErro("Erro ao processar busca", ex);
                }
                finally
                {
                    _driver.Quit();
                }
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
            //await Consulta.InserirLog("Nenhum resultado encontrado para a palavra-chave: " + _palavraChave);
            File.AppendAllText("Log.txt", $"[{DateTime.Now}] Nenhum resultado encontrado para a palavra-chave: {_palavraChave}\n");
            Console.WriteLine("Nenhum resultado encontrado, log registrado.");
        }

        private async Task ProcessarCursosEncontrados()
        {
            // Localiza todos os cursos listados na busca
            var cursosEncontrados = _driver.FindElements(By.CssSelector("#busca-resultados .busca-resultado")); // Seletor para os cursos
            Console.WriteLine($"Cursos encontrados: {cursosEncontrados.Count}");

            for (int i = 0; i < cursosEncontrados.Count; i++)
            {
                try
                {
                    // Recarrega a lista de cursos antes de cada clique
                    cursosEncontrados = _driver.FindElements(By.CssSelector("#busca-resultados .busca-resultado"));

                    // Encontra o link do curso atual e clica para abrir os detalhes
                    var linkCurso = cursosEncontrados[i].FindElement(By.CssSelector("a.busca-resultado-link"));
                    string urlCurso = linkCurso.GetAttribute("href");
                    Console.WriteLine($"Acessando curso: {urlCurso}");

                    // Abre o link do curso em uma nova guia
                    ((IJavaScriptExecutor)_driver).ExecuteScript("window.open();");
                    _driver.SwitchTo().Window(_driver.WindowHandles.Last());
                    _driver.Navigate().GoToUrl(urlCurso);

                    // Aguarda que o título da página carregue
                    _wait.Until(drv => drv.FindElement(By.CssSelector("h1"))); // Ajuste o seletor conforme necessário

                    // Realiza a captura de informações do curso aqui
                    //var tituloCurso = _driver.FindElement(By.CssSelector("h1")).Text;
                    //var descricaoCurso = _driver.FindElement(By.CssSelector("p.descricao-curso")).Text; // Ajuste o seletor para descrição

                    //Console.WriteLine($"Título do Curso: {tituloCurso}");
                    //Console.WriteLine($"Descrição do Curso: {descricaoCurso}");

                    // Fecha a guia atual e volta para a guia de busca
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

        private async Task RegistrarErro(string mensagem, Exception ex)
        {
            ScreenshotHelper.CapturarScreenshot(_driver, "ErroExecucao");
            //await Consulta.InserirLog($"{mensagem}: {ex.Message}");
            File.AppendAllText("Log.txt", $"[{DateTime.Now}] {mensagem}: {ex.Message}\n");
            Console.WriteLine($"{mensagem} - Erro registrado e screenshot capturada.");
        }
    }
}