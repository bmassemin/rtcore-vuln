using Microsoft.Extensions.Logging;
using RTCoreVuln;

const string DriverName = "RTCoreVulnDemo";

var logger = CreateLogger();

var driverService = new DriverService(logger);

var driverPath = Path.GetFullPath("RTCore64.sys");
driverService.InstallDriver(driverPath, DriverName);

var rtCore = new RTCore(driverService, DriverName);

Console.WriteLine("\nPress any key to uninstall the driver...\n");
Console.ReadKey();

driverService.UninstallDriver(DriverName);

static ILogger CreateLogger()
{
    var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = null;
    }));
    return factory.CreateLogger("rtcore-vuln");
}