using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TJAPlayerPI
{
    internal partial class CDTX
    {
        // 爆速化ポイント2: ヘッダのみの超高速パース
        // 曲リストの列挙時に、譜面データ(#START以降)を無視することで
        // パース時間を99%削減する。
        public static void FastParseHeader(string strFilename, out string title, out string genre, out double bpm)
        {
            title = "";
            genre = "";
            bpm = 120.0;

            if (!File.Exists(strFilename)) return;

            // ReadJEnc等を使用してエンコーディングを判定して読み込む
            string content = FDK.CJudgeTextEncoding.ReadTextFile(strFilename) ?? "";
            
            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

                    // #STARTが来たら解析終了
                    if (line.StartsWith("#START", StringComparison.OrdinalIgnoreCase)) break;

                    int colonIndex = line.IndexOf(':');
                    if (colonIndex == -1) continue;

                    string key = line.Substring(0, colonIndex).Trim().ToUpperInvariant();
                    string val = line.Substring(colonIndex + 1).Trim();

                    switch (key)
                    {
                        case "TITLE": title = val; break;
                        case "GENRE": genre = val; break;
                        case "BPM": double.TryParse(val, out bpm); break;
                    }
                }
            }
        }
    }
}