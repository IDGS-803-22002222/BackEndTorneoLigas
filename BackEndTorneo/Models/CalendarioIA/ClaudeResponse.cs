namespace BackEndTorneo.Models.CalendarioIA
{
    public class ClaudeResponse
    {
        public string Id { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Role { get; set; } = null!;
        public List<ClaudeContent> Content { get; set; } = new();
        public string Model { get; set; } = null!;
        public ClaudeUsage Usage { get; set; } = null!;
    }

    public class ClaudeContent
    {
        public string Type { get; set; } = null!;
        public string Text { get; set; } = null!;
    }

    public class ClaudeUsage
    {
        public int Input_Tokens { get; set; }
        public int Output_Tokens { get; set; }
    }
}