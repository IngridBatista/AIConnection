using AIConnection.Dtos.LLM;
using AIConnection.Enum;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;

namespace AIConnection.Services
{
    public static class ClassGenerationService
    {
        public static void CreateClassFile(LargeLanguageModelRequest llmRequest, string llmResponse)
        {
            var match = Regex.Match(llmResponse, @"```csharp([\s\S]*?)```", RegexOptions.IgnoreCase);

            if (!match.Success)
                throw new InvalidOperationException("Bloco csharp não encontrado.");

            string code = match.Groups[1].Value.Trim();
            code = NormalizeIndentation(code);

            bool hasClass = Regex.IsMatch(code, @"\bclass\s+[A-Za-z_][A-Za-z0-9_]*\b");

            string className = string.Empty;

            if (!hasClass)
            {
                string fallbackClassName = $"{llmRequest.LargeLanguageModel}_{llmRequest.QuestionIdentifier}_{llmRequest.Seniority}_{llmRequest.Participant.ToUpper()}";
                className = fallbackClassName;

                code =
                $@"public class {fallbackClassName}
                {{
                {IndentCode(code, 1)}
                }}";
            }
            else
            {
                var classMatch = Regex.Match(code, @"class\s+([A-Za-z_][A-Za-z0-9_]*)");
                if (!classMatch.Success)
                    throw new InvalidOperationException("Classe não encontrada.");

                className = classMatch.Groups[1].Value;
            }

            var usingMatches = Regex.Matches(code, @"^using\s+[A-Za-z0-9_.]+;\s*$", RegexOptions.Multiline);
            string usings = string.Join(Environment.NewLine, usingMatches.Select(m => m.Value.Trim()));
            code = Regex.Replace(code, @"^using\s+[A-Za-z0-9_.]+;\s*$\r?\n?", "", RegexOptions.Multiline).Trim();

            string namespaceName = $"{llmRequest.LargeLanguageModel}.{llmRequest.QuestionIdentifier}.{llmRequest.Seniority}.{llmRequest.Participant.ToUpper()}";
            bool hasNamespace = Regex.IsMatch(code, @"namespace\s+[A-Za-z_][A-Za-z0-9_.]*");

            if (!hasNamespace)
            {
                code =
            $@"{usings}
            namespace {namespaceName}
            {{
            {IndentCode(NormalizeIndentation(code), 1)}
            }}";
            }

            code = FormatCSharp(code);

            string questionIdentifier = RetrieveQuestionIdentifier(llmRequest.QuestionIdentifier);
            string llmIdentifier = RetrieveLlmIdentifier(llmRequest.LargeLanguageModel);
            string fileName = $"GeneratedCode/{llmIdentifier}/{questionIdentifier}/{className}.cs";

            File.WriteAllText(fileName, code);
        }

        private static string IndentCode(string code, int level)
        {
            var indent = new string(' ', level * 4);

            return string.Join(
                Environment.NewLine,
                code.Split(Environment.NewLine)
                    .Select(line =>
                        string.IsNullOrWhiteSpace(line)
                            ? line
                            : indent + line
                    )
            );
        }

        private static string NormalizeIndentation(string code)
        {
            var lines = code
                .Split(Environment.NewLine)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (!lines.Any())
                return code;

            int minIndent = lines
                .Select(l => l.TakeWhile(char.IsWhiteSpace).Count())
                .Min();

            return string.Join(
                Environment.NewLine,
                code.Split(Environment.NewLine)
                    .Select(l =>
                        l.Length >= minIndent
                            ? l[minIndent..]
                            : l
                    )
            );
        }

        private static string FormatCSharp(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var root = tree.GetRoot()
                .NormalizeWhitespace(
                    indentation: "    ",
                    eol: Environment.NewLine);

            return root.ToFullString();
        }

        private static string RetrieveQuestionIdentifier(QuestionType questionType)
        {
            string questionIdentifier = string.Empty;

            switch (questionType)
            {
                case QuestionType.ARRAY_DIFFERENCE:
                    questionIdentifier = "ArrayDifference";
                    break;
                case QuestionType.SEQUENCE_COMPARISON:
                    questionIdentifier = "SequenceComparison";
                    break;
                case QuestionType.STRING_ARRAY_ENCODING:
                    questionIdentifier = "StringArrayEncoding";
                    break;
            }

            return questionIdentifier;
        }

        private static string RetrieveLlmIdentifier(LargeLanguageModelType llmType)
        {
            string llmIdentifier = string.Empty;

            switch (llmType)
            {
                case LargeLanguageModelType.GTP:
                    llmIdentifier = "Gpt";
                    break;
                case LargeLanguageModelType.CLAUDE:
                    llmIdentifier = "Claude";
                    break;
                case LargeLanguageModelType.GEMINI:
                    llmIdentifier = "Gemini";
                    break;
                case LargeLanguageModelType.DEEPSEEK:
                    llmIdentifier = "DeepSeek";
                    break;
            }

            return llmIdentifier;
        }
    }
}
