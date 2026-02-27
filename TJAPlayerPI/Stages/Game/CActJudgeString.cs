using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using FDK;

namespace TJAPlayerPI;

internal class CActJudgeString : CActivity
{
    // プロパティ

    private STSTATUS[] st状態 = new STSTATUS[12];
    [StructLayout(LayoutKind.Sequential)]
    private struct STSTATUS
    {
        public bool b使用中;
        public CCounter ct進行;
        public EJudge judge;
        public int n相対X座標;
        public int n相対Y座標;
        public int n透明度;
        public int nPlayer;
    }

    private readonly Rectangle[] st判定文字列;
    private List<int> n表示順 = new List<int>(); // 追加：描画の優先順位を管理するリスト

    // コンストラクタ

    public CActJudgeString()
    {
        this.st判定文字列 = new Rectangle[] {
            new Rectangle( 0, 0,    90, 60 ),		// Perfect
            new Rectangle( 0, 60,   90, 60 ),		// Good
            new Rectangle( 0, 120,   90, 60 ),		// Bad
            new Rectangle( 0, 120,   90, 60 ),		// Miss
            new Rectangle( 0, 0,    90, 60 )		// Auto
        };
    }


    // メソッド

    public void Start(EJudge judge, int lag, CDTX.CChip pChip, int player)
    {
        // 校正中（Calibration）は判定文字を表示しない
        if (TJAPlayerPI.IsPerformingCalibration)
        {
            return;
        }

        // 連打系チップの場合は判定文字を表示しない
        if (pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x19)
        {
            return;
        }

        for (int j = 0; j < 12; j++)
        {
            if (this.st状態[j].b使用中 == false)
            {
                this.st状態[j].ct進行 = new CCounter(0, 300, 1, TJAPlayerPI.app.Timer);
                this.st状態[j].b使用中 = true;
                this.st状態[j].judge = judge;
                this.st状態[j].n相対X座標 = 0;
                this.st状態[j].n相対Y座標 = 0;
                this.st状態[j].n透明度 = 0xff;
                this.st状態[j].nPlayer = player;

                // ★新しく発生した判定のインデックスをリストの最後に追加（＝最後に描画される＝一番手前）
                this.n表示順.Add(j);
                break;
            }
        }
    }


    // CActivity 実装

    public override void On活性化()
    {
        for (int i = 0; i < 12; i++)
        {
            this.st状態[i].ct進行 = new CCounter();
            this.st状態[i].b使用中 = false;
        }
        this.n表示順.Clear();
        base.On活性化();
    }

    public override void On非活性化()
    {
        for (int i = 0; i < 12; i++)
        {
            this.st状態[i].ct進行 = null;
        }
        this.n表示順.Clear();
        base.On非活性化();
    }

    public int t進行描画()
    {
        // 1. 進行処理（全スロットの状態を更新）
        for (int i = 0; i < 12; i++)
        {
            if (this.st状態[i].b使用中)
            {
                this.st状態[i].ct進行.t進行();
                if (this.st状態[i].ct進行.b終了値に達した)
                {
                    this.st状態[i].ct進行.t停止();
                    this.st状態[i].b使用中 = false;
                }
                else
                {
                    int num2 = this.st状態[i].ct進行.n現在の値;

                    // 移動アニメーション（落下）
                    float fallValue = CConvert.InverseLerpClamp(0, 100, num2);
                    this.st状態[i].n相対Y座標 = (int)(fallValue * 13);

                    // フェードアウトアニメーション（250ms以降）
                    float opacityValue = CConvert.InverseLerpClamp(250, 300, num2);
                    float opacity = MathF.Cos(opacityValue * MathF.PI * 0.5f);
                    this.st状態[i].n透明度 = (int)(opacity * 255);
                }
            }
        }

        // 2. 描画処理（発生順リストに基づいて描画することで、重なり順を制御）
        for (int k = 0; k < this.n表示順.Count; k++)
        {
            int j = this.n表示順[k];

            if (this.st状態[j].b使用中)
            {
                if (TJAPlayerPI.app.Tx.Judge is not null)
                {
                    int baseY = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldY[this.st状態[j].nPlayer] - 53;
                    int x = TJAPlayerPI.app.Skin.SkinConfig.Game.ScrollFieldX[this.st状態[j].nPlayer] - TJAPlayerPI.app.Tx.Judge.szTextureSize.Width / 2;
                    x += this.st状態[j].n相対X座標;
                    int y = (baseY + this.st状態[j].n相対Y座標);

                    TJAPlayerPI.app.Tx.Judge.Opacity = this.st状態[j].n透明度;
                    TJAPlayerPI.app.Tx.Judge.t2D描画(TJAPlayerPI.app.Device, x, y, this.st判定文字列[(int)this.st状態[j].judge]);
                }
            }
            else
            {
                // 使用が終わったものはリストから削除
                this.n表示順.RemoveAt(k);
                k--;
            }
        }

        return 0;
    }
}