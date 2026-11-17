namespace A2A;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Identifies which payload field is set on a StreamResponse.</summary>
public enum StreamResponseCase
{
    /// <summary>No payload field is set.</summary>
    None,
    /// <summary>Task payload.</summary>
    Task,
    /// <summary>Message payload.</summary>
    Message,
    /// <summary>Task status update event.</summary>
    StatusUpdate,
    /// <summary>Task artifact update event.</summary>
    ArtifactUpdate,
}

/// <summary>Represents a streaming response event. Uses field-presence to indicate the event type.</summary>
public sealed class StreamResponse : IJsonOnSerializing, IJsonOnDeserialized
{
    /// <summary>Gets or sets the task result.</summary>
    public AgentTask? Task { get; set; }

    /// <summary>Gets or sets the message result.</summary>
    public Message? Message { get; set; }

    /// <summary>Gets or sets the task status update event.</summary>
    public TaskStatusUpdateEvent? StatusUpdate { get; set; }

    /// <summary>Gets or sets the task artifact update event.</summary>
    public TaskArtifactUpdateEvent? ArtifactUpdate { get; set; }

    /// <summary>Gets which payload field is currently set.</summary>
    /// <exception cref="InvalidOperationException">Multiple alternatives are set.</exception>
    [JsonIgnore]
    public StreamResponseCase PayloadCase
    {
        get
        {
            if ((Task is null ? 0 : 1) + (Message is null ? 0 : 1) + (StatusUpdate is null ? 0 : 1) + (ArtifactUpdate is null ? 0 : 1) > 1)
            {
                throw new InvalidOperationException("StreamResponse cannot contain multiple payload alternatives.");
            }

            return
                Task is not null ? StreamResponseCase.Task :
                Message is not null ? StreamResponseCase.Message :
                StatusUpdate is not null ? StreamResponseCase.StatusUpdate :
                ArtifactUpdate is not null ? StreamResponseCase.ArtifactUpdate :
                StreamResponseCase.None;
        }
    }

    void IJsonOnSerializing.OnSerializing() => _ = PayloadCase;

    void IJsonOnDeserialized.OnDeserialized()
    {
        try
        {
            _ = PayloadCase;
        }
        catch (InvalidOperationException ex)
        {
            throw new JsonException(ex.Message, ex);
        }
    }
}
