namespace A2A;

/// <summary>
/// Centralized mappings from <see cref="A2AErrorCode"/> to HTTP status codes,
/// gRPC status strings, and UPPER_SNAKE_CASE reason strings per A2A spec Section 5.4.
/// </summary>
internal static class A2AErrorCodeMapping
{
    private static readonly Dictionary<string, A2AErrorCode> s_reasonToErrorCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TASK_NOT_FOUND"] = A2AErrorCode.TaskNotFound,
        ["TASK_NOT_CANCELABLE"] = A2AErrorCode.TaskNotCancelable,
        ["PUSH_NOTIFICATION_NOT_SUPPORTED"] = A2AErrorCode.PushNotificationNotSupported,
        ["UNSUPPORTED_OPERATION"] = A2AErrorCode.UnsupportedOperation,
        ["CONTENT_TYPE_NOT_SUPPORTED"] = A2AErrorCode.ContentTypeNotSupported,
        ["INVALID_AGENT_RESPONSE"] = A2AErrorCode.InvalidAgentResponse,
        ["EXTENDED_AGENT_CARD_NOT_CONFIGURED"] = A2AErrorCode.ExtendedAgentCardNotConfigured,
        ["EXTENSION_SUPPORT_REQUIRED"] = A2AErrorCode.ExtensionSupportRequired,
        ["VERSION_NOT_SUPPORTED"] = A2AErrorCode.VersionNotSupported,
        ["INVALID_REQUEST"] = A2AErrorCode.InvalidRequest,
        ["INVALID_PARAMS"] = A2AErrorCode.InvalidParams,
        ["PARSE_ERROR"] = A2AErrorCode.ParseError,
        ["INTERNAL_ERROR"] = A2AErrorCode.InternalError,
        ["METHOD_NOT_FOUND"] = A2AErrorCode.MethodNotFound,
    };

    /// <summary>Looks up A2A and standard JSON-RPC error codes by their ErrorInfo reason.</summary>
    /// <param name="reason">The reason to look up.</param>
    /// <param name="code">Receives the corresponding error code when found.</param>
    public static bool TryGetErrorCode(string reason, out A2AErrorCode code) =>
        s_reasonToErrorCode.TryGetValue(reason, out code);

    /// <summary>Returns the HTTP status code for the given error code per spec Section 5.4.</summary>
    /// <param name="code">The A2A error code to map.</param>
    public static int GetHttpStatusCode(A2AErrorCode code) => code switch
    {
        A2AErrorCode.TaskNotFound or
        A2AErrorCode.MethodNotFound => 404,

        A2AErrorCode.TaskNotCancelable or
        A2AErrorCode.PushNotificationNotSupported or
        A2AErrorCode.UnsupportedOperation or
        A2AErrorCode.ContentTypeNotSupported or
        A2AErrorCode.ExtendedAgentCardNotConfigured or
        A2AErrorCode.ExtensionSupportRequired or
        A2AErrorCode.VersionNotSupported or
        A2AErrorCode.InvalidRequest or
        A2AErrorCode.InvalidParams or
        A2AErrorCode.ParseError => 400,

        A2AErrorCode.InvalidAgentResponse or
        A2AErrorCode.InternalError => 500,

        _ => 500,
    };

    /// <summary>Returns the gRPC status string for the given error code per spec Section 5.4.</summary>
    /// <param name="code">The A2A error code to map.</param>
    public static string GetGrpcStatus(A2AErrorCode code) => code switch
    {
        A2AErrorCode.TaskNotFound or
        A2AErrorCode.MethodNotFound => "NOT_FOUND",

        A2AErrorCode.TaskNotCancelable or
        A2AErrorCode.PushNotificationNotSupported or
        A2AErrorCode.UnsupportedOperation or
        A2AErrorCode.ExtendedAgentCardNotConfigured or
        A2AErrorCode.ExtensionSupportRequired or
        A2AErrorCode.VersionNotSupported => "FAILED_PRECONDITION",

        A2AErrorCode.ContentTypeNotSupported or
        A2AErrorCode.InvalidRequest or
        A2AErrorCode.InvalidParams or
        A2AErrorCode.ParseError => "INVALID_ARGUMENT",

        A2AErrorCode.InvalidAgentResponse or
        A2AErrorCode.InternalError => "INTERNAL",

        _ => "INTERNAL",
    };

    /// <summary>
    /// Returns the UPPER_SNAKE_CASE reason string for A2A-specific errors,
    /// or <c>null</c> for standard JSON-RPC errors.
    /// </summary>
    /// <param name="code">The A2A error code to map.</param>
    public static string? GetReasonString(A2AErrorCode code) => code switch
    {
        A2AErrorCode.TaskNotFound => "TASK_NOT_FOUND",
        A2AErrorCode.TaskNotCancelable => "TASK_NOT_CANCELABLE",
        A2AErrorCode.PushNotificationNotSupported => "PUSH_NOTIFICATION_NOT_SUPPORTED",
        A2AErrorCode.UnsupportedOperation => "UNSUPPORTED_OPERATION",
        A2AErrorCode.ContentTypeNotSupported => "CONTENT_TYPE_NOT_SUPPORTED",
        A2AErrorCode.InvalidAgentResponse => "INVALID_AGENT_RESPONSE",
        A2AErrorCode.ExtendedAgentCardNotConfigured => "EXTENDED_AGENT_CARD_NOT_CONFIGURED",
        A2AErrorCode.ExtensionSupportRequired => "EXTENSION_SUPPORT_REQUIRED",
        A2AErrorCode.VersionNotSupported => "VERSION_NOT_SUPPORTED",
        _ => null,
    };

    /// <summary>Returns <c>true</c> for A2A-specific error codes (-32001 to -32099).</summary>
    /// <param name="code">The A2A error code to check.</param>
    public static bool IsA2ASpecificError(A2AErrorCode code) =>
        (int)code is >= -32099 and <= -32001;
}
