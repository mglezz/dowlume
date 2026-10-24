using System.Text.Json;
using Dowlume.Core;
using Dowlume.Infrastructure;
using Microsoft.Data.Sqlite;

namespace Dowlume.Tests;

public sealed class ProfileCompatibilityTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("es")]
    public void OpensAndPreservesVersion020ProfileAndRestoresClosedDatabaseBackup(string language)
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("report.pdf", "Original 0.2.0 content");
        // Schema and numeric enum values from v0.2.0; do not serialize current model objects as the fixture.
        string settings = $$"""
            {"Language":"{{language}}","DownloadsPath":{{JsonSerializer.Serialize(workspace.Downloads)}},
            "Categories":[
              {"Category":0,"FolderName":"Mis documentos","Extensions":[".pdf"],"ReviewAfterDays":42},
              {"Category":1,"FolderName":"Images","Extensions":[".png"],"ReviewAfterDays":60},
              {"Category":2,"FolderName":"Videos","Extensions":[".mp4"],"ReviewAfterDays":30},
              {"Category":3,"FolderName":"Archives","Extensions":[".zip"],"ReviewAfterDays":14},
              {"Category":4,"FolderName":"Installers","Extensions":[".exe"],"ReviewAfterDays":7},
              {"Category":5,"FolderName":"Other","Extensions":[],"ReviewAfterDays":30}],
            "ExcludedExtensions":[".tmp"]}
            """;
        const string fileId = "b21e2d6d-1146-4efc-93cf-f672850737c2";
        const string operationId = "72212442-167b-43d1-a6ee-f60673037bc9";
        string file = $$"""
            {"Id":"{{fileId}}","OriginalPath":{{JsonSerializer.Serialize(source)}},
            "CurrentPath":{{JsonSerializer.Serialize(source)}},"Category":0,
            "FirstSeenUtc":"2026-10-19T12:00:00+00:00","Status":2,"ContentHash":"retained-hash","SnoozedUntilUtc":null}
            """;
        string operation = $$"""
            {"Id":"{{operationId}}","Kind":2,"Status":1,"TimestampUtc":"2026-10-19T12:00:00+00:00",
            "Before":{{file}},"After":{{file}},"SourcePath":{{JsonSerializer.Serialize(source)}},
            "DestinationPath":null,"Error":null,"UndoesOperationId":null}
            """;
        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = workspace.Database, Pooling = false }.ToString()))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE settings (id INTEGER PRIMARY KEY CHECK(id = 1), payload TEXT NOT NULL);
                CREATE TABLE files (id TEXT PRIMARY KEY, payload TEXT NOT NULL);
                CREATE TABLE operations (id TEXT PRIMARY KEY, payload TEXT NOT NULL);
                PRAGMA user_version=1;
                INSERT INTO settings VALUES(1, $settings);
                INSERT INTO files VALUES($fileId, $file);
                INSERT INTO operations VALUES($operationId, $operation);
                """;
            command.Parameters.AddWithValue("$settings", settings);
            command.Parameters.AddWithValue("$fileId", fileId);
            command.Parameters.AddWithValue("$file", file);
            command.Parameters.AddWithValue("$operationId", operationId);
            command.Parameters.AddWithValue("$operation", operation);
            command.ExecuteNonQuery();
        }
        string backup = Path.Combine(workspace.Root, "closed-profile-backup.db");
        File.Copy(workspace.Database, backup);
        foreach (string database in new[] { workspace.Database, backup })
        {
            var store = new SqliteStateStore(database);
            AppSettings loaded = store.LoadSettings()!;
            Assert.Equal(language, loaded.Language);
            Assert.Equal(workspace.Downloads, loaded.DownloadsPath);
            Assert.Equal("Mis documentos", loaded.Categories[0].FolderName);
            Assert.Equal(42, loaded.Categories[0].ReviewAfterDays);
            Assert.Equal([".tmp"], loaded.ExcludedExtensions);
            store.SaveSettings(loaded);
            var reopened = new SqliteStateStore(database);
            Assert.Equal(language, reopened.LoadSettings()!.Language);
            Assert.Equal(LifecycleStatus.Kept, Assert.Single(reopened.GetFiles()).Status);
            OperationRecord retained = Assert.Single(reopened.GetOperationHistory(new OperationHistoryQuery()).Items);
            Assert.Equal(Guid.Parse(operationId), retained.Id);
            Assert.Equal(OperationKind.Keep, retained.Kind);
            Assert.Equal(OperationStatus.Completed, retained.Status);
            Assert.Equal("retained-hash", retained.After.ContentHash);
            Assert.Equal(source, retained.SourcePath);
        }
        Assert.Equal("Original 0.2.0 content", File.ReadAllText(source));
    }
}
