using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Windows.Win32;
using Windows.Win32.System.Services;
using static Windows.Win32.PInvoke;

namespace RTCoreVuln
{
    /// <summary>
    /// Manages the lifecycle of a kernel driver through the Windows Service Control Manager (SCM).
    /// </summary>
    internal class DriverService(ILogger logger)
    {
        /// <summary>
        /// Registers and starts a kernel driver as a demand-start service.
        /// Removes any leftover service with the same name before creating a new one.
        /// </summary>
        public CloseServiceHandleSafeHandle InstallDriver(string path, string serviceName)
        {
            logger.LogInformation("Installing driver service '{ServiceName}' from path: {Path}", serviceName, path);

            if (!File.Exists(path))
                throw new FileNotFoundException("Driver file not found.", path);

            using var scmHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scmHandle.IsInvalid)
                throw new Win32Exception("Failed to open service control manager.");

            // Clean up any leftover service from a previous run
            StopAndDeleteService(scmHandle, serviceName);

            using var serviceHandle = CreateService(
                scmHandle,
                serviceName,
                "RTCore Vulnerable Driver",
                SERVICE_ALL_ACCESS,
                ENUM_SERVICE_TYPE.SERVICE_KERNEL_DRIVER,
                SERVICE_START_TYPE.SERVICE_DEMAND_START,
                SERVICE_ERROR.SERVICE_ERROR_NORMAL,
                path
            );
            if (serviceHandle.IsInvalid)
                throw new Win32Exception("Failed to create service.");

            var startResult = StartService(serviceHandle);
            if (!startResult)
                throw new Win32Exception("Failed to start service.");

            logger.LogInformation("Driver service '{ServiceName}' installed and started successfully.", serviceName);

            var openedServiceHandle = OpenService(scmHandle, serviceName, SERVICE_ALL_ACCESS);
            if (openedServiceHandle.IsInvalid)
                throw new Win32Exception("Failed to open service after starting.");

            return openedServiceHandle;
        }

        /// <summary>
        /// Stops a running kernel driver and removes it from the SCM.
        /// </summary>
        public void UninstallDriver(string serviceName)
        {
            logger.LogInformation("Uninstalling driver service '{ServiceName}'", serviceName);

            using var scmHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scmHandle.IsInvalid)
                throw new Win32Exception("Failed to open service control manager.");

            if (!StopAndDeleteService(scmHandle, serviceName))
                throw new Win32Exception("Failed to uninstall service.");

            logger.LogInformation("Driver service '{ServiceName}' uninstalled successfully.", serviceName);
        }

        /// <summary>
        /// Stops and deletes an existing service. Returns false if the service doesn't exist.
        /// Waits up to 10 seconds for the driver to fully stop before deleting.
        /// </summary>
        private static bool StopAndDeleteService(CloseServiceHandleSafeHandle scmHandle, string serviceName)
        {
            using var serviceHandle = OpenService(scmHandle, serviceName, SERVICE_ALL_ACCESS);
            if (serviceHandle.IsInvalid)
                return false;

            if (!QueryServiceStatus(serviceHandle, out var status))
                throw new Win32Exception("Failed to query service status.");

            if (status.dwCurrentState != SERVICE_STATUS_CURRENT_STATE.SERVICE_STOPPED)
            {
                if (!ControlService(serviceHandle, SERVICE_CONTROL_STOP, out status))
                    throw new Win32Exception("Failed to stop service.");

                int retries = 10;
                while (retries-- > 0)
                {
                    if (!QueryServiceStatus(serviceHandle, out status))
                        throw new Win32Exception("Failed to query service status.");
                    if (status.dwCurrentState == SERVICE_STATUS_CURRENT_STATE.SERVICE_STOPPED)
                        break;
                    Thread.Sleep(1000);
                }
            }

            if (!DeleteService(serviceHandle))
                throw new Win32Exception("Failed to delete service.");

            return true;
        }
    }
}
