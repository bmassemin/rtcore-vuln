using System;
using System.Collections.Generic;
using System.Text;

namespace RTCoreVuln
{
    internal unsafe interface IVulnerableDriver
    {
        void Read(ulong address, byte* buffer, uint size);
        void Write(ulong address, byte* buffer, uint size);
    }
}
