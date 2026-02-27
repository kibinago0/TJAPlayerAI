using System;
using System.Collections.Generic;
using System.Drawing;
using FDK;

namespace TJAPlayerPI
{
    internal class CActHitResult : CActivity
    {
        // ヒットごとの状態を保持する構造体
        private class STHit
        {
            public bool b使用中;
            public CCounter ct爆発;
            public CCounter ct文字;
            public EJudge judge;
            public int nLane;
            public int nPlayer;
            public int nLag;
        }

        private STHit[] stヒット状態 = new STHit[128]; // 最大同時表示数

        public CActHitResult()
        {
            for (int i = 0; i < 128; i++)
            {
                stヒット状態[i] = new STHit();
                stヒット状態[i].ct爆発 = new CCounter();
                stヒット状態[i].ct文字 = new CCounter();
            }
        }

        public void Start(int nLane, EJudge judge, int lag, int player)
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stヒット状態[i].b使用中)
                {
                    stヒット状態[i].b使用中 = true;
                    stヒット状態[i].judge = judge;
                    stヒット状態[i].nLane = nLane;
                    stヒット状態[i].nPlayer = player;
                    stヒット状態[i].nLag = lag;
                    
                    // 各Activityの既存ロジックからタイマー初期化を移植
                    stヒット状態[i].ct爆発.t開始(0, 25, 25, TJAPlayerPI.app.Timer);
                    stヒット状態[i].ct文字.t開始(0, 300, 1, TJAPlayerPI.app.Timer);
                    break;
                }
            }
        }

        public override int On進行描画()
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stヒット状態[i].b使用中) continue;

                stヒット状態[i].ct爆発.t進行();
                stヒット状態[i].ct文字.t進行();

                // 1. 爆発エフェクトの描画 (既存のCAct演奏DrumsチップファイアDのロジック)
                t爆発描画(stヒット状態[i]);

                // 2. 判定文字の描画 (既存のCActJudgeStringのロジック)
                t判定文字描画(stヒット状態[i]);

                // 両方の演出が終わったら解放
                if (stヒット状態[i].ct爆発.b終了値に達した && stヒット状態[i].ct文字.b終了値に達した)
                {
                    stヒット状態[i].b使用中 = false;
                }
            }
            return 0;
        }

        private void t爆発描画(STHit hit)
        {
            if (TJAPlayerPI.app.Tx.Effects_Hit_Explosion == null) return;

            // 爆発アニメーションのコマ数を計算 (0〜3)
            int index = Math.Min(hit.ct爆発.n現在の値, 3);
            int width = 260;  // 画像1枚の幅 (仮定)
            int height = 260; // 画像1枚の高さ (仮定)
            
            // 描画位置の計算 (判定枠の中心)
            int nX = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[hit.nPlayer] - (width / 2);
            int nY = TJAPlayerPI.app.Skin.SkinConfig.Game.JudgePointY[hit.nPlayer] - (height / 2);
        
            // 判定（良・可・不可）に応じて、画像内のどの部分（Rectangle）を使うか切り替えて描画
            switch (hit.judge)
            {
                case EJudge.Perfect:
                case EJudge.AutoPerfect:
                    // 「良」の爆発
                    TJAPlayerPI.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayerPI.app.Device, nX, nY, new Rectangle(index * width, 0, width, height));
                    break;
                case EJudge.Good:
                    // 「可」の爆発
                    TJAPlayerPI.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayerPI.app.Device, nX, nY, new Rectangle(index * width, height, width, height));
                    break;
                // ...以下、不可などの処理
            }
        }
        private void t判定文字描画(STHit hit)
        {
            if (TJAPlayerPI.app.Tx.Judge == null) return;

            // 文字がふわっと浮き上がるアニメーションの計算
            int nY移動 = (int)(hit.ct文字.n現在の値 * 0.1); // 少しずつ上に移動
            int x = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[hit.nPlayer] - (90 / 2);
            int y = TJAPlayerPI.app.Skin.SkinConfig.Game.JudgePointY[hit.nPlayer] - (60 / 2) - nY移動;
        
            // 透明度の計算 (後半で消えていく)
            int alpha = 255;
            if (hit.ct文字.n現在の値 > 200)
            {
                alpha = 255 - (int)((hit.ct文字.n現在の値 - 200) * 2.55);
            }
            TJAPlayerPI.app.Tx.Judge.Opacity = alpha;
        
            // 「良」「可」などの文字を描画
            // st判定文字列[] は EJudge の値に対応した矩形範囲を持っていると想定
            TJAPlayerPI.app.Tx.Judge.t2D描画(TJAPlayerPI.app.Device, x, y, this.st判定文字列[(int)hit.judge]);
        }
    }
}