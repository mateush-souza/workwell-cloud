namespace WorkWell.Infrastructure.MongoDb;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ChatConversationsCollection { get; set; } = "ChatConversations";
    public string MeditationSessionsCollection { get; set; } = "MeditationSessions";
}

