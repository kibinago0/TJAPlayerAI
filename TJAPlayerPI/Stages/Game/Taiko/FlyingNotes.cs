using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Numerics;
using FDK;

namespace TJAPlayerPI
{
    /// <summary>
    /// 叩いた音符が飛んでいく演出を制御するクラス。
    /// 60fps(1/60秒)単位で定義された座標テーブルに基づいて移動します。
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
            public bool bIsL; // 大音符判定（外部からの引数に合わせる）
        }

        protected STFLY[] stフライ = new STFLY[128];

        /// <summary>
        /// 1フレーム(1/60秒)ごとの相対座標テーブル。
        /// 配列の1要素が1フレームに対応します。
        /// ここを編集することで、自由自在な軌道を作成できます。
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
            // 必要に応じて60フレーム分などを定義可能
        };

        private int MaxFrameCount => FlyingPath.Length;

        // 外部（CAct演奏Drumsレーン太鼓など）からアクセスされる座標プロパティ
        public int[] StartPointX = new int[2];
        public int[] StartPointY = new int[2];

        //-----------------
        // コンストラクタ
        //-----------------

        public FlyingNotes() { }

        /// <summary>
        /// CStage演奏画面共通.cs での new FlyingNotes(this.app, this.skin) に対応
        /// </summary>
        public FlyingNotes(object arg1, object arg2) : this() { }

        //-----------------
        // メソッド
        //-----------------

        /// <summary>
        /// 飛翔アニメーションを開始
        /// </summary>
        /// <param name="nLane">レーン番号</param>
        /// <param name="nPlayer">プレイヤー(0or1)</param>
        /// <param name="bIsL">大音符かどうか(外部呼び出し元との型整合性)</param>
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
                    
                    // カウンター: 16.66msごとに1進む（60fps固定の挙動）
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

        /// <summary>
        /// 描画処理（毎フレーム実行）
        /// </summary>
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

                // フレーム番号を特定
                int frameIdx = Math.Clamp(stフライ[i].Counter.n現在の値, 0, MaxFrameCount - 1);
                Vector2 offset = FlyingPath[frameIdx];

                // 外部から代入された StartPointX/Y を基準に計算
                float drawX = this.StartPointX[stフライ[i].nPlayer] + offset.X;
                float drawY = this.StartPointY[stフライ[i].nPlayer] + offset.Y;

                // 描画実行
                if (TJAPlayerPI.Tx.FlyingNotes != null)
                {
                    // 大/小 音符に応じた矩形インデックス
                    int textureIndex = stフライ[i].bIsL ? 1 : 0; 
                    
                    // スキン定義の矩形を使用
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
