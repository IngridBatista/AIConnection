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

Os 21 prompts abaixo (7 participantes × 3 questões) foram extraídos da [Postman Collection](https://github.com/IngridBatista/AIConnection/blob/main/Dataset_Pesquisa.postman_collection.json) usada para popular a API. Cada prompt foi enviado, sem alterações, aos quatro endpoints (GPT, Claude, Gemini, DeepSeek), devido a isto o mesmo texto gera as 4 soluções comparadas para aquele participante/questão, totalizando as 84 execuções.

### ArrayDifference

**Participant 1 (Sênior)**

> Me dê um método em c# que recebe dois array de inteiros e retorna um array contendo somente os elementos do primeiro parâmetro que não estão no segundo parâmetro.

**Participant 2 (Sênior)**

> Crie uma solução em C# utilizando linq e loop que retorne um array de inteiros que estão presentes A e não aparecem em B.

**Participant 3 (Sênior)**

> o seguinte método em C# public static int[]  Difference(int[] a, int[] b) deve comparar e retornar os  elementos do array a não contidos no array b, a ordem dos elementos deve ser mantida, assuma que os parâmetros nunca serão nulos,  evite comentários desnecessários e inclua uma breve explicação do fluxo ao final

**Participant 4 (Sênior)**

> Imagine that you are a Senior software developer experienced in Microsoft technologies, Angular, Bootstrap, .NET Framework, and .NET Core 8. I am developing using Windows 11, Visual Studio 2022, and Visual Studio Code.
> Always consult the official documentation of the technologies whenever there is a question about how to implement something.
> 
> Objective
> Provide clear and rigorous instructions to guide the work of an assistant with the profile of a Senior developer specialized in Microsoft technologies. The goal is to ensure technically correct, verifiable answers aligned with recommended practices, without speculation or unidentified inference.
> 
> Context
> The assistant must operate as a Senior developer experienced in Microsoft .NET Framework 4.8, .NET Core 8, ASP.NET MVC, Angular 17, Bootstrap, IIS, Azure DevOps pipelines, and Windows 11 or Windows Server 2019 environments, using Visual Studio 2022 and Visual Studio Code. Whenever a technical question arises, research must be conducted exclusively in the official documentation of the mentioned technologies.
> 
> Tone and Style
> Direct, objective, and technical communication, avoiding subjective interpretations, ambiguous language, or improper reconstruction of user-provided content.
> 
> Constraints and Guidelines
> The assistant must never present as fact any information that is generated, inferred, or speculated. If something cannot be verified directly, it must clearly state: I cannot verify this, I do not have access to this information, or My knowledge base does not contain this. Any unverified content must begin with [Inference], [Speculation], or [Unverified].
> It must request clarification whenever information is incomplete, without guessing. If any part of the answer is unverified, the entire answer must be labeled accordingly.
> It must not paraphrase or reinterpret the user’s content without explicit request.
> If using terms such as Prevent, Ensure, Never, Fix, Eliminate, or Guarantee, it must identify the claim unless an explicit source is provided.
> Statements about LLM behavior must include [Inference] or [Unverified].
> If the guideline is violated, it must declare: Correction: I made an unverified statement earlier. It was incorrect and should have been identified.
> It must never replace or alter information provided by the user.
> 
> Encourage Creativity or Precision
> In this technical context, prioritize precision, evidence, and alignment with official documentation.
> 
> Include Examples or Analogies
> Example application: Explain how to configure Windows NTLM authentication in an ASP.NET MVC application on IIS. Include references to official documentation and highlight known limitations.
> 
> Refine for Clarity
> The instructions must remain explicit, complete, and free of ambiguity, ensuring uniformity in interpretation and application.
> 
> Problema 1 -  Problema de codificação de Diferença de Array.
> 
> Escreva o código para o seguinte método em C#:  
> 
> public static int[]  Difference(int[] a, int[] b)
> 
> O método deve retornar um array de inteiros contendo todos e somente os elementos do array a que não aparecem no array b.  
> 
> Você deve assumir que o método sempre será chamado com argumentos não nulos.  
> 
> Com base no seguinte problema resolva da maneira mais simples e coesa em C#. Verifique se sua solução faz sentido e me apresente em seguida.

**Participant 5 (Sênior)**

> Olá, tudo bem bem?
> 
> Você é um desenvolvedor C# especialista, com anos de experiência na área, com foco em boas práticas de código limpo e eficiência. 
> 
> 
> Preciso que você desenvolva um método chamado "Difference"
> 
> Assinatura do metodo: "public static int[]  Difference(int[] a, int[] b)"
> 
> Comportamento esperado: "O método deve retornar um array de inteiros contendo todos e somente os elementos do array a que não aparecem no array b.  
> 
> Você deve assumir que o método sempre será chamado com argumentos não nulos."
> 
> Regras: Utilize nomenclaturas claras e bem definidas. Evite comentários desnecessário e redundantes.

**Participant 6 (Pleno)**

> O método em C# deve retornar um array de inteiros contendo todos e somente os elementos do array a que não aparecem no array b.  
> 
> Você deve assumir que o método sempre será chamado com argumentos não nulos.

**Participant 7 (Júnior)**

> Você é um especialista em C# com amplo conhecimento em algoritmos, arquitetura de software, SOLID e clean code. Com base na sua expertise, resolva o problema a seguir com C#:
> 
> Problema 1 -  Problema de codificação de Diferença de Array.
> 
> Escreva o código para o seguinte método em C#:  
> 
> public static int[]  Difference(int[] a, int[] b)
> 
> O método deve retornar um array de inteiros contendo todos e somente os elementos do array a que não aparecem no array b.  
> 
> Você deve assumir que o método sempre será chamado com argumentos não nulos.  

### SequenceComparison

**Participant 1 (Sênior)**

> Implemente uma classe C# que deve comparar 2 sequências. Os valores devem ser recebidos via console. A sequência "A" deve aceitar somente valores double. E deve parar de receber parâmetro quando o zero for digitado, logo o console deve informa que após digitado o zero e os próximos parâmetros serão para a sequência "B". A sequência B só deve aceitar parâmetros como fração,  da seguinte maneira de entrada numerador/denominador, onde internamente esses parâmetros representam uma classe. Se o valor da fração for menor que zero o valor não deve ser adicionado e deve ser interrompida a leitura de parâmetros. A classe da fração deve ter as propriedades numerador e denominador e os métodos islesser e isGreater para verificar as frações. 
>  Se A ou B ficarem vazias o programa deve ser encerrado e uma mensagem explicando o que houve com base nas regras deve ser mostrada ao usuário.

**Participant 2 (Sênior)**

> Implemente a solução em C# para o Problema de Comparação de Sequências. 
>  1.  Classe Fraction: Crie a classe para representar frações (int Numerator, int Denominator). Deve incluir um método para obter o valor decimal (ToDecimal()) e sobrescrever ToString(). 
>  2.  Sequência A (List<double>): Leia valores double do console. Pare a leitura quando 0 for inserido e o descarte. 
>  3.  Sequência B (List<Fraction>): Leia frações do console. Pare a leitura quando a fração inserida for negativa e a descarte. 
>  4.  Validação: Se A ou B estiverem vazias, lance uma exceção e/ou imprima uma mensagem de erro e finalize. 
>  5.  Saída: Imprima todas as frações em B cujo valor decimal seja maior do que pelo menos metade dos números na sequência A.

**Participant 3 (Sênior)**

> private readonly List<Fraction> B = new List<Fraction>();
> 
>     public void Run()
>     {
>         
>     }
> }
> 
> public class Fraction
> {
>     public int Numerator { get; }
>     public int Denominator { get; }
>     public bool IsLesser(Fraction other);
>     public bool IsGreater(Fraction other);
> }
> 
> O método Run em C# deve usar um while para ler valores do console e adicionar na lista A enquanto a entrada for diferente de 0, depois usar outro while para ler frações e adicionar na lista B enquanto o valor da fração for maior ou igual a 0, no final valide as listas A e B, nenhuma das duas pode ser nula ou vazia, caso alguma esteja nula ou vazia deve exibir messagem de erro apropriada, e se estiver tudo certo deve logar apenas as frações de B que forem maiores que pelo menos metade dos valores de A, mantenha o código simples, evite comentários desnecessários e inclua ao final uma breve explicação do fluxo

**Participant 4 (Sênior)**

> Imagine that you are a Senior software developer experienced in Microsoft technologies, Angular, Bootstrap, .NET Framework, and .NET Core 8. I am developing using Windows 11, Visual Studio 2022, and Visual Studio Code.
> Always consult the official documentation of the technologies whenever there is a question about how to implement something.
> 
> Objective
> Provide clear and rigorous instructions to guide the work of an assistant with the profile of a Senior developer specialized in Microsoft technologies. The goal is to ensure technically correct, verifiable answers aligned with recommended practices, without speculation or unidentified inference.
> 
> Context
> The assistant must operate as a Senior developer experienced in Microsoft .NET Framework 4.8, .NET Core 8, ASP.NET MVC, Angular 17, Bootstrap, IIS, Azure DevOps pipelines, and Windows 11 or Windows Server 2019 environments, using Visual Studio 2022 and Visual Studio Code. Whenever a technical question arises, research must be conducted exclusively in the official documentation of the mentioned technologies.
> 
> Tone and Style
> Direct, objective, and technical communication, avoiding subjective interpretations, ambiguous language, or improper reconstruction of user-provided content.
> 
> Constraints and Guidelines
> The assistant must never present as fact any information that is generated, inferred, or speculated. If something cannot be verified directly, it must clearly state: I cannot verify this, I do not have access to this information, or My knowledge base does not contain this. Any unverified content must begin with [Inference], [Speculation], or [Unverified].
> It must request clarification whenever information is incomplete, without guessing. If any part of the answer is unverified, the entire answer must be labeled accordingly.
> It must not paraphrase or reinterpret the user’s content without explicit request.
> If using terms such as Prevent, Ensure, Never, Fix, Eliminate, or Guarantee, it must identify the claim unless an explicit source is provided.
> Statements about LLM behavior must include [Inference] or [Unverified].
> If the guideline is violated, it must declare: Correction: I made an unverified statement earlier. It was incorrect and should have been identified.
> It must never replace or alter information provided by the user.
> 
> Encourage Creativity or Precision
> In this technical context, prioritize precision, evidence, and alignment with official documentation.
> 
> Include Examples or Analogies
> Example application: Explain how to configure Windows NTLM authentication in an ASP.NET MVC application on IIS. Include references to official documentation and highlight known limitations.
> 
> Refine for Clarity
> The instructions must remain explicit, complete, and free of ambiguity, ensuring uniformity in interpretation and application.
> 
> Problema 2 - Problema de codificação de Comparação de Sequências.
> 
> Implemente a classe CompareSequence que deve funcionar conforme descrito abaixo:
> 
> 1. Ler da entrada padrão (Console) uma sequência A de valores do tipo double, inseridos consecutivamente pelo usuário.
> A leitura deve ser encerrada quando o valor 0 for digitado.
>   
> 2. Ler da entrada padrão uma sequência B de frações, representadas por instâncias de uma classe Fraction que dever ser uma classe personalizada para representar frações (numerador/denominador). A classe Fraction deve possuir métodos como IsLesser(), IsGreater(), Numerator, Denominator, etc.
> 
> A leitura de frações deve parar quando for inserida uma fração menor que 0 (ou seja, cujo valor numérico seja negativo).
> Essa fração negativa não deve ser incluída na sequência.
>   
> 3. Imprimir no console todas as frações da sequência B cujo valor seja maior do que pelo menos metade dos números da sequência A.
> 
> Se A ou B estiverem vazias, a execução deve ser interrompida e o programa deve imprimir uma mensagem de erro apropriada no console.  
> 
> Com base no seguinte problema resolva da maneira mais simples e coesa em C#. Verifique se sua solução faz sentido e me apresente em seguida.

**Participant 5 (Sênior)**

> Olá, tudo bem bem?
> 
> Você é um desenvolvedor C# especialista, com anos de experiência na área, com foco em boas práticas de código limpo e eficiência. 
> 
> 
> Preciso que você desenvolva uma classe chamada "CompareSequence"
> 
> Comportamento esperado: "1. Ler da entrada padrão (Console) uma sequência A de valores do tipo double, inseridos consecutivamente pelo usuário.
> A leitura deve ser encerrada quando o valor 0 for digitado.
>   
> 2. Ler da entrada padrão uma sequência B de frações, representadas por instâncias de uma classe Fraction que dever ser uma classe personalizada para representar frações (numerador/denominador). A classe Fraction deve possuir métodos como IsLesser(), IsGreater(), Numerator, Denominator, etc.
> 
> A leitura de frações deve parar quando for inserida uma fração menor que 0 (ou seja, cujo valor numérico seja negativo).
> Essa fração negativa não deve ser incluída na sequência.
>   
> 3. Imprimir no console todas as frações da sequência B cujo valor seja maior do que pelo menos metade dos números da sequência A.
> 
> Se A ou B estiverem vazias, a execução deve ser interrompida e o programa deve imprimir uma mensagem de erro apropriada no console. "
> 
> Regras: Utilize nomenclaturas claras e bem definidas. Evite comentários desnecessário e redundantes. Separe ao máximo o código em funções pequenas e de responsabilidade única (se preciso crie funções privadas, ou classes extras pra issso).

**Participant 6 (Pleno)**

> Implemente a classe CompareSequence em C# que deve funcionar conforme descrito abaixo:
> 
> 1. Ler da entrada padrão (Console) uma sequência A de valores do tipo double, inseridos consecutivamente pelo usuário.
> A leitura deve ser encerrada quando o valor 0 for digitado.
>   
> 2. Ler da entrada padrão uma sequência B de frações, representadas por instâncias de uma classe Fraction que dever ser uma classe personalizada para representar frações (numerador/denominador). A classe Fraction deve possuir métodos como IsLesser(), IsGreater(), Numerator, Denominator, etc.
> 
> A leitura de frações deve parar quando for inserida uma fração menor que 0 (ou seja, cujo valor numérico seja negativo).
> Essa fração negativa não deve ser incluída na sequência.
>   
> 3. Imprimir no console todas as frações da sequência B cujo valor seja maior do que pelo menos metade dos números da sequência A.
> 
> Se A ou B estiverem vazias, a execução deve ser interrompida e o programa deve imprimir uma mensagem de erro apropriada no console.

**Participant 7 (Júnior)**

> Você é um especialista em C# com amplo conhecimento em algoritmos, arquitetura de software, SOLID e clean code. Com base na sua expertise, resolva o problema a seguir com C#: 
> 
> Problema 2 - Problema de codificação de Comparação de Sequências.
> 
> Implemente a classe CompareSequence que deve funcionar conforme descrito abaixo:
> 
> 1. Ler da entrada padrão (Console) uma sequência A de valores do tipo double, inseridos consecutivamente pelo usuário.
> A leitura deve ser encerrada quando o valor 0 for digitado.
>   
> 2. Ler da entrada padrão uma sequência B de frações, representadas por instâncias de uma classe Fraction que dever ser uma classe personalizada para representar frações (numerador/denominador). A classe Fraction deve possuir métodos como IsLesser(), IsGreater(), Numerator, Denominator, etc.
> 
> A leitura de frações deve parar quando for inserida uma fração menor que 0 (ou seja, cujo valor numérico seja negativo).
> Essa fração negativa não deve ser incluída na sequência.
>   
> 3. Imprimir no console todas as frações da sequência B cujo valor seja maior do que pelo menos metade dos números da sequência A.
> 
> Se A ou B estiverem vazias, a execução deve ser interrompida e o programa deve imprimir uma mensagem de erro apropriada no console.

### StringArrayEncoding

**Participant 1 (Sênior)**

> Quero que você implemente uma classe em C# chamada MatrixString, seguindo exatamente todas as regras abaixo, mas explicando o raciocínio de forma clara e humana. O objetivo é criar uma classe que manipule uma matriz de strings com tratamento correto de erros. 
>  Implemente primeiro o construtor public MatrixString(int rows, int columns, string value). Esse construtor deve criar a matriz privada m com tamanho rows × columns e preencher todas as posições com o valor informado em value. Caso rows ou columns sejam menores ou iguais a zero, lance uma ArgumentException usando o construtor padrão. 
>  Depois implemente o método public void Set(int row, int column, string value). Esse método deve atribuir o valor à posição indicada na matriz. Se row ou column estiver fora dos limites da matriz, lance uma exceção personalizada MatrixException usando o construtor padrão. Essa exceção também deve ser implementada. Em seguida implemente o método public string RowToString(int index, string separator). Esse método deve retornar uma string formada pela junção dos elementos da linha de índice index, separados pela string separator. Caso o index não seja válido ou caso separator seja null, lance MatrixException. Ao final, quero que você me entregue: (1) a classe MatrixString completa, (2) a classe MatrixException.

**Participant 2 (Sênior)**

> Implemente em C# a classe MatrixString com a matriz privada string[,] m e a exceção MatrixException. 
>  ​Construtor MatrixString(int rows, int columns, string value): Inicializa m com rows x columns preenchida com value. Lança ArgumentException se rows ou columns não forem positivos. 
>  Método Set(int row, int column, string value): Atribui valor. Lança MatrixException se índices estiverem fora dos limites. 
>  ​Método RowToString(int index, string separator): Concatena e retorna a linha index separada. Lança MatrixException se index for inválido ou separator for null.

**Participant 3 (Sênior)**

> public class MatrixString
> {
>     private string[,] m;
> }
> 
> O construtor public MatrixString(int rows, int cols, string val) deve criar a matrix m com o tamanho informado e preencher tudo com val. 
> Deve lançar ArgumentException se rows ou cols forem inválidos, como negativos ou iguais a zero.
> 
> Implemente o método public void Set(int r, int c, string val) que deve atribuir o valor na posição indicada e lançar MatrixException se r ou c forem inválidos ou fora do limite da matrix.
> 
> Implemente o método public string RowToString(int index, string separator) em C# que deve retornar a linha index concatenada usando separator e lançar MatrixException se idx for inválido ou se sep for null.
> 
> Evite comentários desnecessários, repetição, mantenha nomes concisos e inclua no final uma breve explicação do fluxo

**Participant 4 (Sênior)**

> Imagine that you are a Senior software developer experienced in Microsoft technologies, Angular, Bootstrap, .NET Framework, and .NET Core 8. I am developing using Windows 11, Visual Studio 2022, and Visual Studio Code.
> Always consult the official documentation of the technologies whenever there is a question about how to implement something.
> 
> Objective
> Provide clear and rigorous instructions to guide the work of an assistant with the profile of a Senior developer specialized in Microsoft technologies. The goal is to ensure technically correct, verifiable answers aligned with recommended practices, without speculation or unidentified inference.
> 
> Context
> The assistant must operate as a Senior developer experienced in Microsoft .NET Framework 4.8, .NET Core 8, ASP.NET MVC, Angular 17, Bootstrap, IIS, Azure DevOps pipelines, and Windows 11 or Windows Server 2019 environments, using Visual Studio 2022 and Visual Studio Code. Whenever a technical question arises, research must be conducted exclusively in the official documentation of the mentioned technologies.
> 
> Tone and Style
> Direct, objective, and technical communication, avoiding subjective interpretations, ambiguous language, or improper reconstruction of user-provided content.
> 
> Constraints and Guidelines
> The assistant must never present as fact any information that is generated, inferred, or speculated. If something cannot be verified directly, it must clearly state: I cannot verify this, I do not have access to this information, or My knowledge base does not contain this. Any unverified content must begin with [Inference], [Speculation], or [Unverified].
> It must request clarification whenever information is incomplete, without guessing. If any part of the answer is unverified, the entire answer must be labeled accordingly.
> It must not paraphrase or reinterpret the user’s content without explicit request.
> If using terms such as Prevent, Ensure, Never, Fix, Eliminate, or Guarantee, it must identify the claim unless an explicit source is provided.
> Statements about LLM behavior must include [Inference] or [Unverified].
> If the guideline is violated, it must declare: Correction: I made an unverified statement earlier. It was incorrect and should have been identified.
> It must never replace or alter information provided by the user.
> 
> Encourage Creativity or Precision
> In this technical context, prioritize precision, evidence, and alignment with official documentation.
> 
> Include Examples or Analogies
> Example application: Explain how to configure Windows NTLM authentication in an ASP.NET MVC application on IIS. Include references to official documentation and highlight known limitations.
> 
> Refine for Clarity
> The instructions must remain explicit, complete, and free of ambiguity, ensuring uniformity in interpretation and application.
> 
> Problema 3 - Problema de codificação de Matriz String.
> 
> Considere a classe MaxString com a seguinte estrutura:
> 
> public class MatrixString 
> { 
>     private string[ , ]m;
>     ...
> }
> 
> 1.  Construtor  
> Implemente o construtor da classe com a seguinte assinatura:
> 
> public MatrixString(int rows, int columns, string value)
> 
> O objetivo do construtor é inicializar o campo m com uma matriz de rows linhas e columns colunas, em que cada posição contenha o valor value. 
> 
> O construtor deve lançar uma ArgumentException caso rows ou columns não sejam valores válidos, ou seja, se os valores não forem números naturais positivos maiores que zero (use o construtor padrão de ArgumentException).
> 
> 2.  Método set  
> Implemente o método abaixo:
> 
>  public void Set(int row, int column, string value)
> 
>  Esse método deve atribuir à posição da matriz m na linha row e coluna column o valor value.  
> 
> O método deve lançar uma exceção do tipo MatrixException (exceção personalizada) caso os índices row ou column estejam fora dos limites da matriz. Utilize o construtor padrão de MatrixException.  
> 
> 3. Método RowToString  
> Implemente o método com a assinatura:
>  
> public string RowToString(int index, string separator)
> 
> O método deve retornar uma string formada pela concatenação dos elementos presentes na linha de índice index da matriz m, separados pela string indicada em separator.  
> 
> O método deve lançar MatrixException caso index não seja uma linha válida da matriz ou caso separator seja null. 
> 
> Com base no seguinte problema resolva da maneira mais simples e coesa em C#. Verifique se sua solução faz sentido e me apresente em seguida.

**Participant 5 (Sênior)**

> Olá, tudo bem bem?
> 
> Você é um desenvolvedor C# especialista, com anos de experiência na área, com foco em boas práticas de código limpo e eficiência. 
> 
> Preciso que você desenvolva uma classe chamada "MaxString"
> 
> Comportamento esperado: "Considere a classe MaxString com a seguinte estrutura:
> 
> public class MatrixString 
> { 
>     private string[ , ]m;
>     ...
> }
> 
> 1.  Construtor  
> Implemente o construtor da classe com a seguinte assinatura:
> 
> public MatrixString(int rows, int columns, string value)
> 
> O objetivo do construtor é inicializar o campo m com uma matriz de rows linhas e columns colunas, em que cada posição contenha o valor value. 
> 
> O construtor deve lançar uma ArgumentException caso rows ou columns não sejam valores válidos, ou seja, se os valores não forem números naturais positivos maiores que zero (use o construtor padrão de ArgumentException).
> 
> 2.  Método set  
> Implemente o método abaixo:
> 
>  public void Set(int row, int column, string value)
> 
>  Esse método deve atribuir à posição da matriz m na linha row e coluna column o valor value.  
> 
> O método deve lançar uma exceção do tipo MatrixException (exceção personalizada) caso os índices row ou column estejam fora dos limites da matriz. Utilize o construtor padrão de MatrixException.  
> 
> 3. Método RowToString  
> Implemente o método com a assinatura:
>  
> public string RowToString(int index, string separator)
> 
> O método deve retornar uma string formada pela concatenação dos elementos presentes na linha de índice index da matriz m, separados pela string indicada em separator.  
> 
> O método deve lançar MatrixException caso index não seja uma linha válida da matriz ou caso separator seja null."
> 
> Regras: Utilize nomenclaturas claras e bem definidas. Evite comentários desnecessário e redundantes. Separe ao máximo o código em funções pequenas e de responsabilidade única (se preciso crie funções privadas, ou classes extras pra issso).

**Participant 6 (Pleno)**

> Considere a classe MaxString com a seguinte estrutura:
> 
> public class MatrixString 
> { 
>     private string[ , ]m;
>     ...
> }
> 
> 1.  Construtor  
> Implemente o construtor em C# da classe com a seguinte assinatura:
> 
> public MatrixString(int rows, int columns, string value)
> 
> O objetivo do construtor é inicializar o campo m com uma matriz de rows linhas e columns colunas, em que cada posição contenha o valor value. 
> 
> O construtor deve lançar uma ArgumentException caso rows ou columns não sejam valores válidos, ou seja, se os valores não forem números naturais positivos maiores que zero (use o construtor padrão de ArgumentException).
> 
> 2.  Método set  
> Implemente o método abaixo:
> 
>  public void Set(int row, int column, string value)
> 
>  Esse método deve atribuir à posição da matriz m na linha row e coluna column o valor value.  
> 
> O método deve lançar uma exceção do tipo MatrixException (exceção personalizada) caso os índices row ou column estejam fora dos limites da matriz. Utilize o construtor padrão de MatrixException.  
> 
> 3. Método RowToString  
> Implemente o método com a assinatura:
>  
> public string RowToString(int index, string separator)
> 
> O método deve retornar uma string formada pela concatenação dos elementos presentes na linha de índice index da matriz m, separados pela string indicada em separator.  
> 
> O método deve lançar MatrixException caso index não seja uma linha válida da matriz ou caso separator seja null. 

**Participant 7 (Júnior)**

> Você é um especialista em C# com amplo conhecimento em algoritmos, arquitetura de software, SOLID e clean code. Com base na sua expertise, resolva o problema a seguir com C#:
> 
> Problema 3 - Problema de codificação de Matriz String.
> 
> Considere a classe MaxString com a seguinte estrutura:
> 
> public class MatrixString 
> { 
>     private string[ , ]m;
>     ...
> }
> 
> 1.  Construtor  
> Implemente o construtor da classe com a seguinte assinatura:
> 
> public MatrixString(int rows, int columns, string value)
> 
> O objetivo do construtor é inicializar o campo m com uma matriz de rows linhas e columns colunas, em que cada posição contenha o valor value. 
> 
> O construtor deve lançar uma ArgumentException caso rows ou columns não sejam valores válidos, ou seja, se os valores não forem números naturais positivos maiores que zero (use o construtor padrão de ArgumentException).
> 
> 2.  Método set  
> Implemente o método abaixo:
> 
>  public void Set(int row, int column, string value)
> 
>  Esse método deve atribuir à posição da matriz m na linha row e coluna column o valor value.  
> 
> O método deve lançar uma exceção do tipo MatrixException (exceção personalizada) caso os índices row ou column estejam fora dos limites da matriz. Utilize o construtor padrão de MatrixException.  
> 
> 3. Método RowToString  
> Implemente o método com a assinatura:
>  
> public string RowToString(int index, string separator)
> 
> O método deve retornar uma string formada pela concatenação dos elementos presentes na linha de índice index da matriz m, separados pela string indicada em separator.  
> 
> O método deve lançar MatrixException caso index não seja uma linha válida da matriz ou caso separator seja null.

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
