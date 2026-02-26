using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Numerics;
using FDK;

namespace TJAPlayerPI
{
    internal class CAct演奏DrumsチップファイアD : CActivity
    {
        public struct ST状態
        {
            public CCounter ct進行;
            public EJudge judge;
            public int nPlayer;
        }
        public ST状態[] st状態 = new ST状態[128];
        public ST状態[] st状態_大 = new ST状態[128];

        public CAct演奏DrumsチップファイアD()
        {
        }

        public virtual void Start(int nLane, EJudge judge, int player, int index)
        {
            if (index < 0 || index >= 128) return;

            this.st状態[index].ct進行 = new CCounter(0, 13, 25, TJAPlayerPI.app.Timer);
            this.st状態[index].judge = judge;
            this.st状態[index].nPlayer = player;

            if (nLane == 0x13 || nLane == 0x14 || nLane == 0x1A || nLane == 0x1B)
            {
                this.st状態_大[index].ct進行 = new CCounter(0, 240, 1, TJAPlayerPI.app.Timer);
                this.st状態_大[index].judge = judge;
                this.st状態_大[index].nPlayer = player;
            }
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
                    int nX = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[this.st状態[i].nPlayer] - 65;
                    int nY = TJAPlayerPI.app.Skin.SkinConfig.Game.JudgePointY[this.st状態[i].nPlayer] - 65;
                    int index = Math.Min(this.st状態[i].ct進行.n現在の値, 3);

                    if (TJAPlayerPI.app.Tx.Effects_Hit_Explosion != null)
                    {
                        Rectangle rect = new Rectangle(index * 130, (int)this.st状態[i].judge * 130, 130, 130);
                        TJAPlayerPI.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayerPI.app.Device, nX, nY, rect);
                    }
                }
            }

            if (this.st状態_大[i].ct進行 != null && !this.st状態_大[i].ct進行.b停止中)
            {
                this.st状態_大[i].ct進行.t進行();
                if (this.st状態_大[i].ct進行.b終了値に達した)
                {
                    this.st状態_大[i].ct進行.t停止();
                }
                else
                {
                    int nX = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[this.st状態_大[i].nPlayer];
                    int nY = TJAPlayerPI.app.Skin.SkinConfig.Game.JudgePointY[this.st状態_大[i].nPlayer];
                    float value = (float)this.st状態_大[i].ct進行.n現在の値 / 240f;
                    
                    if (TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big != null)
                    {
                        float scale = 1.0f + (float)Math.Sin(value * Math.PI * 0.5f) * 0.5f;
                        TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big.vcScaling = new Vector2(scale);
                        TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big.t2D拡大率考慮描画(TJAPlayerPI.app.Device, CTexture.RefPnt.Center, nX, nY);
                    }
                }
            }
        }

        public override void On活性化()
        {
            for (int i = 0; i < 128; i++)
            {
                this.st状態[i].ct進行 = new CCounter();
                this.st状態_大[i].ct進行 = new CCounter();
            }
            base.On活性化();
        }

        public override int On進行描画()
        {
            return 0;
        }
    }
}