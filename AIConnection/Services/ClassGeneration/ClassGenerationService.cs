using AIConnection.Dtos.LLM;
using AIConnection.Enum;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using System.Text.RegularExpressions;

namespace AIConnection.Services.ClassGeneration
{
    public static class ClassGenerationService
    {
        private const string CODIGO_COMPLETO = "Código completo";

        public static void CreateClassFile(LargeLanguageModelRequest llmRequest, string llmResponse)
        {
            if (llmResponse.Contains(CODIGO_COMPLETO))
            {
                int index = llmResponse.IndexOf(CODIGO_COMPLETO);

                llmResponse = index >= 0 ? llmResponse[(index)..].Trim() : llmResponse;

                var matches = Regex.Matches(llmResponse, @"```csharp([\s\S]*?)```", RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    (string code, string className) result = NormalizeClass(llmRequest, match);

                    CreateFile(llmRequest, result.className, result.code);
                }
            }
            else
            {
                var match = Regex.Match(llmResponse, @"```csharp([\s\S]*?)```", RegexOptions.IgnoreCase);

                (string code, string className) result = NormalizeClass(llmRequest, match);

                CreateFile(llmRequest, result.className, result.code);
            }
        }

        private static (string, string) NormalizeClass(LargeLanguageModelRequest llmRequest, Match match)
        {
            if (!match.Success)
            {
                throw new InvalidOperationException("Bloco csharp não encontrado.");
            }

            string code = match.Groups[1].Value.Trim();
            code = NormalizeIndentation(code);

            string className = string.Empty;

            bool hasClass = Regex.IsMatch(code, @"\bclass\s+[A-Za-z_][A-Za-z0-9_]*\b");

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
                {
                    throw new InvalidOperationException("Classe não encontrada.");
                }

                className = classMatch.Groups[1].Value;
            }

            var usingMatches = Regex.Matches(code, @"^using\s+[A-Za-z0-9_.]+;\s*$", RegexOptions.Multiline);
            string usings = string.Join(Environment.NewLine, usingMatches.Select(m => m.Value.Trim()));

            string namespaceName = $"{llmRequest.LargeLanguageModel}.{llmRequest.QuestionIdentifier}.{llmRequest.Seniority}.{llmRequest.Participant.ToUpper()}";
            bool hasNamespace = Regex.IsMatch(code, @"namespace\s+[A-Za-z_][A-Za-z0-9_.]*");

            if (!hasNamespace)
            {
                code = Regex.Replace(code, @"^using\s+[A-Za-z0-9_.]+;\s*$\r?\n?", "", RegexOptions.Multiline).Trim();
                code =
            $@"{usings}
            namespace {namespaceName}
            {{
            {IndentCode(NormalizeIndentation(code), 1)}
            }}";
            }

            else
            {
                code = IndentCode(NormalizeIndentation(code), 1);
            }

            code = FormatCSharp(code);

            return (code, className);
        }

        private static string IndentCode(string code, int level)
        {
            var indent = new string(' ', level * 4);

            return string.Join(Environment.NewLine,
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
            var root = tree.GetRoot();

            using var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);

            var formattedRoot = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, workspace);

            return formattedRoot.ToFullString();
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

        private static string RetrieveParticipantIdentifier(string participant)
        {
            string paricipantIdentifier = string.Empty;

            switch (participant.ToUpper())
            {
                case "PARTICIPANT_1":
                    paricipantIdentifier = "Participant_1";
                    break;
                case "PARTICIPANT_2":
                    paricipantIdentifier = "Participant_2";
                    break;
                case "PARTICIPANT_3":
                    paricipantIdentifier = "Participant_3";
                    break;
                case "PARTICIPANT_4":
                    paricipantIdentifier = "Participant_4";
                    break;
                case "PARTICIPANT_5":
                    paricipantIdentifier = "Participant_5";
                    break;
                case "PARTICIPANT_6":
                    paricipantIdentifier = "Participant_6";
                    break;
                case "PARTICIPANT_7":
                    paricipantIdentifier = "Participant_7";
                    break;
            }

            return paricipantIdentifier;
        }

        private static void CreateFile(LargeLanguageModelRequest llmRequest, string className, string code)
        {
            string llmIdentifier = RetrieveLlmIdentifier(llmRequest.LargeLanguageModel);
            string questionIdentifier = RetrieveQuestionIdentifier(llmRequest.QuestionIdentifier);
            string participantIdentifier = RetrieveParticipantIdentifier(llmRequest.Participant);

            string folderPath = Path.Combine("..", "..", "GeneratedCodeByAI", "GeneratedCodeByAI", "GeneratedCode", llmIdentifier, questionIdentifier, participantIdentifier);
            string filePath = Path.Combine(folderPath, $"{className}.cs");


            if (File.Exists(folderPath))
            {
                int index = folderPath.IndexOf(".cs");
                folderPath = index >= 0 ? folderPath.Substring(0, index) : folderPath;

                folderPath = $"{folderPath}_.cs";
            }

            File.WriteAllText(filePath, code);
        }
    }
}
