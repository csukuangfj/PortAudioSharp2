// License:     APL 2.0
// Author:      Benjamin N. Summerton <https://16bpp.net>

using System;
using System.Text;
using System.Runtime.InteropServices;

using Time = System.Double;

namespace PortAudioSharp
{
    /// <summary>
    /// A structure containing unchanging information about an open stream.
    /// Mirrors `PaStreamInfo` from portaudio.h; retrieved with <see cref="Stream.Info"/>
    /// (`Pa_GetStreamInfo()`).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StreamInfo
    {
        /// <summary>
        /// This is struct version 1.
        /// </summary>
        public int structVersion;

        /// <summary>
        /// The input latency of the stream in seconds. This value provides the most
        /// accurate estimate of input latency available to the implementation. It may
        /// differ significantly from the suggestedLatency value passed to Pa_OpenStream().
        /// The value of this field will be zero (0.) for output-only streams.
        /// @see PaTime
        /// </summary>
        public Time inputLatency;

        /// <summary>
        /// The output latency of the stream in seconds. This value provides the most
        /// accurate estimate of output latency available to the implementation. It may
        /// differ significantly from the suggestedLatency value passed to Pa_OpenStream().
        /// The value of this field will be zero (0.) for input-only streams.
        /// @see PaTime
        /// </summary>
        public Time outputLatency;

        /// <summary>
        /// The sample rate of the stream in Hertz (samples per second). In cases
        /// where the hardware sample rate is inaccurate and PortAudio is aware of it,
        /// the value of this field may be different from the sampleRate parameter
        /// passed to Pa_OpenStream(). If information about the actual hardware sample
        /// rate is not available, this field will have the same value as the sampleRate
        /// parameter passed to Pa_OpenStream().
        /// </summary>
        public double sampleRate;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("StreamInfo [");
            sb.AppendLine($"  structVersion={structVersion}");
            sb.AppendLine($"  inputLatency={inputLatency}");
            sb.AppendLine($"  outputLatency={outputLatency}");
            sb.AppendLine($"  sampleRate={sampleRate}");
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}
