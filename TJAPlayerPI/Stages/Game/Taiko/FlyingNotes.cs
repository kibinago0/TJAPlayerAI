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
        // コンストラクタ
        public FlyingNotes(CActChipEffects chipEffects, FireWorks fireWorks)
        {
            this.actChipEffects = chipEffects;
            this.FireWorks = fireWorks;
            base.b活性化してない = true;
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
                    // スキンからフレーム数を取得（デフォルトは60フレーム）
                    int maxFrames = 60;
                    if (TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[nPlayer] != null)
                    {
                        maxFrames = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[nPlayer].Length;
                    }

                    // CCounterを使用するが、t進行()は呼ばず、手動でn現在の値をインクリメントする。
                    // 第3引数の間隔msは、手動更新のため0に設定。
                    Flying[i].Counter = new CCounter(0, maxFrames - 1, 0, TJAPlayerPI.app.Timer);
                    Flying[i].Counter.n現在の値 = 0;
                    
                    // スキンの基本座標を保持（配列外参照時のフォールバック用）
                    Flying[i].StartPointX = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointX[nPlayer];
                    Flying[i].StartPointY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer];
                    Flying[i].EndPointX = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[nPlayer];
                    Flying[i].EndPointY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[nPlayer];

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
            if (base.b活性化してない)
                return 0;

            for (int i = 0; i < 128; i++)
            {
                if (Flying[i].IsUsing)
                {
                    int nPlayer = Flying[i].Player;
                    int nFrame = Flying[i].Counter.n現在の値;

                    // 指定された座標データ配列から現在フレームの座標を取得
                    // 配列が存在しない、またはインデックスが範囲外の場合はEndPointを使用
                    float x = Flying[i].EndPointX;
                    float y = Flying[i].EndPointY;

                    float[] frameX = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[nPlayer];
                    float[] frameY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsY[nPlayer];

                    if (frameX != null && nFrame < frameX.Length)
                    {
                        x = frameX[nFrame];
                    }
                    if (frameY != null && nFrame < frameY.Length)
                    {
                        y = frameY[nFrame];
                    }

                    Flying[i].X = x;
                    Flying[i].Y = y;

                    // 飛行中の火花エフェクト（4フレームに1回発生）
                    if (nFrame % 4 == 0)
                    {
                        FireWorks.Start(Flying[i].Lane, Flying[i].Player, x, y);
                    }

                    // ノーツの描画
                    if (TJAPlayerPI.app.Tx.Notes != null)
                    {
                        // レーン番号(0-7)に応じてテクスチャ内の矩形を選択
                        // 130px四方のノーツ画像を想定（TJAPlayerPIの標準仕様）
                        Rectangle rect = new Rectangle(Flying[i].Lane * 130, 0, 130, 130);
                        TJAPlayerPI.app.Tx.Notes.t2D拡大率考慮描画(TJAPlayerPI.app.Device, CTexture.RefPnt.Center, x, y, rect);
                    }

                    // 【重要】FPSに関係なく、描画1回（1フレーム）につきカウントを1進める
                    if (!Flying[i].Counter.b停止中)
                    {
                        Flying[i].Counter.n現在の値++;

                        // 終了判定（指定されたフレーム数に達したか）
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