using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Numerics; // SharpDXの代わりにSystem.Numericsを使用
using FDK;

namespace TJAPlayerPI
{
    /// <summary>
    /// 叩いたノーツが飛んでいく演出を制御するクラス。
    /// 1フレーム(60fps)単位で座標テーブルを参照して移動します。
    /// </summary>
    internal class FlyingNotes : CActivity // クラス名を外部参照に合わせて FlyingNotes に変更
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
        }

        protected STFLY[] stフライ = new STFLY[128];

        // --- フレーム単位の座標指定テーブル (60fps基準) ---
        // (x, y) の相対座標を配列で定義。ここを書き換えることで軌道を1フレーム単位で制御可能です。
        private static readonly Vector2[] FlyingPath = new Vector2[]
        {
            new Vector2(0, 0),      // 0フレーム
            new Vector2(12, -20),   // 1フレーム
            new Vector2(28, -45),   // 2フレーム
            new Vector2(48, -75),   // 3フレーム
            new Vector2(75, -110),  // 4フレーム
            new Vector2(110, -150), // 5フレーム
            new Vector2(150, -195), // 6フレーム
            new Vector2(195, -245), // 7フレーム
            new Vector2(245, -300), // 8フレーム
            new Vector2(300, -360), // 9フレーム
            new Vector2(360, -425), // 10フレーム
            // ... 最大60フレーム分などのデータをここに追加
        };

        private int MaxFrameCount => FlyingPath.Length;

        //-----------------
        // メソッド
        //-----------------

        public void Start(int nLane, int nPlayer)
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stフライ[i].b使用中)
                {
                    stフライ[i].b使用中 = true;
                    stフライ[i].nLane = nLane;
                    stフライ[i].nPlayer = nPlayer;
                    
                    // カウンターの初期化（16.6msごとにカウントアップ）
                    stフライ[i].Counter = new CCounter(0, MaxFrameCount - 1, 16, TJAPlayerPI.app.Timer);
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
        /// 進行描画
        /// </summary>
        public override int On進行描画() // override対象を On進行描画 に修正
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

                // 現在のフレーム番号（インデックス）を取得
                int currentFrame = stフライ[i].Counter.n現在の値; // プロパティ名を修正
                int frameIdx = Math.Clamp(currentFrame, 0, MaxFrameCount - 1);
                
                Vector2 offset = FlyingPath[frameIdx];

                // SkinConfigから描画開始基準位置を取得
                // ※SkinConfigの構造はプロジェクトの定義に合わせています
                int startX = TJAPlayerPI.app.Skin.SkinConfig.Game.FlyingNotes.StartX[stフライ[i].nPlayer];
                int startY = TJAPlayerPI.app.Skin.SkinConfig.Game.FlyingNotes.StartY[stフライ[i].nPlayer];

                float drawX = startX + offset.X;
                float drawY = startY + offset.Y;

                if (TJAPlayerPI.app.Tx.FlyingNotes != null)
                {
                    // レーン番号に基づいてテクスチャ（小/大）を選択
                    int textureIndex = (stフライ[i].nLane >= 3) ? 1 : 0; 
                    
                    TJAPlayerPI.app.Tx.FlyingNotes.t2D描画(
                        TJAPlayerPI.app.Device, 
                        (int)drawX, 
                        (int)drawY, 
                        TJAPlayerPI.app.Skin.SkinConfig.Game.FlyingNotes.Rect[textureIndex]
                    );
                }
            }
            return 0;
        }
    }
}
