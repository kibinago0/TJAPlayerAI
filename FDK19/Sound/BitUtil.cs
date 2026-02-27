using System;
using System.Runtime.InteropServices;

namespace FDK
{
    public static class BitUtil
    {
        // ... 他のメソッドは既存通り ...

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
                    // 24bitのリトルエンディアンデータから上位16bitを抽出
                    dst[i] = (short)((src[i * 3 + 1]) | (src[i * 3 + 2] << 8));
                }
            }
            return newB;
        }
    }
}