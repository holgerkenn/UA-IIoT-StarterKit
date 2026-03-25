 using UaPublisher;
using UaPubSubCommon;

static string GetEnv(string name, string defaultValue) => Environment.GetEnvironmentVariable(name) ?? defaultValue;
static int GetEnvInt(string name, int defaultValue) => int.TryParse(Environment.GetEnvironmentVariable(name), out var val) ? val : defaultValue;
static bool GetEnvBool(string name, bool defaultValue) => bool.TryParse(Environment.GetEnvironmentVariable(name), out var val) ? val : defaultValue;

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
    BrokerHost = GetEnv("BROKER_HOST", "iop-gateway-germany.opcfoundation.org"),
    BrokerPort = GetEnvInt("BROKER_PORT", 1883),
    UseTls = GetEnvBool("BROKER_TLS", false),
    UserName = GetEnv("BROKER_USER", "iopuser"),
    Password = GetEnv("BROKER_PASS", "iop-opc"),
    TopicPrefix = GetEnv("TOPIC_PREFIX", "opcua-kit"),
    PublisherId = GetEnv("PUBLISHER_ID", "opcf-iiot-kit-dotnet"),
    UseNewEncodings = GetEnvBool("USE_NEW_ENCODINGS", true),
    EnableCompression = GetEnvBool("ENABLE_COMPRESSION", true)
};

configuration.ApplicationDescription = new()
{
    ApplicationName = "OPC-F IIoT StarterKit Publisher (.NET)",
    ApplicationType = Opc.Ua.ApplicationType.Client,
    ApplicationUri = $"urn:{Configuration.GetPrivateHostName()}.local:{DateTime.Now:yyyy-MM}:{configuration.TopicPrefix}:{configuration.PublisherId}",
    ProductUri = "https://github.com/OPCFoundation/UA-IIoT-StarterKit"
};

Log.System("Use Ctrl-C or Ctrl-Break or exit program.");
await new Publisher(configuration).Connect(token);
