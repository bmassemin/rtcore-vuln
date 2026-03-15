using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace RTCoreVuln;

/// <summary>
/// Static helpers for communicating with a device driver via IOCTL.
/// </summary>
internal static unsafe class DeviceIo
{
    public static void IoControl(SafeHandle handle, uint dwIoControlCode, void* lpInBuffer, uint nInBufferSize, void* lpOutBuffer, uint nOutBufferSize)
    {
        var unsafeHandle = (HANDLE)handle.DangerousGetHandle();
        if (!DeviceIoControl(unsafeHandle, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize))
            throw new Win32Exception("DeviceIoControl failed.");
    }

    public static void IoControl<TI, TO>(SafeHandle handle, uint code, TI* bufferIn, TO* bufferOut)
        where TI : unmanaged
        where TO : unmanaged
    {
        IoControl(handle, code, bufferIn, (uint)sizeof(TI), bufferOut, (uint)sizeof(TO));
    }

    public static void IoControl<T>(SafeHandle handle, uint code, T* bufferInAndOut)
        where T : unmanaged
    {
        IoControl(handle, code, bufferInAndOut, (uint)sizeof(T), bufferInAndOut, (uint)sizeof(T));
    }

    public static uint CtlCode(uint deviceType, uint function, uint method, uint access)
    {
        return (deviceType << 16) | (access << 14) | (function << 2) | method;
    }
}
