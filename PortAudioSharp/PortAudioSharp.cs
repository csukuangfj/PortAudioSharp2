// License:     APL 2.0
// Author:      Benjamin N. Summerton <https://16bpp.net>

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NativeLibraryManager;

using DeviceIndex = System.Int32;

namespace PortAudioSharp
{
    internal static partial class Native
    {
        public const string PortAudioDLL = "portaudio";

        [DllImport(PortAudioDLL)]
        public static extern int Pa_GetVersion();

        [DllImport(PortAudioDLL)]
        public static extern IntPtr Pa_GetVersionInfo();           // Originally returns `const PaVersionInfo *`

        [DllImport(PortAudioDLL)]
        public static extern IntPtr Pa_GetErrorText([MarshalAs(UnmanagedType.I4)] ErrorCode errorCode);     // Orignially returns `const char *`

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_Initialize();

        [DllImport(PortAudioDLL)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern ErrorCode Pa_Terminate();

        [DllImport(PortAudioDLL)]
        public static extern DeviceIndex Pa_GetDefaultOutputDevice();

        [DllImport(PortAudioDLL)]
        public static extern DeviceIndex Pa_GetDefaultInputDevice();

        [DllImport(PortAudioDLL)]
        public static extern IntPtr Pa_GetDeviceInfo(DeviceIndex device);   // Originally returns `const PaDeviceInfo *`

        [DllImport(PortAudioDLL)]
        public static extern DeviceIndex Pa_GetDeviceCount();

        [DllImport(PortAudioDLL)]
        public static extern void Pa_Sleep(System.Int32 msec);
    }

    public static class PortAudio
    {
        #region Constants
        /// <summary>
        /// A special PaDeviceIndex value indicating that no device is available,
        /// or should be used.
        ///
        /// @see PaDeviceIndex
        /// </summary>
        public const DeviceIndex NoDevice = -1;

        /// <summary>
        /// Can be passed as the framesPerBuffer parameter to Pa_OpenStream()
        /// or Pa_OpenDefaultStream() to indicate that the stream callback will
        /// accept buffers of any size.
        /// </summary>
        public const System.UInt32 FramesPerBufferUnspecified = 0;
        #endregion // Constants

        /// <summary>
        /// Retrieve the release number of the currently running PortAudio build.
        /// For example, for version "19.5.1" this will return 0x00130501.
        ///
        /// @see paMakeVersionNumber
        /// </summary>
        /// <value></value>
        public static int Version
        {
            get => Native.Pa_GetVersion();
        }

        /// <summary>
        /// Retrieve version information for the currently running PortAudio build.
        /// @return A pointer to an immutable PaVersionInfo structure.
        ///
        /// @note This function can be called at any time. It does not require PortAudio
        /// to be initialized. The structure pointed to is statically allocated. Do not
        /// attempt to free it or modify it.
        ///
        /// @see PaVersionInfo, paMakeVersionNumber
        /// @version Available as of 19.5.0.
        /// </summary>
        public static VersionInfo VersionInfo
        {
            get => Marshal.PtrToStructure<VersionInfo>(Native.Pa_GetVersionInfo());
        }

        /// <summary>
        /// Translate the supplied PortAudio error code into a human readable
        /// message.
        /// </summary>
        public static string GetErrorText(ErrorCode errorCode) =>
            Marshal.PtrToStringAnsi(Native.Pa_GetErrorText(errorCode));

        /// <summary>
        /// This is a function that's not found in the original PortAudio library.  Because of how the native libraries are
        /// packaged, this function must be called before anything else with the package.  It loads the native shared library
        /// (from an embedded resource) and makes it accessible.
        /// </summary>
        public static void LoadNativeLibrary()
        {
            /*
                Commented out in development branch so `/third_party/lib` DLLs can be used instead

            // Extract the the native library that has been embedded and load it up
            ResourceAccessor accessor = new ResourceAccessor(Assembly.GetExecutingAssembly());
            LibraryManager libManager = new LibraryManager(
                Assembly.GetExecutingAssembly(),
                new LibraryItem(Platform.Linux,   Bitness.x64, new LibraryFile("libportaudio.so",    accessor.Binary("libportaudio.so"))),
                new LibraryItem(Platform.MacOs,   Bitness.x64, new LibraryFile("libportaudio.dylib", accessor.Binary("libportaudio.dylib"))),
                new LibraryItem(Platform.Windows, Bitness.x64, new LibraryFile("portaudio.dll",      accessor.Binary("portaudio.dll")))
            );
            libManager.LoadNativeLibrary();
            */
        }

        /// <summary>
        /// Library initialization function - call this before using PortAudio.
        /// This function initializes internal data structures and prepares underlying
        /// host APIs for use.  With the exception of Pa_GetVersion(), Pa_GetVersionText(),
        /// and Pa_GetErrorText(), this function MUST be called before using any other
        /// PortAudio API functions.
        ///
        /// If Pa_Initialize() is called multiple times, each successful
        /// call must be matched with a corresponding call to Pa_Terminate().
        /// Pairs of calls to Pa_Initialize()/Pa_Terminate() may overlap, and are not
        /// required to be fully nested.
        ///
        /// Note that if Pa_Initialize() returns an error code, Pa_Terminate() should
        /// NOT be called.
        ///
        /// @return paNoError if successful, otherwise an error code indicating the cause
        /// of failure.
        ///
        /// @see Pa_Terminate
        /// </summary>
        public static void Initialize()
        {
            ErrorCode ec = Native.Pa_Initialize();
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error initializing PortAudio");
        }

        /// <summary>
        /// Library termination function - call this when finished using PortAudio.
        /// This function deallocates all resources allocated by PortAudio since it was
        /// initialized by a call to Pa_Initialize(). In cases where Pa_Initialise() has
        /// been called multiple times, each call must be matched with a corresponding call
        /// to Pa_Terminate(). The final matching call to Pa_Terminate() will automatically
        /// close any PortAudio streams that are still open.
        ///
        /// Pa_Terminate() MUST be called before exiting a program which uses PortAudio.
        /// Failure to do so may result in serious resource leaks, such as audio devices
        /// not being available until the next reboot.
        ///
        /// @return paNoError if successful, otherwise an error code indicating the cause
        /// of failure.
        ///
        /// @see Pa_Initialize
        /// </summary>
        public static void Terminate()
        {
            ErrorCode ec = Native.Pa_Terminate();
            if (ec != ErrorCode.NoError)
                throw new PortAudioException(ec, "Error terminating PortAudio");
        }

        /// <summary>
        /// Retrieve the index of the default output device. The result can be
        /// used in the outputDevice parameter to Pa_OpenStream().
        ///
        /// @return The default output device index for the default host API, or paNoDevice
        /// if no default output device is available or an error was encountered.
        ///
        /// @note
        /// On the PC, the user can specify a default device by
        /// setting an environment variable. For example, to use device #1.
        /// <pre>
        /// set PA_RECOMMENDED_OUTPUT_DEVICE=1
        /// </pre>
        /// The user should first determine the available device ids by using
        /// the supplied application "pa_devs".
        /// </summary>
        public static DeviceIndex DefaultOutputDevice
        {
            get => Native.Pa_GetDefaultOutputDevice();
        }

        /// <summary>
        /// Retrieve the index of the default input device. The result can be
        /// used in the inputDevice parameter to Pa_OpenStream().
        ///
        /// @return The default input device index for the default host API, or paNoDevice
        /// if no default input device is available or an error was encountered.
        /// </summary>
        public static DeviceIndex DefaultInputDevice
        {
            get => Native.Pa_GetDefaultInputDevice();
        }

        /// <summary>
        /// Retrieve a pointer to a PaDeviceInfo structure containing information
        /// about the specified device.
        /// @return A pointer to an immutable PaDeviceInfo structure. If the device
        /// parameter is out of range the function returns NULL.
        ///
        /// @param device A valid device index in the range 0 to (Pa_GetDeviceCount()-1)
        ///
        /// @note PortAudio manages the memory referenced by the returned pointer,
        /// the client must not manipulate or free the memory. The pointer is only
        /// guaranteed to be valid between calls to Pa_Initialize() and Pa_Terminate().
        ///
        /// @see PaDeviceInfo, PaDeviceIndex
        /// </summary>
        public static DeviceInfo GetDeviceInfo(DeviceIndex device) =>
            Marshal.PtrToStructure<DeviceInfo>(Native.Pa_GetDeviceInfo(device));

        /// <summary>
        /// Retrieve the number of available devices. The number of available devices
        /// may be zero.
        ///
        /// @return A non-negative value indicating the number of available devices
        /// or, a PaErrorCode (which are always negative) if PortAudio is not initialized
        /// or an error is encountered.
        /// </summary>
        public static DeviceIndex DeviceCount
        {
            get => Native.Pa_GetDeviceCount();
        }
    }
}
