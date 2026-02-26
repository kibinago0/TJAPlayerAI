using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FDK;

namespace TJAPlayerPI
{
    internal class CAct演奏DrumsチップファイアD : CActivity
    {
        // プロパティ
        public struct ST状態
        {
            public CCounter ct進行;
            public EJudge judge;
            public int nPlayer;
            public int nIsBig;
        }
        public ST状態[] st状態 = new ST状態[128];
        public ST状態[] st状態_大 = new ST状態[128];

        // コンストラクタ
        public CAct演奏DrumsチップファイアD()
        {
            // base.b活性化してない = true; // 読み取り専用エラー回避のため削除
        }

        // メソッド
        // インデックスを外部から指定できるように変更
        public virtual void Start(int nLane, EJudge judge, int player, int index)
        {
            if (index < 0 || index >= 128) return;

            // 通常の爆発 (13フレーム)
            this.st状態[index].ct進行 = new CCounter(0, 13, 25, TJAPlayerPI.app.Timer);
            this.st状態[index].judge = judge;
            this.st状態[index].nPlayer = player;
            this.st状態[index].nIsBig = 0;

            // 大音符の爆発
            if (nLane == 0x13 || nLane == 0x14 || nLane == 0x1A || nLane == 0x1B)
            {
                this.st状態_大[index].ct進行 = new CCounter(0, 240, 1, TJAPlayerPI.app.Timer);
                this.st状態_大[index].judge = judge;
                this.st状態_大[index].nPlayer = player;
                this.st状態_大[index].nIsBig = 1;
            }
        }

        // 特定のインデックスのみを進行・描画するメソッド
        public void t進行描画(int i)
        {
            // 小音符エフェクト
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
                    int width = 130;
                    int height = 130;
                    int index = Math.Min(this.st状態[i].ct進行.n現在の値, 3);

                    if (TJAPlayerPI.app.Tx.Effects_Hit_Explosion != null)
                    {
                        Rectangle rect = new Rectangle(index * width, (int)this.st状態[i].judge * height, width, height);
                        TJAPlayerPI.app.Tx.Effects_Hit_Explosion.t2D描画(TJAPlayerPI.app.Device, nX, nY, rect);
                    }
                }
            }
        
            // 大音符エフェクト
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
                        TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big.vcScaling = new System.Numerics.Vector2(scale);
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


    // その他

    #region [ private ]
    //-----------------
    protected STSTATUS[] st状態 = new STSTATUS[3 * 4];
    protected STSTATUS_B[] st状態_大 = new STSTATUS_B[3 * 4];

    protected int[] nX座標 = new int[] { 450, 521, 596, 686, 778, 863, 970, 1070, 1150 };
    protected int[] nY座標 = new int[] { 172, 108, 50, 8, -10, -60, -5, 30, 90 };
    protected int[] nY座標P2 = new int[] { 172, 108, 50, 8, -10, -60, -5, 30, 90 };

    private int nOutLength = 2;

    [StructLayout(LayoutKind.Sequential)]
    protected struct STSTATUS
    {
        public bool b使用中;
        public CCounter ct進行;
        public EJudge judge;
        public int nIsBig;
        public int n透明度;
        public int nPlayer;
    }
    [StructLayout(LayoutKind.Sequential)]
    protected struct STSTATUS_B
    {
        public CCounter ct進行;
        public EJudge judge;
        public int nIsBig;
        public int n透明度;
        public int nPlayer;
    }

    private void DrawExpBig(int x, int y, float value, float scaleBegin, float scaleEnd, float length, EJudge eJudge, bool is2nd)
    {
        if (TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big is null) return;
        if (value > length) return;

        int width = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.HitExplosion.BigWidth;
        int height = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.HitExplosion.BigHeight;

        value /= length;

        float endLength = 0.5f;
        if (is2nd)
        {
            endLength = 0.0f;
        }
        else
        {
            value *= 1.0f + endLength;
        }

            float scaleValue = MathF.Min(value, 1.0f);
        scaleValue = MathF.Sin(scaleValue * MathF.PI * 0.5f);
        float scale = float.Lerp(scaleBegin, scaleEnd, scaleValue);

        float opacityValue = 0.0f;
        if (is2nd)
        {
            opacityValue = 1.0f - MathF.Cos(value * MathF.PI * 0.5f);
        }
        else
        {
            opacityValue = MathF.Max(value - 1.0f, 0.0f) / endLength;
        }
        float opacity = 1.0f - opacityValue;

        TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big.Opacity = (int)(opacity * 255);
        TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big.vcScaling = new Vector2(scale);

        switch (eJudge)
        {
            case EJudge.Perfect:
            case EJudge.AutoPerfect:
                {
                    TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big.t2D拡大率考慮描画(TJAPlayerPI.app.Device, CTexture.RefPnt.Center, x, y, new Rectangle(width, 0, width, height));
                }
                break;

            case EJudge.Good:
                {
                    TJAPlayerPI.app.Tx.Effects_Hit_Explosion_Big.t2D拡大率考慮描画(TJAPlayerPI.app.Device, CTexture.RefPnt.Center, x, y, new Rectangle(0, 0, width, height));
                }
                break;

            case EJudge.Miss:
            case EJudge.Bad:
                break;
        }
    }

    //-----------------
    #endregion
}
