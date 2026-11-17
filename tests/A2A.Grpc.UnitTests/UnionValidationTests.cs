namespace A2A.Grpc.UnitTests;

using System.Text.Json;
using A2A;
using A2A.Grpc;

public sealed class UnionValidationTests
{
    [Fact]
    public void Part_WithConflictingContent_IsRejectedByBothTransports()
    {
        var part = new Part { Text = "see file", Url = "https://example.com/file" };
        Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(part, A2AJsonUtilities.DefaultOptions));
        Assert.Throws<InvalidOperationException>(() => ProtoMap.ToProto(part));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Part>("""{"text":"see file","url":"https://example.com/file"}""", A2AJsonUtilities.DefaultOptions));
    }

    [Fact]
    public void SendMessageResponse_WithConflictingPayload_IsRejectedByBothTransports()
    {
        var response = new SendMessageResponse { Task = new AgentTask(), Message = new Message() };
        Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(response, A2AJsonUtilities.DefaultOptions));
        Assert.Throws<InvalidOperationException>(() => ProtoMap.ToProto(response));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SendMessageResponse>("""{"task":{},"message":{}}""", A2AJsonUtilities.DefaultOptions));
    }

    [Fact]
    public void StreamResponse_WithConflictingPayload_IsRejectedByBothTransports()
    {
        var response = new StreamResponse { StatusUpdate = new TaskStatusUpdateEvent(), ArtifactUpdate = new TaskArtifactUpdateEvent() };
        Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(response, A2AJsonUtilities.DefaultOptions));
        Assert.Throws<InvalidOperationException>(() => ProtoMap.ToProto(response));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StreamResponse>("""{"statusUpdate":{},"artifactUpdate":{}}""", A2AJsonUtilities.DefaultOptions));
    }

    [Theory]
    [InlineData("\"text\":\"x\"", "\"raw\":\"AQ==\"")]
    [InlineData("\"text\":\"x\"", "\"url\":\"https://example.com\"")]
    [InlineData("\"text\":\"x\"", "\"data\":{}")]
    [InlineData("\"raw\":\"AQ==\"", "\"url\":\"https://example.com\"")]
    [InlineData("\"raw\":\"AQ==\"", "\"data\":{}")]
    [InlineData("\"url\":\"https://example.com\"", "\"data\":{}")]
    public void DeserializePart_WithAnyTwoAlternatives_RejectsInput(string first, string second)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Part>($"{{{first},{second}}}", A2AJsonUtilities.DefaultOptions));
    }

    [Theory]
    [InlineData("task", "message")]
    [InlineData("task", "statusUpdate")]
    [InlineData("task", "artifactUpdate")]
    [InlineData("message", "statusUpdate")]
    [InlineData("message", "artifactUpdate")]
    [InlineData("statusUpdate", "artifactUpdate")]
    public void DeserializeStream_WithAnyTwoAlternatives_RejectsInput(string first, string second)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StreamResponse>($"{{\"{first}\":{{}},\"{second}\":{{}}}}", A2AJsonUtilities.DefaultOptions));
    }
}
