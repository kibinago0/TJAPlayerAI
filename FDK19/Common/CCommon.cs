using System;
using SkiaSharp;

namespace FDK
{
    public class CCommon
    {
        // 解放
        public static void tRunCompleteGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 指定されたImageで、backColor以外の色が使われている範囲を計測する
        /// ポインタアクセスによる高速化版
        /// </summary>
        public static unsafe SKRectI MeasureForegroundArea(SKBitmap bmp, SKColor backColor)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;

            uint* ptr = (uint*)bmp.GetPixels();
            uint targetColor = (uint)backColor;

            for (int y = 0; y < height; y++)
            {
                uint* row = ptr + (y * width);
                for (int x = 0; x < width; x++)
                {
                    if (row[x] != targetColor)
                    {
                        if (x < left) left = x;
                        if (x > right) right = x;
                        if (y < top) top = y;
                        if (y > bottom) bottom = y;
                    }
                }
            }

            if (right == -1) return SKRectI.Empty;
            return new SKRectI(left, top, right + 1, bottom + 1);
        }
    }
}