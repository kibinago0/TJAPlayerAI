using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Numerics;
using FDK;

namespace TJAPlayerPI
{
    /// <summary>
    /// 叩いたノーツが飛んでいく演出を制御するクラス。
    /// 60fps(16.66ms)基準のテーブルを参照し、1フレームごとの座標を完全に制御します。
    /// </summary>
    internal class FlyingNotes : CActivity
    {
        //-----------------
        // 構造体・フィールド
        //-----------------

        [StructLayout(LayoutKind.Sequential)]
        protected struct STFLY
        {
            public bool b使用中;
            public CCounter Counter;
            public int nPlayer;
            public int nLane;
            public bool bIsL; // 第3引数がbool型（大音符判定）であることに対応
        }

        protected STFLY[] stフライ = new STFLY[128];

        /// <summary>
        /// 1フレーム(1/60秒)ごとの相対座標テーブル
        /// ここを書き換えることで、自由な軌道を設定できます。
        /// </summary>
        private static readonly Vector2[] FlyingPath = new Vector2[]
        {
            new Vector2(0, 0),      // 0フレーム
            new Vector2(10, -20),   // 1
            new Vector2(25, -45),   // 2
            new Vector2(45, -75),   // 3
            new Vector2(70, -110),  // 4
            new Vector2(100, -150), // 5
            new Vector2(135, -195), // 6
            new Vector2(175, -245), // 7
            new Vector2(220, -300), // 8
            new Vector2(270, -360), // 9
            new Vector2(325, -425), // 10
            new Vector2(385, -495), // 11
            new Vector2(450, -570), // 12
            new Vector2(520, -650), // 13
            new Vector2(595, -735), // 14
            new Vector2(675, -825), // 15
        };

        private int MaxFrameCount => FlyingPath.Length;

        // CAct演奏Drumsレーン太鼓.cs 等から参照されるプロパティ
        public int[] StartPointX = new int[2];
        public int[] StartPointY = new int[2];

        //-----------------
        // コンストラクタ
        //-----------------

        public FlyingNotes() { }

        // CStage演奏画面共通.cs (line 63) での 2引数 new 呼び出しに対応
        public FlyingNotes(object arg1, object arg2) { }

        //-----------------
        // メソッド
        //-----------------

        /// <summary>
        /// ノーツ飛翔アニメーションを開始します
        /// </summary>
        /// <param name="nLane">レーン番号</param>
        /// <param name="nPlayer">プレイヤー番号</param>
        /// <param name="bIsL">大音符かどうか(引数エラーCS1503対策でintからboolへ変更)</param>
        public void Start(int nLane, int nPlayer, bool bIsL)
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stフライ[i].b使用中)
                {
                    stフライ[i].b使用中 = true;
                    stフライ[i].nLane = nLane;
                    stフライ[i].nPlayer = nPlayer;
                    stフライ[i].bIsL = bIsL;
                    
                    // 16.66msごとにカウントアップし、配列のインデックスを回す
                    stフライ[i].Counter = new CCounter(0, MaxFrameCount - 1, 16.66, TJAPlayerPI.Timer);
                    break;
                }
            }
        }

        public override void On活性化()
        {
            for (int i = 0; i < 128; i++)
            {
                stフライ[i].b使用中 = false;
                stフライ[i].Counter = new CCounter();
            }
            base.On活性化();
        }

        public override void On非活性化()
        {
            for (int i = 0; i < 128; i++)
            {
                stフライ[i].Counter = null;
            }
            base.On非活性化();
        }

        public override int On進行描画()
        {
            if (base.b活性化してない) return 0;

            for (int i = 0; i < 128; i++)
            {
                if (!stフライ[i].b使用中) continue;

                stフライ[i].Counter.t進行();
                
                if (stフライ[i].Counter.b終了値に達した)
                {
                    stフライ[i].b使用中 = false;
                    continue;
                }

                int frameIdx = Math.Clamp(stフライ[i].Counter.n現在の値, 0, MaxFrameCount - 1);
                Vector2 offset = FlyingPath[frameIdx];

                // 描画座標の決定
                float drawX = this.StartPointX[stフライ[i].nPlayer] + offset.X;
                float drawY = this.StartPointY[stフライ[i].nPlayer] + offset.Y;

                // テクスチャとスキン矩形の取得 (エラーCS1061に基づき修正)
                if (TJAPlayerPI.Tx.FlyingNotes != null)
                {
                    // 大音符か小音符かで矩形を選択
                    int textureIndex = stフライ[i].bIsL ? 1 : 0; 
                    
                    // TJAPlayerPI.Skinの既存の矩形定義を参照
                    Rectangle rect = TJAPlayerPI.Skin.stFlyingNotes_Rect[textureIndex];

                    TJAPlayerPI.Tx.FlyingNotes.t2D描画(
                        TJAPlayerPI.Device, 
                        (int)drawX, 
                        (int)drawY, 
                        rect
                    );
                }
            }
            return 0;
        }
    }
}
