using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using SkiaSharp;

namespace FDK
{
    /// <summary>
    /// 高速描画版のCFontRendererクラス。
    /// レンダリング結果をハッシュマップで管理し、O(1)での高速検索を実現します。
    /// </summary>
    public class CCachedFontRenderer : CFontRenderer
    {
        // キャッシュキー用の構造体（検索時のアロケーションをゼロにするため）
        private struct FontCacheKey : IEquatable<FontCacheKey>
        {
            public string drawstr;
            public DrawMode drawmode;
            public Color fontColor;
            public Color edgeColor;
            public Color gradationTopColor;
            public Color gradationBottomColor;
            public int edge_Ratio;
            public bool vertical;

            public bool Equals(FontCacheKey other)
            {
                return drawstr == other.drawstr &&
                       drawmode == other.drawmode &&
                       fontColor == other.fontColor &&
                       edgeColor == other.edgeColor &&
                       gradationTopColor == other.gradationTopColor &&
                       gradationBottomColor == other.gradationBottomColor &&
                       edge_Ratio == other.edge_Ratio &&
                       vertical == other.vertical;
            }

            public override bool Equals(object obj) => obj is FontCacheKey other && Equals(other);

            public override int GetHashCode()
            {
                var hash = new HashCode();
                hash.Add(drawstr);
                hash.Add(drawmode);
                hash.Add(fontColor);
                hash.Add(edgeColor);
                hash.Add(gradationTopColor);
                hash.Add(gradationBottomColor);
                hash.Add(edge_Ratio);
                hash.Add(vertical);
                return hash.ToHashCode();
            }
        }

        private class FontCache
        {
            public SKBitmap bmp;
            public long lastUsedTick; // LRU管理用
        }

        private Dictionary<FontCacheKey, FontCache> _cache = new Dictionary<FontCacheKey, FontCache>();
        private int nCacheSize = 64; // キャッシュする最大数

        public CCachedFontRenderer(string fontpath, int pt, FontStyle style = FontStyle.Regular) 
            : base(fontpath, pt, style)
        {
        }

        // --- オーバーロード群 ---

        public new SKBitmap DrawText(string drawstr, Color fontColor)
            => DrawTextInternal(drawstr, DrawMode.Normal, fontColor, Color.White, Color.White, Color.White, 0, false);

        public new SKBitmap DrawText(string drawstr, Color fontColor, Color edgeColor, int edge_Ratio)
            => DrawTextInternal(drawstr, DrawMode.Edge, fontColor, edgeColor, Color.White, Color.White, edge_Ratio, false);

        public new SKBitmap DrawText(string drawstr, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio)
            => DrawTextInternal(drawstr, DrawMode.Edge | DrawMode.Gradation, fontColor, edgeColor, gradationTopColor, gradationBottomColor, edge_Ratio, false);

        public new SKBitmap DrawText_V(string drawstr, Color fontColor, Color edgeColor, int edge_Ratio)
            => DrawTextInternal(drawstr, DrawMode.Edge, fontColor, edgeColor, Color.White, Color.White, edge_Ratio, true);

        // --- コアロジック ---

        private SKBitmap DrawTextInternal(string drawstr, DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, int edge_Ratio, bool vertical)
        {
            if (string.IsNullOrEmpty(drawstr)) return new SKBitmap(1, 1);

            var key = new FontCacheKey
            {
                drawstr = drawstr,
                drawmode = drawmode,
                fontColor = fontColor,
                edgeColor = edgeColor,
                gradationTopColor = gradationTopColor,
                gradationBottomColor = gradationBottomColor,
                edge_Ratio = edge_Ratio,
                vertical = vertical
            };

            // 1. Dictionaryによる高速検索 (O(1))
            if (_cache.TryGetValue(key, out var cached))
            {
                cached.lastUsedTick = DateTime.Now.Ticks;
                // 既存の呼び出し側がDisposeすることを想定し、Copyを返す
                return cached.bmp.Copy();
            }

            // 2. キャッシュミス時の生成
            SKBitmap bmp = vertical 
                ? base.DrawText_V(drawstr, drawmode, fontColor, edgeColor, gradationTopColor, gradationBottomColor, edge_Ratio)
                : base.DrawText(drawstr, drawmode, fontColor, edgeColor, gradationTopColor, gradationBottomColor, edge_Ratio);

            // 3. キャッシュ容量の管理 (LRU: 最も古く使われたものを削除)
            if (_cache.Count >= nCacheSize)
            {
                FontCacheKey oldestKey = default;
                long oldestTick = long.MaxValue;
                foreach (var kvp in _cache)
                {
                    if (kvp.Value.lastUsedTick < oldestTick)
                    {
                        oldestTick = kvp.Value.lastUsedTick;
                        oldestKey = kvp.Key;
                    }
                }
                if (oldestTick != long.MaxValue)
                {
                    _cache[oldestKey].bmp.Dispose();
                    _cache.Remove(oldestKey);
                }
            }

            var newEntry = new FontCache { bmp = bmp, lastUsedTick = DateTime.Now.Ticks };
            _cache[key] = newEntry;

            return bmp.Copy();
        }

        public new void Dispose()
        {
            foreach (var item in _cache.Values)
            {
                item.bmp?.Dispose();
            }
            _cache.Clear();
            base.Dispose();
        }
    }
}