namespace A2A.UnitTests.Client;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;

public sealed class RestClientErrorDiagnosticsTests
{
    [Theory]
    [InlineData("get")]
    [InlineData("post")]
    [InlineData("post-empty")]
    [InlineData("delete")]
    [InlineData("subscribe")]
    [InlineData("stream")]
    [InlineData("success")]
    public async Task Request_RecordsDiagnosticsAndPreservesOutcome(string operation)
    {
        using var parent = new Activity("rest-diagnostics-test").SetIdFormat(ActivityIdFormat.W3C).Start();
        var traceId = parent.TraceId;
        Activity? completed = null;
        using var activities = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "A2A",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity =>
            {
                if (activity.TraceId == traceId) completed = activity;
            },
        };
        ActivitySource.AddActivityListener(activities);

        long errors = 0;
        long requests = 0;
        using var meters = new MeterListener();
        meters.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == "A2A" &&
                (instrument.Name == "a2a.client.error.count" || instrument.Name == "a2a.client.request.count"))
                listener.EnableMeasurementEvents(instrument);
        };
        meters.SetMeasurementEventCallback<long>((instrument, value, _, _) =>
        {
            if (Activity.Current?.TraceId != traceId) return;
            if (instrument.Name == "a2a.client.error.count") Interlocked.Add(ref errors, value);
            if (instrument.Name == "a2a.client.request.count") Interlocked.Add(ref requests, value);
        });
        meters.Start();

        var handler = new ResponseHandler(operation == "success");
        using var http = new HttpClient(handler);
        using var client = new A2AHttpJsonClient(new Uri("https://agent.example"), http);
        A2AException? exception = null;
        if (operation == "success")
        {
            await InvokeAsync(client, operation);
        }
        else
        {
            exception = await Assert.ThrowsAsync<A2AException>(() => InvokeAsync(client, operation));
            Assert.Equal(A2AErrorCode.TaskNotFound, exception.ErrorCode);
            Assert.Contains("not found", exception.Message);
            Assert.Contains("https://example.org/ext", exception.ActivatedExtensions);
        }

        Assert.Equal(1, requests);
        Assert.Equal(operation == "success" ? 0 : 1, errors);
        Assert.True(handler.ContentDisposed);
        Assert.NotNull(completed);
        Assert.Equal(operation == "success" ? ActivityStatusCode.Unset : ActivityStatusCode.Error, completed.Status);
        if (exception is not null) Assert.Equal(exception.Message, completed.StatusDescription);
    }

    private static async Task InvokeAsync(A2AHttpJsonClient client, string operation)
    {
        var message = new SendMessageRequest
        {
            Message = new Message { Role = Role.User, MessageId = "m", Parts = [Part.FromText("hello")] },
        };
        switch (operation)
        {
            case "get":
            case "success": await client.GetTaskAsync(new GetTaskRequest { Id = "missing" }); break;
            case "post": await client.SendMessageAsync(message); break;
            case "post-empty": await client.CancelTaskAsync(new CancelTaskRequest { Id = "missing" }); break;
            case "delete": await client.DeleteTaskPushNotificationConfigAsync(new DeleteTaskPushNotificationConfigRequest { TaskId = "missing", Id = "cfg" }); break;
            case "subscribe":
                await foreach (var _ in client.SubscribeToTaskAsync(new SubscribeToTaskRequest { Id = "missing" })) { }
                break;
            case "stream":
                await foreach (var _ in client.SendStreamingMessageAsync(message)) { }
                break;
            default: throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    private sealed class ResponseHandler(bool success) : HttpMessageHandler
    {
        public bool ContentDisposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = new TrackedContent(success ? """{"id":"t","contextId":"c","status":{"state":"TASK_STATE_COMPLETED"}}""" : "not found", () => ContentDisposed = true);
            var response = new HttpResponseMessage(success ? HttpStatusCode.OK : HttpStatusCode.NotFound) { Content = content };
            response.Headers.TryAddWithoutValidation("A2A-Extensions", "https://example.org/ext");
            return Task.FromResult(response);
        }
    }

    private sealed class TrackedContent(string value, Action onDispose) : StringContent(value)
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing) onDispose();
            base.Dispose(disposing);
        }
    }
}
