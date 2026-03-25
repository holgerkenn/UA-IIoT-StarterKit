using UaPubSubCommon;
using UaSubscriber;

static string GetEnv(string name, string defaultValue) => Environment.GetEnvironmentVariable(name) ?? defaultValue;
static int GetEnvInt(string name, int defaultValue) => int.TryParse(Environment.GetEnvironmentVariable(name), out var val) ? val : defaultValue;
static bool GetEnvBool(string name, bool defaultValue) => bool.TryParse(Environment.GetEnvironmentVariable(name), out var val) ? val : defaultValue;

try
{
    var cts = new CancellationTokenSource();
    CancellationToken token = cts.Token;

    // raised when Ctrl-C or Ctrl-Break is pressed.
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true; // Prevents immediate termination
        cts.Cancel();    // Signals cancellation
    };

    var configuration = new Configuration()
    {
        BrokerHost = GetEnv("BROKER_HOST", "mqtt-broker-local"),
        BrokerPort = GetEnvInt("BROKER_PORT", 1883),
        UseTls = GetEnvBool("BROKER_TLS", false),
        UserName = GetEnv("BROKER_USER", "iopuser"),
        Password = GetEnv("BROKER_PASS", "iop-opc"),
        TopicPrefix = GetEnv("TOPIC_PREFIX", "opcua-kit"),
        PublisherId = GetEnv("PUBLISHER_ID", "opcf-iiot-kit-requestor"),
        UseNewEncodings = GetEnvBool("USE_NEW_ENCODINGS", true)
    };

    Log.System("Use Ctrl-C or Ctrl-Break or exit program.");

    bool isResponder = false;
    string targetPublisherId = "";

    for (int ii = 0; ii < args.Length; ii++)
    {
        switch (args[ii])
        {
            case "--responder": { isResponder = true; break; }
            case "--target": { targetPublisherId = (ii + 1 < args.Length) ? args[ii + 1] : null; break; }
        }
    }

    await new Subscriber(
        configuration, 
        isResponder, 
        targetPublisherId
    ).Connect(token);
}
catch (AggregateException e)
{
    Log.Error($"[{e.GetType().Name}] {e.Message}");

    foreach (var ie in e.InnerExceptions)
    {
        Log.Warning($">>> [{ie.GetType().Name}] {ie.Message}");
    }

    Environment.Exit(3);
}
catch (Exception e)
{
    Log.Error($"[{e.GetType().Name}] {e.Message}");

    Exception ie = e.InnerException;

    while (ie != null)
    {
        Log.Warning($">>> [{ie.GetType().Name}] {ie.Message}");
        ie = ie.InnerException;
    }

    Log.System($"========================");
    Log.Info($"{e.StackTrace}");
    Log.System($"========================");

    Environment.Exit(3);
}