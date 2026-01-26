using AIConnection.Enum;

namespace AIConnection.Dtos.LLM
{
    public class LargeLanguageModelRequest
    {
        public LargeLanguageModelType LargeLanguageModel { get; set; }
        public QuestionType QuestionIdentifier { get; set; }
        public SeniorityType Seniority { get; set; }
        public required string Participant { get; set; }
        public required string Prompt { get; set; }
    }
}
