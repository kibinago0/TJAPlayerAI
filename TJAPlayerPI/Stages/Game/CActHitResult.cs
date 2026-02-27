using System;
using System.Collections.Generic;
using System.Drawing;
using FDK;

namespace TJAPlayerPI
{
    internal class CActHitResult : CActivity
    {
        private class STHit
        {
            public bool b使用中;
            public CCounter ct爆発;
            public CCounter ct文字;
            public EJudge judge;
            public int nPlayer;
            public bool bIsBig; // 大音符判定
        }

        private STHit[] stヒット状態 = new STHit[128];

        private readonly Rectangle[] st判定文字列 = new Rectangle[] {
            new Rectangle( 0, 0,    90, 60 ),   // Perfect
            new Rectangle( 0, 60,   90, 60 ),   // Good
            new Rectangle( 0, 120,  90, 60 ),   // Bad
            new Rectangle( 0, 120,  90, 60 ),   // Miss
            new Rectangle( 0, 0,    90, 60 )    // Auto
        };

        public CActHitResult()
        {
            for (int i = 0; i < 128; i++)
            {
                stヒット状態[i] = new STHit();
                stヒット状態[i].ct爆発 = new CCounter();
                stヒット状態[i].ct文字 = new CCounter();
            }
        }

        public void Start(int nChannel, EJudge judge, int lag, int player)
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stヒット状態[i].b使用中)
                {
                    stヒット状態[i].b使用中 = true;
                    stヒット状態[i].judge = judge;
                    stヒット状態[i].nPlayer = player;
                    stヒット状態[i].bIsBig = (nChannel == 0x13 || nChannel == 0x14); // 大音符チャンネル判定

                    // 元のタイマー設定を復元
                    // 爆発: 0〜14 (25ms間隔)
                    stヒット状態[i].ct爆発.t開始(0, 14, 25, TJAPlayerPI.app.Timer);
                    // 文字: 0〜300 (1ms間隔)
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

                // 重なり順を維持しつつ、元の動きで描画
                t爆発描画(stヒット状態[i]);
                t判定文字描画(stヒット状態[i]);

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

            // 元の連番アニメーションロジックを復元
            int index = Math.Min(hit.ct爆発.n現在の値, 3); // 0,1,2,3で固定
            int width = 260; 
            int height = 260;
            int nX = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[hit.nPlayer] - (width / 2);
            int nY = TJAPlayerPI.app.Skin.SkinConfig.Game.JudgePointY[hit.nPlayer] - (height / 2);

            // 判定ごとの画像縦位置
            int yOffset = 0;
            if (hit.judge == EJudge.Good) yOffset = height;
            else if (hit.judge == EJudge.Bad || hit.judge == EJudge.Miss) yOffset = height * 2;

            // 描画
            TJAPlayerPI.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayerPI.app.Device, nX, nY, new Rectangle(index * width, yOffset, width, height));
            
            // 大音符時の追加エフェクトが必要な場合はここに追記
        }

        private void t判定文字描画(STHit hit)
        {
            if (TJAPlayerPI.app.Tx.Judge == null) return;

            // 元の「段階的な移動」ロジックを復元
            int n相対Y = 15;
            int v = hit.ct文字.n現在の値;
            if (v < 20) n相対Y = 0;
            else if (v < 40) n相対Y = 5;
            else if (v < 60) n相対Y = 10;

            int x = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[hit.nPlayer] - (90 / 2);
            int y = TJAPlayerPI.app.Skin.SkinConfig.Game.JudgePointY[hit.nPlayer] - (60 / 2) + n相対Y;

            // 元のフェードアウトロジック (250ms以降で消える)
            int alpha = 255;
            if (v > 250)
            {
                float opacityValue = (v - 250) / 50.0f;
                alpha = (int)(MathF.Cos(opacityValue * MathF.PI * 0.5f) * 255);
            }
            TJAPlayerPI.app.Tx.Judge.Opacity = alpha;

            TJAPlayerPI.app.Tx.Judge.t2D描画(TJAPlayerPI.app.Device, x, y, st判定文字列[(int)hit.judge]);
        }
    }
}
