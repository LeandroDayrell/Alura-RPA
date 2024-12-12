# Automação de Busca e Registro de Cursos

## Descrição do Projeto

Este projeto de estudo realiza a automação de busca e registro de informações de cursos a partir de um site específico, utilizando **Selenium** para navegação automatizada e **Dapper** para integração com banco de dados SQL Server. A aplicação segue os princípios da **Arquitetura DDD** (Domain-Driven Design) e foi construída com foco em escalabilidade e manutenção.

## Tecnologias Utilizadas

- **C# .NET Core**: Plataforma principal do projeto.
- **Selenium.WebDriver e Selenium.WebDriver.ChromeDriver**: Automação e manipulação de páginas web.
- **Dapper**: Facilita a comunicação com o banco de dados.
- **System.Data.SqlClient**: Gerenciamento da conexão com o SQL Server.
- **Microsoft.Extensions.Configuration**: Centralização de configurações com `appsettings.json`.
- **Newtonsoft.Json**: Manipulação e validação de dados JSON.

## Estrutura do Projeto

### 1. Configuração e Inicialização

No **Program.cs**, são executadas as seguintes etapas:

- **Carregamento de Configurações**: Configurações são obtidas a partir do `appsettings.json` (URL, palavra-chave e conexão com o banco de dados).
- **Configuração de Serviços**: Inicialização dos principais serviços (navegador, captura de tela e serviços de curso).
- **Verificação de Conexão**: Garantia de conectividade com a internet antes de prosseguir.
- **Execução do Serviço**: Inicia a busca automatizada com o serviço `CursoServico`.
- **Encerramento de Processos**: Fecha o navegador ao final da execução.

### 2. Fluxo de Automação

No **CursoServico.cs**, o fluxo principal ocorre em etapas:

1. **Busca Inicial**  
   - Acessa a URL configurada e realiza a busca com a palavra-chave fornecida.

2. **Aplicação de Filtros**  
   - Aplica filtros opcionais para restringir os resultados (ex.: Cursos ou Formações).

3. **Processamento de Resultados**  
   - Verifica a presença de cursos. Em caso de ausência, registra um log e tira uma captura de tela.

4. **Coleta de Dados**  
   - Para cada curso encontrado:
     - Captura título, descrição e informações detalhadas.
     - Acessa a página individual, se necessário, para coletar informações extras.

5. **Persistência no Banco de Dados**  
   - Os dados são salvos no banco de dados utilizando uma **stored procedure**.

6. **Tratamento de Erros**  
   - Registra logs de erros no console e no arquivo `Log.txt`.
   - Captura screenshots dos erros para facilitar diagnósticos.

## Configurações

O arquivo `appsettings.json` centraliza as principais configurações do projeto:

```json
"Configuracoes": {
  "StringConexaoBanco": "String de Conexao com banco de dados",
  "UrlBusca": "https://www.alura.com.br/",
  "PalavraChave": "rpa",
  "TipoDeCurso": "Cursos"
}

