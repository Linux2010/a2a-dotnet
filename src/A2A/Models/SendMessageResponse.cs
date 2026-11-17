namespace A2A;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Identifies which payload field is set on a SendMessageResponse.</summary>
public enum SendMessageResponseCase
{
    /// <summary>No payload field is set.</summary>
    None,
    /// <summary>Task payload.</summary>
    Task,
    /// <summary>Message payload.</summary>
    Message,
}

/// <summary>Represents the response to a send message request. Uses field-presence to indicate whether the result is a task or a message.</summary>
public sealed class SendMessageResponse : IJsonOnSerializing, IJsonOnDeserialized
{
    /// <summary>Gets or sets the task result.</summary>
    public AgentTask? Task { get; set; }

    /// <summary>Gets or sets the message result.</summary>
    public Message? Message { get; set; }

    /// <summary>Gets which payload field is currently set.</summary>
    /// <exception cref="InvalidOperationException">Multiple alternatives are set.</exception>
    [JsonIgnore]
    public SendMessageResponseCase PayloadCase
    {
        get
        {
            if ((Task is null ? 0 : 1) + (Message is null ? 0 : 1) > 1)
            {
                throw new InvalidOperationException("SendMessageResponse cannot contain multiple payload alternatives.");
            }

            return
                Task is not null ? SendMessageResponseCase.Task :
                Message is not null ? SendMessageResponseCase.Message :
                SendMessageResponseCase.None;
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
