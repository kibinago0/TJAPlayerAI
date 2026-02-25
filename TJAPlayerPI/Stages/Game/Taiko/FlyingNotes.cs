using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Numerics;
using FDK;

namespace TJAPlayerPI
{
    /// <summary>
    /// 飛んでいく音符（FlyingNotes）の演出を管理するクラス。
    /// 1レンダリングフレームごとに1ステップ進む「フレームベース」の移動を実現します。
    /// </summary>
    internal class FlyingNotes : CActivity
    {
        // プロパティ (他のクラスから参照されるため、パブリックで保持)
        public float[] StartPointX = new float[2];
        public float[] StartPointY = new float[2];
        public float[] EndPointX = new float[2];
        public float[] EndPointY = new float[2];

        // コンストラクタ
        public FlyingNotes(CActChipEffects chipEffects, FireWorks fireWorks)
        {
            this.actChipEffects = chipEffects;
            this.FireWorks = fireWorks;
        }

        // メソッド

        /// <summary>
        /// 飛んでいく音符の演出を開始します。
        /// </summary>
        /// <param name="nLane">レーン番号</param>
        /// <param name="nPlayer">プレイヤー番号(0 or 1)</param>
        /// <param name="isRoll">連打中かどうか</param>
        public virtual void Start(int nLane, int nPlayer, bool isRoll = false)
        {
            for (int i = 0; i < 128; i++)
            {
                if (!Flying[i].IsUsing)
                {
                    Flying[i].IsUsing = true;
                    Flying[i].Lane = nLane;
                    Flying[i].Player = nPlayer;
                    Flying[i].isRoll = isRoll;

                    // フレームベースモードの初期化
                    // スキンから配列のフレーム数を取得
                    int arrayFrames = 0;
                    if (TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[nPlayer] != null)
                    {
                        arrayFrames = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[nPlayer].Length;
                    }

                    // 全体のステップ数 = StartPoint(1) + Array(arrayFrames) + EndPoint(1)
                    int totalSteps = arrayFrames + 2;

                    // CCounterを使用するが、t進行()は呼ばず、手動でn現在の値をインクリメントする。
                    // 第3引数の間隔msは、手動更新のため0に設定。
                    Flying[i].Counter = new CCounter(0, totalSteps - 1, 0, TJAPlayerPI.app.Timer);
                    Flying[i].Counter.n現在の値 = 0;
                    
                    // 個別ノーツの状態として座標を保持
                    Flying[i].StartPointX = this.StartPointX[nPlayer];
                    Flying[i].StartPointY = this.StartPointY[nPlayer];
                    Flying[i].EndPointX = this.EndPointX[nPlayer];
                    Flying[i].EndPointY = this.EndPointY[nPlayer];

                    break;
                }
            }
        }

        // CActivity 実装

        public override void On活性化()
        {
            for (int i = 0; i < 128; i++)
            {
                Flying[i] = new Status();
                Flying[i].IsUsing = false;
                Flying[i].Counter = new CCounter();
            }

            // スキン設定から座標を読み込み、クラスのフィールドにキャッシュ
            for (int i = 0; i < 2; i++)
            {
                this.StartPointX[i] = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointX[i];
                this.StartPointY[i] = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[i];
                this.EndPointX[i] = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[i];
                this.EndPointY[i] = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[i];
            }

            base.On活性化();
        }

        public override void On非活性化()
        {
            for (int i = 0; i < 128; i++)
            {
                Flying[i].IsUsing = false;
                Flying[i].Counter = null;
            }
            base.On非活性化();
        }

        /// <summary>
        /// 進行と描画を行います。
        /// </summary>
        public override int On進行描画()
        {
            if (this.b活性化してない)
                return 0;

            for (int i = 0; i < 128; i++)
            {
                if (Flying[i].IsUsing)
                {
                    int nPlayer = Flying[i].Player;
                    int nStep = Flying[i].Counter.n現在の値;

                    float[] frameX = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[nPlayer];
                    float[] frameY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsY[nPlayer];
                    int arrayLen = (frameX != null) ? frameX.Length : 0;

                    // 描画座標の決定
                    // 順序: StartPoint (Step 0) -> Array (Step 1 to arrayLen) -> EndPoint (Step arrayLen + 1)
                    float x, y;
                    if (nStep == 0)
                    {
                        x = Flying[i].StartPointX;
                        y = Flying[i].StartPointY;
                    }
                    else if (nStep <= arrayLen)
                    {
                        int arrayIdx = nStep - 1;
                        x = frameX[arrayIdx];
                        y = (frameY != null && arrayIdx < frameY.Length) ? frameY[arrayIdx] : Flying[i].EndPointY;
                    }
                    else
                    {
                        x = Flying[i].EndPointX;
                        y = Flying[i].EndPointY;
                    }

                    Flying[i].X = x;
                    Flying[i].Y = y;

                    // 飛行中の火花エフェクト（巨大ノーツのみ発生）
                    // Lane 3, 4 は大音符、6 は大連打
                    if (Flying[i].Lane == 3 || Flying[i].Lane == 4 || Flying[i].Lane == 6)
                    {
                        if (nStep % 4 == 0)
                        {
                            FireWorks.Start(Flying[i].Lane, Flying[i].Player, x, y);
                        }
                    }

                    // ノーツの描画
                    if (TJAPlayerPI.app.Tx.Notes != null)
                    {
                        // レーン番号(0-7)に応じてテクスチャ内の矩形を選択
                        Rectangle rect = new Rectangle(Flying[i].Lane * 130, 0, 130, 130);
                        TJAPlayerPI.app.Tx.Notes.t2D拡大率考慮描画(TJAPlayerPI.app.Device, CTexture.RefPnt.Center, x, y, rect);
                    }

                    // 【重要】FPSに関係なく、描画1回（1フレーム）につきカウントを1進める
                    if (!Flying[i].Counter.b停止中)
                    {
                        Flying[i].Counter.n現在の値++;

                        // 終了判定（全ステップ終了したか）
                        if (Flying[i].Counter.b終了値に達した)
                        {
                            // 到着時の爆発エフェクト（チップエフェクト）開始
                            actChipEffects.Start(Flying[i].Player, Flying[i].Lane);
                            
                            Flying[i].Counter.t停止();
                            Flying[i].IsUsing = false;
                        }
                    }
                }
            }
            return 0;
        }

        //-----------------
        #region [ private ]
        private struct Status
        {
            public int Lane;
            public int Player;
            public bool IsUsing;
            public CCounter Counter;
            public float X;
            public float Y;
            public float Height;
            public float Width;
            public double IncreaseX;
            public double IncreaseY;
            public bool isRoll;
            public float StartPointX;
            public float StartPointY;
            public float EndPointX;
            public float EndPointY;
        }

        private Status[] Flying = new Status[128];
        private CActChipEffects actChipEffects;
        private FireWorks FireWorks;
        #endregion
    }
}