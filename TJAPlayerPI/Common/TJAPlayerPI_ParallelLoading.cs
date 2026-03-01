using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TJAPlayerPI
{
    internal partial class TJAPlayerPI
    {
        // 爆速化ポイント1: WAV読み込みの並列化
        // 元のコードでは1フレームに1〜3個ずつ読み込んでいたが、
        // Task.RunとParallel.ForEachを使用して全コアで一気に読み込む。
        private async Task LoadingChartParallel()
        {
            DateTime timeBeginLoadWAV = DateTime.Now;
            var dtx = TJAPlayerPI.DTX[0];

            if (dtx == null) return;

            // 読み込み対象のWAVを抽出
            var wavsToLoad = dtx.listWAV.Values
                .Where(w => w.bUse && w.rSound == null)
                .ToList();

            Trace.TraceInformation($"並列読み込み開始: {wavsToLoad.Count}個のWAV");

            // CPUコア数に合わせて並列実行
            await Task.Run(() =>
            {
                Parallel.ForEach(wavsToLoad, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, wav =>
                {
                    try
                    {
                        // tWAVの読み込み内部でBASSのストリーム生成などを行う
                        // BASSはスレッドセーフなので並列実行可能
                        dtx.tWAVの読み込み(wav);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"WAV読み込み失敗({wav.strFilename}): {e}");
                    }
                });
            });

            TimeSpan audioLoadingSpan = (TimeSpan)(DateTime.Now - timeBeginLoadWAV);
            Trace.TraceInformation("WAV読込所要時間({0,4}):     {1}", dtx.listWAV.Count, audioLoadingSpan.ToString());
        }
    }
}