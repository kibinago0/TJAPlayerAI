using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TJAPlayerPI
{
    internal partial class CDTX
    {
        // 爆速化ポイント4: 正規表現の排除とSpan<char>の活用
        // 大量の譜面行を処理する際、Regexは非常に遅い。
        // Spanを使用してメモリ割り当てを抑えつつ高速にパースする。
        private void t入力_行解析譜面_Optimized(string InputText)
        {
            if (string.IsNullOrEmpty(InputText)) return;

            // 修正前: Regex.Match(InputText, pattern) 
            
            // 修正後: Spanによる直接スキャン
            ReadOnlySpan<char> span = InputText.AsSpan();
            
            if (span.StartsWith("#"))
            {
                // コマンド処理
                int spaceIndex = span.IndexOf(' ');
                ReadOnlySpan<char> command = spaceIndex == -1 ? span : span.Slice(0, spaceIndex);
                ReadOnlySpan<char> argument = spaceIndex == -1 ? ReadOnlySpan<char>.Empty : span.Slice(spaceIndex + 1);
                
                // command.Equals("#BPM", StringComparison.OrdinalIgnoreCase) 等で分岐
            }
            else
            {
                // 譜面データ処理 (10201020, 等)
                // カンマの位置を高速に検索
                int commaIndex = span.IndexOf(',');
                if (commaIndex != -1)
                {
                    // 小節の区切り処理
                }
            }
        }
    }
}