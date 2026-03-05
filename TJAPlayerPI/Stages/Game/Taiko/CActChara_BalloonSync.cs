using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using FDK;

namespace TJAPlayerPI
{
    internal partial class CActChara : CActivity
    {
        // 爆速化・同期化ポイント: BalloonアニメーションのBPM同期と固定時間処理
        
        public void OnDraw_Balloon(int nPlayer)
        {
            // 1. Balloon_Breaking (BPM同期: 1小節分の長さ)
            if (CharaAction_Balloon_Breaking[nPlayer]?.b進行中 == true)
            {
                CharaAction_Balloon_Breaking[nPlayer].t進行();
                
                if (CharaAction_Balloon_Breaking[nPlayer].b終了値に達した)
                {
                    CharaAction_Balloon_Breaking[nPlayer].t停止();
                }
                else
                {
                    int ptn = TJAPlayerPI.app.Skin.Game_Chara_Ptn_Balloon_Breaking[nPlayer];
                    if (ptn > 0)
                    {
                        // 現在のBPMから1小節の時間を計算
                        double bpm = stage演奏ドラム画面.actPlayInfo.dbBPM[nPlayer];
                        double measureTimeMs = (60000.0 / bpm) * 4.0;
                        
                        // 経過時間から現在のフレームを計算
                        // CharaAction_Balloon_Breakingは開始からの経過時間を保持するように初期化されている前提
                        long elapsed = CharaAction_Balloon_Breaking[nPlayer].n現在の値;
                        int frame = (int)(elapsed * ptn / measureTimeMs);
                        frame = Math.Clamp(frame, 0, ptn - 1);
                        
                        TJAPlayerPI.app.Tx.Chara_Balloon_Breaking[nPlayer][frame]?.t2D描画(
                            TJAPlayerPI.app.Device, 
                            (TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - stage演奏ドラム画面.actLaneTaiko.nDefaultJudgePos[nPlayer, 0]) + TJAPlayerPI.app.Skin.SkinConfig.Game.Chara.BalloonX[nPlayer], 
                            TJAPlayerPI.app.Skin.SkinConfig.Game.Chara.BalloonY[nPlayer]
                        );
                    }
                }
            }

            // 2. Balloon_Broke (再生速度: 1小節 / 表示時間: 333.33ms固定)
            if (CharaAction_Balloon_Broke[nPlayer]?.b進行中 == true)
            {
                CharaAction_Balloon_Broke[nPlayer].t進行();
                
                const double DisplayDurationMs = 333.33;

                if (CharaAction_Balloon_Broke[nPlayer].n現在の値 >= DisplayDurationMs)
                {
                    CharaAction_Balloon_Broke[nPlayer].t停止();
                }
                else
                {
                    int ptn = TJAPlayerPI.app.Skin.Game_Chara_Ptn_Balloon_Broke[nPlayer];
                    if (ptn > 0)
                    {
                        double bpm = stage演奏ドラム画面.actPlayInfo.dbBPM[nPlayer];
                        double measureTimeMs = (60000.0 / bpm) * 4.0;
                        long elapsed = CharaAction_Balloon_Broke[nPlayer].n現在の値;

                        // 再生速度を1小節に同期
                        int frame = (int)(elapsed * ptn / measureTimeMs);
                        // 1小節より速い場合は最後のフレームで待機、遅い場合は途中で消える
                        frame = Math.Min(frame, ptn - 1);
                        
                        TJAPlayerPI.app.Tx.Chara_Balloon_Broke[nPlayer][frame]?.t2D描画(
                            TJAPlayerPI.app.Device, 
                            (TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - stage演奏ドラム画面.actLaneTaiko.nDefaultJudgePos[nPlayer, 0]) + TJAPlayerPI.app.Skin.SkinConfig.Game.Chara.BalloonX[nPlayer], 
                            TJAPlayerPI.app.Skin.SkinConfig.Game.Chara.BalloonY[nPlayer]
                        );
                    }
                }
            }

            // 3. Balloon_Miss (再生速度: 1小節 / 表示時間: 333.33ms固定)
            if (CharaAction_Balloon_Miss[nPlayer]?.b進行中 == true)
            {
                CharaAction_Balloon_Miss[nPlayer].t進行();
                
                const double DisplayDurationMs = 333.33;

                if (CharaAction_Balloon_Miss[nPlayer].n現在の値 >= DisplayDurationMs)
                {
                    CharaAction_Balloon_Miss[nPlayer].t停止();
                }
                else
                {
                    int ptn = TJAPlayerPI.app.Skin.Game_Chara_Ptn_Balloon_Miss[nPlayer];
                    if (ptn > 0)
                    {
                        double bpm = stage演奏ドラム画面.actPlayInfo.dbBPM[nPlayer];
                        double measureTimeMs = (60000.0 / bpm) * 4.0;
                        long elapsed = CharaAction_Balloon_Miss[nPlayer].n現在の値;

                        // 再生速度を1小節に同期
                        int frame = (int)(elapsed * ptn / measureTimeMs);
                        // 1小節より速い場合は最後のフレームで待機、遅い場合は途中で消える
                        frame = Math.Min(frame, ptn - 1);
                        
                        TJAPlayerPI.app.Tx.Chara_Balloon_Miss[nPlayer][frame]?.t2D描画(
                            TJAPlayerPI.app.Device, 
                            (TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[nPlayer] - stage演奏ドラム画面.actLaneTaiko.nDefaultJudgePos[nPlayer, 0]) + TJAPlayerPI.app.Skin.SkinConfig.Game.Chara.BalloonX[nPlayer], 
                            TJAPlayerPI.app.Skin.SkinConfig.Game.Chara.BalloonY[nPlayer]
                        );
                    }
                }
            }
        }
    }
}
