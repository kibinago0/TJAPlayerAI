using System;
using System.Collections.Generic;
using FDK;

namespace TJAPlayerPI
{
    internal partial class CStage演奏画面共通 : CStage
    {
        // 爆速化・同期化ポイント: Balloonプロセスの初期化ロジック
        
        protected bool tBalloonProcess_Optimized(CDTX.CChip pChip, int nPlayer)
        {
            // ... (既存のコンボチェック等のロジック)
            
            // 1. Breakingアニメーションの開始
            // 1小節分の長さで再生するため、カウンターは十分な長さ（例: 10秒）を持たせ、
            // 実際の描画時にBPMに合わせてフレームを計算する。
            // インターバルは1msにして経過時間をミリ秒で取得できるようにする。
            actChara.CharaAction_Balloon_Breaking[nPlayer] = new CCounter(0, 10000, 1, TJAPlayerPI.app.Timer);
            
            // 2. Broke/Missアニメーションの開始 (333.33ms固定)
            // こちらもインターバル1msで、333msまでカウントする。
            // 描画側で333.33msを超えたら停止する処理を入れている。
            
            // 例: 風船が割れた時
            if (this.n風船残り[nPlayer] <= 0)
            {
                actChara.CharaAction_Balloon_Broke[nPlayer] = new CCounter(0, 1000, 1, TJAPlayerPI.app.Timer);
                // ...
            }
            
            // 例: ミスした時
            // actChara.CharaAction_Balloon_Miss[nPlayer] = new CCounter(0, 1000, 1, TJAPlayerPI.app.Timer);
            
            return true;
        }
    }
}
