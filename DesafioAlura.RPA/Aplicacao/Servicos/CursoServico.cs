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
using DesafioAlura.RPA.Dominio.Entidades;
using System.Globalization;

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

                    // Captura informações do curso na página principal
                    var curso = await CapturarInformacoesPaginaPrincipal();

                    // Insere o curso no banco de dados, se as informações forem capturadas com sucesso

                    // Continua com a navegação
                    ((IJavaScriptExecutor)_driver).ExecuteScript("window.open();");
                    _driver.SwitchTo().Window(_driver.WindowHandles.Last());
                    _driver.Navigate().GoToUrl(urlCurso);

                    await CapturarInformacoesDentroPagina(curso);

                    _wait.Until(drv => drv.FindElement(By.CssSelector("h1")));

                    if (curso != null)
                    {
                        await Consulta.InserirCursoLog(curso.Titulo, curso.Descricao, curso.Professor, curso.CargaHoraria, curso.UltimaAtualizacao, curso.PublicoAlvo);
                        Console.WriteLine("Curso inserido no banco de dados com sucesso.");
                    }

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


        private async Task<Curso> CapturarInformacoesPaginaPrincipal()
        {
            try
            {
                // Captura o título e a descrição do curso
                var titulo = _driver.FindElement(By.CssSelector("h4.busca-resultado-nome")).Text;
                var descricao = _driver.FindElement(By.CssSelector("p.busca-resultado-descricao")).Text;

                Console.WriteLine($"Título: {titulo}");
                Console.WriteLine($"Descrição: {descricao}");

                // Cria e retorna uma nova instância de Curso com as informações capturadas
                return new Curso
                {
                    Titulo = titulo,
                    Descricao = descricao
                };
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Erro ao capturar título ou descrição: Elemento não encontrado.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado ao capturar informações: {ex.Message}");
                return null;
            }
        }

        private async Task CapturarInformacoesDentroPagina(Curso curso)
        {

            try
            {
                // Captura o título e a descrição do curso
                var Professor = _driver.FindElement(By.CssSelector("h3.instructor-title--name")).Text;
                var CargaHoraria = _driver.FindElement(By.CssSelector("p.courseInfo-card-wrapper-infos")).Text;
                var UltimaAtualizacao = _driver.FindElement(By.CssSelector("div.course-container--update")).Text;
                var PublicoAlvo = _driver.FindElement(By.CssSelector("p.couse-text--target-audience")).Text;

                var UltimaAtualizacaoConvertida = ConverterDataUltimaAtualizacao(UltimaAtualizacao);

                Console.WriteLine($"Professor: {Professor}");
                Console.WriteLine($"Carga Horária: {CargaHoraria}");
                Console.WriteLine($"Última Atualização: {UltimaAtualizacaoConvertida}");
                Console.WriteLine($"Público Alvo: {PublicoAlvo}");

                // Cria e retorna uma nova instância de Curso com as informações capturadas
                curso.Professor = Professor;
                curso.CargaHoraria = CargaHoraria;
                curso.UltimaAtualizacao = UltimaAtualizacaoConvertida;
                curso.PublicoAlvo = PublicoAlvo;
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

        public DateTime ConverterDataUltimaAtualizacao(string ultimaAtualizacao)
        {
            // Remove o prefixo "Curso atualizado em " e extrai apenas a data
            string dataString = ultimaAtualizacao.Replace("Curso atualizado em ", "").Trim();

            // Converte a data extraída para DateTime
            if (DateTime.TryParseExact(dataString, "dd/MM/yyyy",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out DateTime dataConvertida))
            {
                return dataConvertida;
            }
            else
            {
                throw new FormatException("A data está em um formato incorreto.");
            }
        }
    }
}
