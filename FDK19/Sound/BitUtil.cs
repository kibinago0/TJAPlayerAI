using System;
using System.Runtime.InteropServices;
using NAudio.Wave;
using Silk.NET.OpenAL;
using SDL;

namespace FDK
{
    public static class BitUtil
    {
        public static BufferFormat GetBufferFormat(WaveStream waveStream)
        {
            if (waveStream.WaveFormat.Channels == 1)
                return waveStream.WaveFormat.BitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16;
            return waveStream.WaveFormat.BitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16;
        }

        public static SDL_AudioFormat GetSDLAudioFormat(WaveStream waveStream)
        {
            return waveStream.WaveFormat.BitsPerSample switch
            {
                8 => (SDL_AudioFormat)0x0008,  // AUDIO_S8
                16 => (SDL_AudioFormat)0x8010, // AUDIO_S16LE
                24 => (SDL_AudioFormat)0x8020, // AUDIO_S32LE
                32 => (waveStream.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat) ? (SDL_AudioFormat)0x8120 : (SDL_AudioFormat)0x8020,
                _ => (SDL_AudioFormat)0x8010,
            };
        }

        public static unsafe byte[] Bit24ToBit16(byte[] bytes)
        {
            int count = bytes.Length / 3;
            byte[] newB = new byte[count * 2];
            fixed (byte* pSrc = bytes, pDst = newB) {
                byte* src = pSrc;
                short* dst = (short*)pDst;
                for (int i = 0; i < count; i++)
                    dst[i] = (short)(src[i * 3 + 1] | (src[i * 3 + 2] << 8));
            }
            return newB;
        }
    }
}