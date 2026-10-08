// Update code by Marcus Dudley <marcus @ nicsure . co . uk> to include Host API information - START

// License:     APL 2.0
// Author:      Marcus Dudley <marcus @ nicsure . co . uk>

using System;
using System.Text;
using System.Runtime.InteropServices;

using HostApiIndex = System.Int32;
using Time = System.Double;


namespace PortAudioSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HostApiInfoData
    {
        public int structVersion;
        public int type;
        public IntPtr name;
        public int deviceCount;
        public int defaultInputDevice;
        public int defaultOutputDevice;
    }

    public struct HostApiInfo
    {
        public int structVersion;
        public int type;
        public string name;
        public int deviceCount;
        public int defaultInputDevice;
        public int defaultOutputDevice;
    }
}

// Update code by Marcus Dudley <marcus @ nicsure . co . uk> to include Host API information - END
