using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FDK;

namespace TJAPlayerPI
{
    internal class CActJudgeString : CActivity
    {
        public struct ST状態
        {
            public CCounter ct進行;
            public EJudge judge;
            public int nPlayer;
        }
        public ST状態[] st状態 = new ST状態[128];

        private readonly Rectangle[] st判定文字列 = new Rectangle[]
        {
            new Rectangle( 0, 0,    90, 60 ),		// Perfect
            new Rectangle( 0, 60,   90, 60 ),		// Good
            new Rectangle( 0, 120,   90, 60 ),		// Bad
            new Rectangle( 0, 120,   90, 60 ),		// Miss
            new Rectangle( 0, 0,    90, 60 )		// Auto
        };

        public CActJudgeString()
        {
        }

        // 引数を (EJudge judge, int lag, CDTX.CChip pChip, int player, int index = 0) に修正
        public void Start(EJudge judge, int lag, CDTX.CChip pChip, int player, int index = 0)
        {
            if (index < 0 || index >= 128) return;

            this.st状態[index].ct進行 = new CCounter(0, 300, 1, TJAPlayerPI.app.Timer);
            this.st状態[index].judge = judge;
            this.st状態[index].nPlayer = player;
        }

        public void t進行描画(int i)
        {
            if (this.st状態[i].ct進行 != null && !this.st状態[i].ct進行.b停止中)
            {
                this.st状態[i].ct進行.t進行();
                if (this.st状態[i].ct進行.b終了値に達した)
                {
                    this.st状態[i].ct進行.t停止();
                }
                else
                {
                    int nX = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[this.st状態[i].nPlayer] - 45;
                    int nY = TJAPlayerPI.app.Skin.SkinConfig.Game.JudgePointY[this.st状態[i].nPlayer] - 30;
                    
                    float value = (float)this.st状態[i].ct進行.n現在の値 / 300f;
                    int offset = (int)(Math.Sin(value * Math.PI * 2.0f) * 15.0f * (1.0f - value));
                    
                    if (TJAPlayerPI.app.Tx.Judge != null)
                    {
                        TJAPlayerPI.app.Tx.Judge.t2D描画(TJAPlayerPI.app.Device, nX, nY - offset, this.st判定文字列[(int)this.st状態[i].judge]);
                    }
                }
            }
        }

        public override void On活性化()
        {
            for (int i = 0; i < 128; i++)
            {
                this.st状態[i].ct進行 = new CCounter();
            }
            base.On活性化();
        }

        public override int On進行描画()
        {
            for (int i = 0; i < 128; i++) t進行描画(i);
            return 0;
        }
    }
}