using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX;
using FDK;

namespace TJAPlayerPI
{
    /// <summary>
    /// 叩いたノーツがゲージやスコア方向へ飛んでいく演出を制御するクラス。
    /// 1フレーム(60fps)ごとに指定された座標リストに基づいて移動します。
    /// </summary>
    internal class CActSingleFlyingNotes : CActivity
    {
        //-----------------
        // 構造体・定数
        //-----------------

        /// <summary>
        /// 1つのフライングノーツの状態を管理する構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        protected struct STFLY
        {
            public bool b使用中;
            public CCounter Counter;
            public int nPlayer;
            public int nLane;
        }

        protected STFLY[] stフライ = new STFLY[128];

        // --- フレーム単位の座標指定テーブル ---
        // 60fpsを想定し、1要素が1フレーム(約16.6ms)に対応します。
        // ここでは例として60フレーム分（1秒間）の軌道を定義します。
        // (x, y) は判定位置からの相対座標、または絶対座標として利用可能です。
        private static readonly Vector2[] FlyingPath = new Vector2[]
        {
            new Vector2(0, 0),      // 0フレーム目
            new Vector2(10, -15),   // 1フレーム目
            new Vector2(25, -35),   // 2フレーム目
            new Vector2(45, -60),   // 3フレーム目
            new Vector2(70, -90),   // 4フレーム目
            new Vector2(100, -125), // 5フレーム目
            new Vector2(135, -165), // 6フレーム目
            new Vector2(175, -210), // 7フレーム目
            new Vector2(220, -260), // 8フレーム目
            new Vector2(270, -315), // 9フレーム目
            new Vector2(325, -375), // 10フレーム目
            // ... 必要に応じて、60fps分（またはそれ以上）の座標をここに列挙します。
            // 配列の長さがそのまま演出の最大フレーム数（寿命）となります。
        };

        // 配列の最大フレーム数
        private readonly int MaxFrameCount = FlyingPath.Length;

        //-----------------
        // メソッド
        //-----------------

        /// <summary>
        /// ノーツの飛翔を開始させる
        /// </summary>
        /// <param name="nLane">レーン番号</param>
        /// <param name="nPlayer">プレイヤー番号</param>
        public void Start(int nLane, int nPlayer)
        {
            for (int i = 0; i < 128; i++)
            {
                if (!stフライ[i].b使用中)
                {
                    stフライ[i].b使用中 = true;
                    stフライ[i].nLane = nLane;
                    stフライ[i].nPlayer = nPlayer;
                    
                    // カウンターの範囲を0〜(座標配列の長さ-1)に設定
                    // 1フレームあたり16.66ms進むように調整します
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
        /// 進行描画
        /// </summary>
        /// <returns>常に0</returns>
        public override int t進行描画()
        {
            if (!base.b活性化してない)
            {
                for (int i = 0; i < 128; i++)
                {
                    if (stフライ[i].b使用中)
                    {
                        stフライ[i].Counter.t進行();
                        if (stフライ[i].Counter.b終了値に達した)
                        {
                            stフライ[i].b使用中 = false;
                            continue;
                        }

                        // 現在のフレーム番号（インデックス）を取得
                        int currentFrame = stフライ[i].Counter.n現在値;

                        // 指定されたフレームの座標を取得
                        // 配列外参照を防ぐため、念のためクランプ処理を入れる
                        int frameIdx = Math.Min(currentFrame, MaxFrameCount - 1);
                        Vector2 offset = FlyingPath[frameIdx];

                        // 描画基準位置の設定（P1/P2や判定枠の位置に合わせて調整）
                        // ここでは、TJAPlayerPI.Skin の設定値を基準にします
                        int startX = TJAPlayerPI.Skin.nFlyingNotes_StartX[stフライ[i].nPlayer];
                        int startY = TJAPlayerPI.Skin.nFlyingNotes_StartY[stフライ[i].nPlayer];

                        float drawX = startX + offset.X;
                        float drawY = startY + offset.Y;

                        // ノーツの種類（音符の色など）に合わせてテクスチャを描画
                        if (TJAPlayerPI.Tx.FlyingNotes != null)
                        {
                            // 0:小, 1:大 などの切り替えロジック
                            int textureIndex = (stフライ[i].nLane >= 3) ? 1 : 0; 
                            
                            // 描画実行
                            TJAPlayerPI.Tx.FlyingNotes.t2D描画(
                                TJAPlayerPI.Device, 
                                (int)drawX, 
                                (int)drawY, 
                                TJAPlayerPI.Skin.stFlyingNotes_Rect[textureIndex]
                            );
                        }
                    }
                }
            }
            return 0;
        }
    }
}
