# Introduction

C# binding for [portaudio][portaudio] supporting Linux, macOS, and Windows.


See <https://www.nuget.org/packages/PortAudioSharp2>

The binding code is copied from [PortAudioSharp][PortAudioSharp].

Different from [PortAudioSharp][PortAudioSharp], this project packages pre-compiled
[portaudio][portaudio] into the `nuget` package, which simplifies user's life.

It's worth mentioning again that the binding code is copied
directly from [PortAudioSharp][PortAudioSharp].

[PortAudioSharp]: https://gitlab.com/define-private-public/Bassoon/-/tree/develop/src/Bassoon/PortAudioSharp
[portaudio]: https://github.com/PortAudio/portaudio

# Examples

## Recording for speech-to-text

You can find its usage for real-time speech-to-text from a microphone using
[sherpa-onnx](https://github.com/k2-fsa/sherpa-onnx) at
<https://github.com/k2-fsa/sherpa-onnx/tree/master/dotnet-examples/speech-recognition-from-microphone>

## Playing for text-to-speech

You can find its usage for text-to-speech with a speaker using
[sherpa-onnx](https://github.com/k2-fsa/sherpa-onnx) at
<https://github.com/k2-fsa/sherpa-onnx/tree/master/dotnet-examples/offline-tts-play>
