using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace TJAPlayerPI
{
    internal partial class CSongsManager
    {
        // 爆速化ポイント3: 曲リスト構築の最適化
        // new CDTX() を呼び出す代わりに FastParseHeader を使用する。
        private void t曲を検索してリストを作成する_Optimized(string str基点フォルダ, bool b子BOXへ再帰する, List<C曲リストノード> listノードリスト, C曲リストノード node親)
        {
            DirectoryInfo info = new DirectoryInfo(str基点フォルダ);
            
            foreach (FileInfo fileinfo in info.GetFiles())
            {
                string strExt = fileinfo.Extension.ToLowerInvariant();
                if (strExt.Equals(".tja") || strExt.Equals(".tcm") || strExt.Equals(".tci"))
                {
                    // 修正前: CDTX dtx = new CDTX(path, false, 0, 0, false); // 全パース
                    
                    // 修正後: ヘッダのみ取得
                    CDTX.FastParseHeader(fileinfo.FullName, out string title, out string genre, out double bpm);
                    
                    C曲リストノード c曲リストノード = new C曲リストノード();
                    c曲リストノード.strTitle = title;
                    c曲リストノード.strGenre = genre;
                    // ... その他のノード構築処理 ...
                    
                    listノードリスト.Add(c曲リストノード);
                }
            }
            
            // ディレクトリ再帰処理...
        }
    }
}