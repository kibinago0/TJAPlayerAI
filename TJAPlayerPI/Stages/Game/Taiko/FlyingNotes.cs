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
    /// 1フレーム(60fps)単位で座標テーブルを参照して移動します。
    /// </summary>
    internal class FlyingNotes : CActivity
    {
        //-----------------
        // 構造体・定数
        //-----------------

        [StructLayout(LayoutKind.Sequential)]
        protected struct STFLY
        {
            public bool b使用中;
            public CCounter Counter;
            public int nPlayer;
            public int nLane;
            public int nCourse;
        }

        protected STFLY[] stフライ = new STFLY[128];

        // --- フレーム単位の相対座標テーブル (60fps基準) ---
        // ここに 0フレーム目からの (x, y) オフセットを記述します。
        private static readonly Vector2[] FlyingPath = new Vector2[]
        {
            new Vector2(0, 0),      // 0
            new Vector2(15, -25),   // 1
            new Vector2(35, -55),   // 2
            new Vector2(60, -90),   // 3
            new Vector2(90, -130),  // 4
            new Vector2(125, -175), // 5
            new Vector2(165, -225), // 6
            new Vector2(210, -280), // 7
            new Vector2(260, -340), // 8
            new Vector2(315, -405), // 9
            new Vector2(375, -475), // 10
            // 必要に応じて 60フレーム分まで定義可能
        };

        private int MaxFrameCount => FlyingPath.Length;

        // 外部クラス（CAct演奏Drumsレーン太鼓など）から参照されるプロパティ
        public int[] StartPointX = new int[2];
        public int[] StartPointY = new int[2];

        //-----------------
        // コンストラクタ
        //-----------------
        
        // エラー CS1729 対策: 2引数のコンストラクタを追加
        public FlyingNotes() { }
        public FlyingNotes(object arg1, object arg2) 
        {
            // 引数が必要な初期化があればここで行う
        }

        //-----------------
        // メソッド
        //-----------------

        /// <summary>
        /// ノーツの飛翔を開始（3引数版）
        /// </summary>
        public void Start(int nLane, int nPlayer, int nCourse)
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stフライ[i].b使用中)
                {
                    stフライ[i].b使用中 = true;
                    stフライ[i].nLane = nLane;
                    stフライ[i].nPlayer = nPlayer;
                    stフライ[i].nCourse = nCourse;
                    
                    // 16.66ms間隔でカウントアップ
                    stフライ[i].Counter = new CCounter(0, MaxFrameCount - 1, 16, TJAPlayerPI.app.Timer);
                    break;
                }
            }
        }

        // 2引数版（念のため保持）
        public void Start(int nLane, int nPlayer) => Start(nLane, nPlayer, 0);

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
        /// 進行描画
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

                int currentFrame = stフライ[i].Counter.n現在の値;
                int frameIdx = Math.Clamp(currentFrame, 0, MaxFrameCount - 1);
                
                Vector2 offset = FlyingPath[frameIdx];

                // クラス内の StartPoint プロパティを使用（外部からセットされる想定）
                float drawX = this.StartPointX[stフライ[i].nPlayer] + offset.X;
                float drawY = this.StartPointY[stフライ[i].nPlayer] + offset.Y;

                // テクスチャ名はコードベースの慣習に合わせ Tx.Flying_Notes 等と仮定
                // もしエラーが続く場合は、Tx配下の正しいテクスチャ名を確認してください
                var txNotes = TJAPlayerPI.app.Tx.Flying_Notes; 
                if (txNotes != null)
                {
                    int textureIndex = (stフライ[i].nLane >= 3) ? 1 : 0; 
                    
                    // SkinConfigの構造に合わせてRectを参照
                    // エラーメッセージに基づき、Skin直下の旧来の配列形式を参照するように修正
                    var rect = TJAPlayerPI.app.Skin.stFlyingNotes_Rect[textureIndex];

                    txNotes.t2D描画(
                        TJAPlayerPI.app.Device, 
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
