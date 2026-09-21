# AIConnection

API em **ASP.NET Core (.NET 10)** que centraliza o envio de prompts para quatro LLMs diferentes (**GPT**, **Claude**, **Gemini** e **DeepSeek**) e converte automaticamente cada resposta em um arquivo `.cs`. O projeto é a infraestrutura de coleta de dados de um Trabalho de Conclusão de Curso (TCC) que avalia a qualidade do código C# gerado por LLMs.

## O que o projeto faz

A API expõe um endpoint por modelo. Cada requisição recebe um prompt de programação junto com metadados do experimento (qual questão está sendo respondida, a senioridade do participante que originou o prompt e o identificador do participante) e devolve a solução gerada pelo modelo, já extraída e salva como um arquivo de classe C# organizado em pastas.

Fluxo de uma requisição:

1. O cliente envia um `POST` para a rota do modelo desejado com o prompt e os metadados (`LargeLanguageModelType`, `QuestionType`, `SeniorityType`, `Participant`).
2. O `AIController` encaminha o prompt ao respectivo cliente de chat (`IChatClient` da Microsoft.Extensions.AI para GPT/Gemini/DeepSeek, ou um `ClaudeService` HTTP dedicado para Claude, já que a Anthropic ainda não possui adapter oficial para essa abstração).
3. O `ClassGenerationService` interpreta o texto de resposta do modelo, localiza o bloco de código (procurando marcadores como "CÓDIGO COMPLETO", "SOLUÇÃO RECOMENDADA" ou blocos ```csharp), extrai a classe, normaliza indentação e namespace, formata o código com o **Roslyn** (`Microsoft.CodeAnalysis.CSharp`) e grava o arquivo `.cs` resultante.
4. O arquivo é salvo em `GeneratedCode/{Modelo}/{Questão}/Participant_{N}/`, criando uma estrutura idêntica para todos os modelos e participantes.

## Modelos integrados

| Modelo | Provedor / SDK |
|---|---|
| GPT (`gpt-5.1`) | OpenAI, via `Microsoft.Extensions.AI` |
| Claude (`claude-sonnet-4-5`) | Chamada HTTP direta à API da Anthropic (`ClaudeService`) |
| Gemini (`gemini-2.5-pro`) | `GeminiDotnet.Extensions.AI` |
| DeepSeek (`deepseek-coder`) | Client OpenAI-compatible apontando para `api.deepseek.com` |

As chaves de API são lidas de variáveis de ambiente: `OPENAI_API_KEY`, `CLAUDE_API_KEY`, `GOOGLE_API_KEY` e `DEEPSEEK_API_KEY`.

## Endpoints

Todos os endpoints são `POST` e recebem um `LargeLanguageModelRequest` (`LargeLanguageModel`, `QuestionIdentifier`, `Seniority`, `Participant`, `Prompt`):

- `api/AI/geracao-codigo/openAI/gtp5`
- `api/AI/geracao-codigo/claude/sonnet4.5`
- `api/AI/geracao-codigo/google/gemini2.5`
- `api/AI/geracao-codigo/deepseek/deepseek-code`

## Desenho do experimento

O projeto foi estruturado para gerar **84 projetos** de código (4 modelos × 3 questões × 7 participantes), refletidos diretamente na árvore de pastas `GeneratedCode/` declarada no `.csproj`:

- **Modelos:** GPT, Claude, Gemini, DeepSeek
- **Questões (`QuestionType`):** `ArrayDifference`, `SequenceComparison`, `StringArrayEncoding`
- **Participantes (`SeniorityType`):** 7 participantes, com senioridades que vão de Júnior a Especialista

## Prompts utilizados

Os 21 prompts (7 participantes × 3 questões) foram extraídos da [Postman Collection](https://github.com/IngridBatista/AIConnection/blob/main/Dataset_Pesquisa.postman_collection.json) usada para popular a API. Cada prompt foi enviado, sem alterações, aos quatro endpoints (GPT, Claude, Gemini, DeepSeek), devido a isto o mesmo texto gera as 4 soluções comparadas para aquele participante/questão, totalizando as 84 execuções.


## Quantidade de execuções por modelo, questão e participante

A tabela abaixo traz o total de execuções registrado na planilha de acompanhamento do experimento para cada uma das 84 combinações de modelo × questão × participante (número de tentativas necessárias até obter uma resposta válida, incluindo as que resultaram em erro):

| Modelo | Questão | Participante | Total de Execuções |
|---|---|---|---|
| GPT | ArrayDifference | Participant 1 | 14 |
| GPT | ArrayDifference | Participant 2 | 6 |
| GPT | ArrayDifference | Participant 3 | 2 |
| GPT | ArrayDifference | Participant 4 | 2 |
| GPT | ArrayDifference | Participant 5 | 2 |
| GPT | ArrayDifference | Participant 6 | 2 |
| GPT | ArrayDifference | Participant 7 | 2 |
| GPT | SequenceComparison | Participant 1 | 6 |
| GPT | SequenceComparison | Participant 2 | 4 |
| GPT | SequenceComparison | Participant 3 | 2 |
| GPT | SequenceComparison | Participant 4 | 2 |
| GPT | SequenceComparison | Participant 5 | 3 |
| GPT | SequenceComparison | Participant 6 | 2 |
| GPT | SequenceComparison | Participant 7 | 2 |
| GPT | StringArrayEncoding | Participant 1 | 11 |
| GPT | StringArrayEncoding | Participant 2 | 3 |
| GPT | StringArrayEncoding | Participant 3 | 2 |
| GPT | StringArrayEncoding | Participant 4 | 2 |
| GPT | StringArrayEncoding | Participant 5 | 3 |
| GPT | StringArrayEncoding | Participant 6 | 2 |
| GPT | StringArrayEncoding | Participant 7 | 2 |
| Claude | ArrayDifference | Participant 1 | 2 |
| Claude | ArrayDifference | Participant 2 | 2 |
| Claude | ArrayDifference | Participant 3 | 1 |
| Claude | ArrayDifference | Participant 4 | 1 |
| Claude | ArrayDifference | Participant 5 | 2 |
| Claude | ArrayDifference | Participant 6 | 2 |
| Claude | ArrayDifference | Participant 7 | 1 |
| Claude | SequenceComparison | Participant 1 | 5 |
| Claude | SequenceComparison | Participant 2 | 1 |
| Claude | SequenceComparison | Participant 3 | 14 |
| Claude | SequenceComparison | Participant 4 | 1 |
| Claude | SequenceComparison | Participant 5 | 1 |
| Claude | SequenceComparison | Participant 6 | 1 |
| Claude | SequenceComparison | Participant 7 | 1 |
| Claude | StringArrayEncoding | Participant 1 | 2 |
| Claude | StringArrayEncoding | Participant 2 | 1 |
| Claude | StringArrayEncoding | Participant 3 | 1 |
| Claude | StringArrayEncoding | Participant 4 | 2 |
| Claude | StringArrayEncoding | Participant 5 | 1 |
| Claude | StringArrayEncoding | Participant 6 | 1 |
| Claude | StringArrayEncoding | Participant 7 | 2 |
| Gemini | ArrayDifference | Participant 1 | 2 |
| Gemini | ArrayDifference | Participant 2 | 1 |
| Gemini | ArrayDifference | Participant 3 | 1 |
| Gemini | ArrayDifference | Participant 4 | 1 |
| Gemini | ArrayDifference | Participant 5 | 5 |
| Gemini | ArrayDifference | Participant 6 | 2 |
| Gemini | ArrayDifference | Participant 7 | 1 |
| Gemini | SequenceComparison | Participant 1 | 2 |
| Gemini | SequenceComparison | Participant 2 | 1 |
| Gemini | SequenceComparison | Participant 3 | 1 |
| Gemini | SequenceComparison | Participant 4 | 4 |
| Gemini | SequenceComparison | Participant 5 | 6 |
| Gemini | SequenceComparison | Participant 6 | 4 |
| Gemini | SequenceComparison | Participant 7 | 1 |
| Gemini | StringArrayEncoding | Participant 1 | 4 |
| Gemini | StringArrayEncoding | Participant 2 | 1 |
| Gemini | StringArrayEncoding | Participant 3 | 1 |
| Gemini | StringArrayEncoding | Participant 4 | 2 |
| Gemini | StringArrayEncoding | Participant 5 | 2 |
| Gemini | StringArrayEncoding | Participant 6 | 1 |
| Gemini | StringArrayEncoding | Participant 7 | 1 |
| DeepSeek | ArrayDifference | Participant 1 | 1 |
| DeepSeek | ArrayDifference | Participant 2 | 5 |
| DeepSeek | ArrayDifference | Participant 3 | 1 |
| DeepSeek | ArrayDifference | Participant 4 | 2 |
| DeepSeek | ArrayDifference | Participant 5 | 1 |
| DeepSeek | ArrayDifference | Participant 6 | 1 |
| DeepSeek | ArrayDifference | Participant 7 | 1 |
| DeepSeek | SequenceComparison | Participant 1 | 4 |
| DeepSeek | SequenceComparison | Participant 2 | 1 |
| DeepSeek | SequenceComparison | Participant 3 | 14 |
| DeepSeek | SequenceComparison | Participant 4 | 2 |
| DeepSeek | SequenceComparison | Participant 5 | 1 |
| DeepSeek | SequenceComparison | Participant 6 | 2 |
| DeepSeek | SequenceComparison | Participant 7 | 2 |
| DeepSeek | StringArrayEncoding | Participant 1 | 1 |
| DeepSeek | StringArrayEncoding | Participant 2 | 1 |
| DeepSeek | StringArrayEncoding | Participant 3 | 1 |
| DeepSeek | StringArrayEncoding | Participant 4 | 7 |
| DeepSeek | StringArrayEncoding | Participant 5 | 2 |
| DeepSeek | StringArrayEncoding | Participant 6 | 1 |
| DeepSeek | StringArrayEncoding | Participant 7 | 1 |

**Total geral: 217 execuções.** Por modelo: GPT 76, Claude 45, Gemini 44, DeepSeek 52. Alguns pares questão/participante se destacam por exigir muito mais tentativas que os demais (`GPT/ArrayDifference/Participant 1` (14), `Claude/SequenceComparison/Participant 3` (14) e `DeepSeek/SequenceComparison/Participant 3` (14)) indicando prompts ou respostas problemáticas nesses pontos específicos da coleta.

## Stack técnica

- **.NET 10** / ASP.NET Core Web API
- `Microsoft.Extensions.AI` + `Microsoft.Extensions.AI.OpenAI` — abstração de chat client para GPT e DeepSeek
- `GeminiDotnet.Extensions.AI` — client para Gemini
- `Microsoft.CodeAnalysis.CSharp` (Roslyn) — parsing, formatação e geração de código C# a partir do texto retornado pelos modelos
- Swagger / OpenAPI habilitado em desenvolvimento

## Estrutura do projeto

```
AIConnection/
├── Controllers/
│   └── AIController.cs          # Endpoints por modelo
├── Services/
│   ├── Claude/ClaudeService.cs        # Cliente HTTP dedicado para a API da Anthropic
│   └── ClassGeneration/ClassGenerationService.cs  # Extração/formatação do código gerado
├── Dtos/
│   └── LLM/                     # DTOs de requisição/resposta (inclui DTOs específicos do Claude)
├── Enum/
│   ├── LargeLanguageModelType.cs
│   ├── QuestionType.cs
│   └── SeniorityType.cs
├── GeneratedCode/                # Saída: uma pasta por Modelo/Questão/Participante
└── Program.cs                    # Configuração dos clients de IA e pipeline HTTP
```

## Contexto

Este repositório é a camada de coleta de dados de um TCC que compara a qualidade de código C# gerado por diferentes LLMs, usando métricas como CodeBLEU e análise estática via SonarQube sobre os 84 projetos gerados.
