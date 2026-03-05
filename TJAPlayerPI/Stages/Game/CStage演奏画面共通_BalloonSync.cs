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
            // 毎回新しく生成し、明示的に開始(t開始)させることで、2回目以降も確実に再生されるようにする。
            actChara.CharaAction_Balloon_Breaking[nPlayer] = new CCounter(0, 10000, 1, TJAPlayerPI.app.Timer);
            actChara.CharaAction_Balloon_Breaking[nPlayer].t開始();
            
            // 2. Broke/Missアニメーションの開始 (1小節分の長さ)
            
            // 例: 風船が割れた時
            if (this.n風船残り[nPlayer] <= 0)
            {
                // 前のアニメーションが残っている可能性を考慮して停止してから新しく生成
                actChara.CharaAction_Balloon_Broke[nPlayer]?.t停止();
                actChara.CharaAction_Balloon_Broke[nPlayer] = new CCounter(0, 10000, 1, TJAPlayerPI.app.Timer);
                actChara.CharaAction_Balloon_Broke[nPlayer].t開始();
                
                // 成功時はBreakingを止める
                actChara.CharaAction_Balloon_Breaking[nPlayer]?.t停止();
            }
            
            // 例: ミスした時 (風船チップが通り過ぎた時など)
            // if (ミス判定) {
            //     actChara.CharaAction_Balloon_Miss[nPlayer]?.t停止();
            //     actChara.CharaAction_Balloon_Miss[nPlayer] = new CCounter(0, 10000, 1, TJAPlayerPI.app.Timer);
            //     actChara.CharaAction_Balloon_Miss[nPlayer].t開始();
            //     actChara.CharaAction_Balloon_Breaking[nPlayer]?.t停止();
            // }
            
            return true;
        }
    }
}
