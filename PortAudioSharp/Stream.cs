// License:     APL 2.0
// Author:      Benjamin N. Summerton <https://16bpp.net>

using System;
using System.Runtime.InteropServices;

namespace PortAudioSharp
{
    internal static partial class Native
    {
        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_OpenStream(
            out IntPtr stream,                          // `PaStream **`
            IntPtr inputParameters,                     // `const PaStreamParameters *`
            IntPtr outputParameters,                    // `const PaStreamParameters *`
            double sampleRate,
            System.UInt32 framesPerBuffer,
            StreamFlags streamFlags,
            IntPtr streamCallback,                      // `PaStreamCallback *`
            IntPtr userData                             // `void *`
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I4)]
        public delegate StreamCallbackResult Callback(
            IntPtr input, IntPtr output,                // Originally `const void *, void *`
            System.UInt32 frameCount,
            ref StreamCallbackTimeInfo timeInfo,        // Originally `const PaStreamCallbackTimeInfo*`
            StreamCallbackFlags statusFlags,
            IntPtr userData                             // Orignially `void *`
        );

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_CloseStream(IntPtr stream);       // `PaStream *`

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_SetStreamFinishedCallback(
            IntPtr stream,                                                  // `PaStream *`
            IntPtr streamFinishedCallback                                   // `PaStreamFinishedCallback *`
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FinishedCallback(
            IntPtr userData                         // Originally `void *`
        );

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_StartStream(IntPtr stream);       // `PaStream *`

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_StopStream(IntPtr stream);        // `PaStream *`

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_AbortStream(IntPtr stream);       // `PaStream *`

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_IsStreamStopped(IntPtr stream);   // `PaStream *`

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_IsStreamActive(IntPtr stream);    // `PaStream *`

        [DllImport(PortAudioDLL)]
        public static extern double Pa_GetStreamTime(IntPtr stream);        // `PaStream *`

        [DllImport(PortAudioDLL)]
        public static extern double Pa_GetStreamCpuLoad(IntPtr stream);     // `PaStream *`
    }

    /// <summary>
    /// A single PaStream can provide multiple channels of real-time
    /// streaming audio input and output to a client application. A stream
    /// provides access to audio hardware represented by one or more
    /// PaDevices. Depending on the underlying Host API, it may be possible
    /// to open multiple streams using the same device, however this behavior
    /// is implementation defined. Portable applications should assume that
    /// a PaDevice may be simultaneously used by at most one PaStream.
    ///
    /// Pointers to PaStream objects are passed between PortAudio functions that
    /// operate on streams.
    ///
    /// @see Pa_OpenStream, Pa_OpenDefaultStream, Pa_OpenDefaultStream, Pa_CloseStream,
    /// Pa_StartStream, Pa_StopStream, Pa_AbortStream, Pa_IsStreamActive,
    /// Pa_GetStreamTime, Pa_GetStreamCpuLoad
    /// </summary>
    public class Stream : IDisposable
    {
        // Clean & manually managed data
        private bool disposed = false;
        private IntPtr streamPtr = IntPtr.Zero;      // `Stream *`
        private GCHandle userDataHandle;

        // Callback structures
        private _NativeInterfacingCallback<Callback> streamCallback = null;
        private _NativeInterfacingCallback<FinishedCallback> finishedCallback = null;

        /// <summary>
        /// The input parameters for this stream, if any
        /// </summary>
        /// <value>will be `null` if the user never supplied any</value>
        public StreamParameters? inputParameters { get; private set; }

        /// <summary>
        /// The output parameters for this stream, if any
        /// </summary>
        /// <value>will be `null` if the user never supplied any</value>
        public StreamParameters? outputParameters { get; private set; }


        #region Constructors & Cleanup
        /// <summary>
        /// Opens a stream for either input, output or both.
        ///
        /// @param stream The address of a PaStream pointer which will receive
        /// a pointer to the newly opened stream.
        ///
        /// @param inputParameters A structure that describes the input parameters used by
        /// the opened stream. See PaStreamParameters for a description of these parameters.
        /// inputParameters must be NULL for output-only streams.
        ///
        /// @param outputParameters A structure that describes the output parameters used by
        /// the opened stream. See PaStreamParameters for a description of these parameters.
        /// outputParameters must be NULL for input-only streams.
        ///
        /// @param sampleRate The desired sampleRate. For full-duplex streams it is the
        /// sample rate for both input and output
        ///
        /// @param framesPerBuffer The number of frames passed to the stream callback
        /// function, or the preferred block granularity for a blocking read/write stream.
        /// The special value paFramesPerBufferUnspecified (0) may be used to request that
        /// the stream callback will receive an optimal (and possibly varying) number of
        /// frames based on host requirements and the requested latency settings.
        /// Note: With some host APIs, the use of non-zero framesPerBuffer for a callback
        /// stream may introduce an additional layer of buffering which could introduce
        /// additional latency. PortAudio guarantees that the additional latency
        /// will be kept to the theoretical minimum however, it is strongly recommended
        /// that a non-zero framesPerBuffer value only be used when your algorithm
        /// requires a fixed number of frames per stream callback.
        ///
        /// @param streamFlags Flags which modify the behavior of the streaming process.
        /// This parameter may contain a combination of flags ORed together. Some flags may
        /// only be relevant to certain buffer formats.
        ///
        /// @param streamCallback A pointer to a client supplied function that is responsible
        /// for processing and filling input and output buffers. If this parameter is NULL
        /// the stream will be opened in 'blocking read/write' mode. In blocking mode,
        /// the client can receive sample data using Pa_ReadStream and write sample data
        /// using Pa_WriteStream, the number of samples that may be read or written
        /// without blocking is returned by Pa_GetStreamReadAvailable and
        /// Pa_GetStreamWriteAvailable respectively.
        ///
        /// @param userData A client supplied pointer which is passed to the stream callback
        /// function. It could for example, contain a pointer to instance data necessary
        /// for processing the audio buffers. This parameter is ignored if streamCallback
        /// is NULL.
        ///   NOTE: userData will no longer be automatically GC'd normally by C#.  The cleanup
        ///         of that will be handled by this class upon `Dipose()` or deletion. You (the
        ///         programmer), shouldn't have to worry about this.
        ///
        /// @return
        /// Upon success Pa_OpenStream() returns paNoError and places a pointer to a
        /// valid PaStream in the stream argument. The stream is inactive (stopped).
        /// If a call to Pa_OpenStream() fails, a non-zero error code is returned (see
        /// PaError for possible error codes) and the value of stream is invalid.
        ///
        /// @see PaStreamParameters, PaStreamCallback, Pa_ReadStream, Pa_WriteStream,
        /// Pa_GetStreamReadAvailable, Pa_GetStreamWriteAvailable
        /// </summary>
        public Stream(
            StreamParameters? inParams,
            StreamParameters? outParams,
            double sampleRate,
            System.UInt32 framesPerBuffer,
            StreamFlags streamFlags,
            Callback callback,
            object userData
        )
        {
            // Setup the steam's callback
            streamCallback = new _NativeInterfacingCallback<Callback>(callback);

            // Take control of the userdata object
            userDataHandle = GCHandle.Alloc(userData);

            // Set the ins and the outs
            inputParameters = inParams;
            outputParameters = outParams;

            // If the in/out params are set, then we need to make some P/Invoke friendly memory
            IntPtr inParamsPtr = IntPtr.Zero;
            IntPtr outParamsPtr = IntPtr.Zero;
            if (inParams.HasValue)
            {
                inParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(inParams.Value));
                Marshal.StructureToPtr<StreamParameters>(inParams.Value, inParamsPtr, false);
            }
            if (outParams.HasValue)
            {
                outParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(outParams.Value));
                Marshal.StructureToPtr<StreamParameters>(outParams.Value, outParamsPtr, false);
            }

            // Open the stream
            ErrorCode ec = Native.Pa_OpenStream(
                out streamPtr,
                inParamsPtr,
                outParamsPtr,
                sampleRate,
                framesPerBuffer,
                streamFlags,
                streamCallback.Ptr,
                GCHandle.ToIntPtr(userDataHandle)
            );
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error opening PortAudio Stream");

            // Cleanup the in/out params ptrs
            if (inParamsPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(inParamsPtr);
            if (outParamsPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(outParamsPtr);
        }

        ~Stream()
        {
            dispose(false);
        }

        /// <summary>
        /// Cleanup resources (for the IDisposable interface)
        /// </summary>
        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Does the actual disposing work
        /// </summary>
        protected virtual void dispose(bool disposing)
        {
            if (disposed)
                return;

            // Free Managed Resources
            if (disposing)
            {
            }

            // Free Unmanaged resources
            Close();
            userDataHandle.Free();
            streamCallback.Free();
            if (finishedCallback != null)
                finishedCallback.Free();

            disposed = true;
        }
        #endregion // Constructors & Cleanup

        /// <summary>
        /// Set a callback to be triggered when the stream is done.
        /// </summary>
        public void SetFinishedCallback(FinishedCallback fcb)
        {
            finishedCallback = new _NativeInterfacingCallback<FinishedCallback>(fcb);

            // TODO what happens if a callback is already set?  Find out and make the necessary adjustments
            ErrorCode ec = Native.Pa_SetStreamFinishedCallback(streamPtr, finishedCallback.Ptr);
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error setting finished callback for PortAudio Stream");
        }

        #region Operations
        /// <summary>
        /// Closes an audio stream. If the audio stream is active it
        /// discards any pending buffers as if Pa_AbortStream() had been called.
        /// </summary>
        public void Close()
        {
            // Did we already clean up?
            if (streamPtr == IntPtr.Zero)
                return;

            ErrorCode ec = Native.Pa_CloseStream(streamPtr);
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error closing PortAudio Stream");

            // Reset the handle, since we've cleaned up
            streamPtr = IntPtr.Zero;
        }

        /// <summary>
        /// Commences audio processing.
        /// </summary>
        public void Start()
        {
            ErrorCode ec = Native.Pa_StartStream(streamPtr);
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error starting PortAudio Stream");
        }

        /// <summary>
        /// Terminates audio processing. It waits until all pending
        /// audio buffers have been played before it returns.
        /// </summary>
        public void Stop()
        {
            ErrorCode ec = Native.Pa_StopStream(streamPtr);
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error stopping PortAudio Stream");
        }

        /// <summary>
        /// Terminates audio processing immediately without waiting for pending
        /// buffers to complete.
        /// </summary>
        public void Abort()
        {
            ErrorCode ec = Native.Pa_AbortStream(streamPtr);
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error aborting PortAudio Stream");
        }
        #endregion // Operations

        #region Properties
        /// <summary>
        /// Determine whether the stream is stopped.
        /// A stream is considered to be stopped prior to a successful call to
        /// Pa_StartStream and after a successful call to Pa_StopStream or Pa_AbortStream.
        /// If a stream callback returns a value other than paContinue the stream is NOT
        /// considered to be stopped.
        ///
        /// @return Returns one (1) when the stream is stopped, zero (0) when
        /// the stream is running or, a PaErrorCode (which are always negative) if
        /// PortAudio is not initialized or an error is encountered.
        ///
        /// @see Pa_StopStream, Pa_AbortStream, Pa_IsStreamActive
        /// </summary>
        public bool IsStopped
        {
            get
            {
                ErrorCode ec = Native.Pa_IsStreamStopped(streamPtr);

                // Yes, No, or wat?
                if ((int)ec == 1)
                    return true;
                else if ((int)ec == 0)
                    return false;
                else
                    throw new PortAudioException(ec, "Error checking if PortAudio Stream is stopped");
            }
        }

        /// <summary>
        /// Determine whether the stream is active.
        /// A stream is active after a successful call to Pa_StartStream(), until it
        /// becomes inactive either as a result of a call to Pa_StopStream() or
        /// Pa_AbortStream(), or as a result of a return value other than paContinue from
        /// the stream callback. In the latter case, the stream is considered inactive
        /// after the last buffer has finished playing.
        ///
        /// @return Returns one (1) when the stream is active (ie playing or recording
        /// audio), zero (0) when not playing or, a PaErrorCode (which are always negative)
        /// if PortAudio is not initialized or an error is encountered.
        ///
        /// @see Pa_StopStream, Pa_AbortStream, Pa_IsStreamStopped
        /// </summary>
        public bool IsActive
        {
            get
            {
                ErrorCode ec = Native.Pa_IsStreamActive(streamPtr);

                // Yes, No, or wat?
                if ((int)ec == 1)
                    return true;
                else if ((int)ec == 0)
                    return false;
                else
                    throw new PortAudioException(ec, "Error checking if PortAudio Stream is active");
            }
        }

        /// <summary>
        /// Returns the current time in seconds for a stream according to the same clock
        /// used to generate callback PaStreamCallbackTimeInfo timestamps. The time values
        /// are monotonically increasing and have unspecified origin.
        ///
        /// Pa_GetStreamTime returns valid time values for the entire life of the stream,
        /// from when the stream is opened until it is closed. Starting and stopping the
        /// stream does not affect the passage of time returned by Pa_GetStreamTime.
        ///
        /// This time may be used for synchronizing other events to the audio stream,
        /// for example synchronizing audio to MIDI.
        ///
        /// @return
        /// The stream's current time in seconds, or 0 if an error occurred.
        ///
        /// @see PaTime, PaStreamCallback, PaStreamCallbackTimeInfo
        /// </summary>
        public double Time
        {
            get => Native.Pa_GetStreamTime(streamPtr);
        }

        /// <summary>
        /// Retrieve CPU usage information for the specified stream.
        /// The "CPU Load" is a fraction of total CPU time consumed by a callback stream's
        /// audio processing routines including, but not limited to the client supplied
        /// stream callback. This function does not work with blocking read/write streams.
        ///
        /// This function may be called from the stream callback function or the
        /// application.
        ///
        /// @return
        /// A floating point value, typically between 0.0 and 1.0, where 1.0 indicates
        /// that the stream callback is consuming the maximum number of CPU cycles possible
        /// to maintain real-time operation. A value of 0.5 would imply that PortAudio and
        /// the stream callback was consuming roughly 50% of the available CPU time. The
        /// return value may exceed 1.0. A value of 0.0 will always be returned for a
        /// blocking read/write stream, or if an error occurs.
        /// </summary>
        public double CpuLoad
        {
            get => Native.Pa_GetStreamCpuLoad(streamPtr);
        }
        #endregion Properties

        #region Programmer Friendly Callbacks
        /// <summary>
        /// Functions of type PaStreamCallback are implemented by PortAudio clients.
        /// They consume, process or generate audio in response to requests from an
        /// active PortAudio stream.
        ///
        /// When a stream is running, PortAudio calls the stream callback periodically.
        /// The callback function is responsible for processing buffers of audio samples
        /// passed via the input and output parameters.
        ///
        /// The PortAudio stream callback runs at very high or real-time priority.
        /// It is required to consistently meet its time deadlines. Do not allocate
        /// memory, access the file system, call library functions or call other functions
        /// from the stream callback that may block or take an unpredictable amount of
        /// time to complete.
        ///
        /// In order for a stream to maintain glitch-free operation the callback
        /// must consume and return audio data faster than it is recorded and/or
        /// played. PortAudio anticipates that each callback invocation may execute for
        /// a duration approaching the duration of frameCount audio frames at the stream
        /// sample rate. It is reasonable to expect to be able to utilise 70% or more of
        /// the available CPU time in the PortAudio callback. However, due to buffer size
        /// adaption and other factors, not all host APIs are able to guarantee audio
        /// stability under heavy CPU load with arbitrary fixed callback buffer sizes.
        /// When high callback CPU utilisation is required the most robust behavior
        /// can be achieved by using paFramesPerBufferUnspecified as the
        /// Pa_OpenStream() framesPerBuffer parameter.
        ///
        /// @param input and @param output are either arrays of interleaved samples or;
        /// if non-interleaved samples were requested using the paNonInterleaved sample
        /// format flag, an array of buffer pointers, one non-interleaved buffer for
        /// each channel.
        ///
        /// The format, packing and number of channels used by the buffers are
        /// determined by parameters to Pa_OpenStream().
        ///
        /// @param frameCount The number of sample frames to be processed by
        /// the stream callback.
        ///
        /// @param timeInfo Timestamps indicating the ADC capture time of the first sample
        /// in the input buffer, the DAC output time of the first sample in the output buffer
        /// and the time the callback was invoked.
        /// See PaStreamCallbackTimeInfo and Pa_GetStreamTime()
        ///
        /// @param statusFlags Flags indicating whether input and/or output buffers
        /// have been inserted or will be dropped to overcome underflow or overflow
        /// conditions.
        ///
        /// @param userData The value of a user supplied pointer passed to
        /// Pa_OpenStream() intended for storing synthesis data etc.
        ///   NOTE: In the implementing callback, you can use the `GetUserData()` method to
        ///         retrive the actual object.
        ///
        /// @return
        /// The stream callback should return one of the values in the
        /// ::PaStreamCallbackResult enumeration. To ensure that the callback continues
        /// to be called, it should return paContinue (0). Either paComplete or paAbort
        /// can be returned to finish stream processing, after either of these values is
        /// returned the callback will not be called again. If paAbort is returned the
        /// stream will finish as soon as possible. If paComplete is returned, the stream
        /// will continue until all buffers generated by the callback have been played.
        /// This may be useful in applications such as soundfile players where a specific
        /// duration of output is required. However, it is not necessary to utilize this
        /// mechanism as Pa_StopStream(), Pa_AbortStream() or Pa_CloseStream() can also
        /// be used to stop the stream. The callback must always fill the entire output
        /// buffer irrespective of its return value.
        ///
        /// @see Pa_OpenStream, Pa_OpenDefaultStream
        ///
        /// @note With the exception of Pa_GetStreamCpuLoad() it is not permissible to call
        /// PortAudio API functions from within the stream callback.
        /// </summary>
        public delegate StreamCallbackResult Callback(
            IntPtr input, IntPtr output,                // Originally `const void *, void *`
            System.UInt32 frameCount,
            ref StreamCallbackTimeInfo timeInfo,        // Originally `const PaStreamCallbackTimeInfo*`
            StreamCallbackFlags statusFlags,
            IntPtr userDataPtr                          // Orignially `void *`
        );

        /// <summary>
        /// Functions of type PaStreamFinishedCallback are implemented by PortAudio
        /// clients. They can be registered with a stream using the Pa_SetStreamFinishedCallback
        /// function. Once registered they are called when the stream becomes inactive
        /// (ie once a call to Pa_StopStream() will not block).
        /// A stream will become inactive after the stream callback returns non-zero,
        /// or when Pa_StopStream or Pa_AbortStream is called. For a stream providing audio
        /// output, if the stream callback returns paComplete, or Pa_StopStream() is called,
        /// the stream finished callback will not be called until all generated sample data
        /// has been played.
        ///
        /// @param userData The userData parameter supplied to Pa_OpenStream()
        ///   NOTE: In the implementing callback, you can use the `GetUserData()` method to
        ///         retrive the actual object.
        ///
        /// @see Pa_SetStreamFinishedCallback
        /// </summary>
        public delegate void FinishedCallback(
            IntPtr userDataPtr                          // Originally `void *`
        );
        #endregion // Callbacks

        /// <summary>
        /// This function will retrieve the `userData` of the stream from it's pointer.
        ///
        /// This is meant to be used by the callbacks for `Callback` and `FinishedCallback`, and
        /// their `userDataPtr`.
        /// </summary>
        /// <param name="userDataPtr"></param>
        /// <typeparam name="UD">The type of data that was put into the stream</typeparam>
        /// <returns></returns>
        public static UD GetUserData<UD>(IntPtr userDataPtr) =>
            (UD)GCHandle.FromIntPtr(userDataPtr).Target;


        /// <summary>
        /// This is an internal structure to aid with C# Callbacks that interface with P/Invoke functions.
        ///
        /// The constructor, the `Free()` method, and the `Ptr` property are all that you can use, and are
        /// the most important parts.
        /// </summary>
        /// <typeparam name="CB">Callback</typeparam>
        private class _NativeInterfacingCallback<CB>
            where CB : Delegate
        {
            /// <summary>
            /// The callback itself (needs to be a delegate)
            /// </summary>
            private CB callback;

            /// <summary>
            /// GC Handle to the callback
            /// </summary>
            private GCHandle handle;

            /// <summary>
            /// Get the pointer to where the function/delegate lives in memory
            /// </summary>
            public IntPtr Ptr { get; private set; } = IntPtr.Zero;

            /// <summary>
            /// Setup the data structure.
            ///
            /// When done with it, don't forget to call the Free() method.
            /// </summary>
            /// <param name="cb"></param>
            public _NativeInterfacingCallback(CB cb)
            {
                callback = cb ?? throw new ArgumentNullException(nameof(cb));
                handle = GCHandle.Alloc(cb);
                Ptr = Marshal.GetFunctionPointerForDelegate<CB>(cb);
            }

            /// <summary>
            /// Manually clean up memory
            /// </summary>
            public void Free()
            {
                handle.Free();
            }
        }
    }
}
