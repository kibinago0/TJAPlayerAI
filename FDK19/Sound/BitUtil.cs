using System;
using System.Runtime.InteropServices;
using NAudio.Wave;
using Silk.NET.OpenAL;
using SDL;

namespace FDK
{
    public static class BitUtil
    {
        /// <summary>
        /// WaveStreamからOpenAL用のBufferFormatを取得します。
        /// </summary>
        public static BufferFormat GetBufferFormat(WaveStream waveStream)
        {
            if (waveStream.WaveFormat.Channels == 1)
            {
                return waveStream.WaveFormat.BitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16;
            }
            else
            {
                return waveStream.WaveFormat.BitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16;
            }
        }

        /// <summary>
        /// WaveStreamからSDL用のSDL_AudioFormatを取得します。
        /// </summary>
        public static SDL_AudioFormat GetSDLAudioFormat(WaveStream waveStream)
        {
            // ppy.SDL3-CS では SDL_AUDIO_ プレフィックスを除いた名前が使用されます
            return waveStream.WaveFormat.BitsPerSample switch
            {
                8 => SDL_AudioFormat.S8,
                16 => SDL_AudioFormat.S16,
                24 => SDL_AudioFormat.S32, // 24bitは32bitとして扱うか、別途変換が必要
                32 => (waveStream.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat) ? SDL_AudioFormat.F32 : SDL_AudioFormat.S32,
                _ => SDL_AudioFormat.S16,
            };
        }

        /// <summary>
        /// 24bitバイト配列を16bitバイト配列に高速変換。
        /// ポインタ演算を使用してオーバーヘッドを最小化。
        /// </summary>
        public static unsafe byte[] Bit24ToBit16(byte[] bytes)
        {
            int count = bytes.Length / 3;
            byte[] newB = new byte[count * 2];
            
            fixed (byte* pSrc = bytes, pDst = newB)
            {
                byte* src = pSrc;
                short* dst = (short*)pDst;

                for (int i = 0; i < count; i++)
                {
                    // 24bitのリトルエンディアンデータ(L, M, H)から上位16bit(M, H)を抽出
                    // src[i*3] は最下位バイト(L)なので捨て、i*3+1 と i*3+2 を結合
                    dst[i] = (short)(src[i * 3 + 1] | (src[i * 3 + 2] << 8));
                }
            }
            return newB;
        }
    }
}