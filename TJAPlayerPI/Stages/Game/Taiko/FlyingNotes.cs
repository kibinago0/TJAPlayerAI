using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Numerics; // SharpDXの代わりに.NET標準のNumericsを使用
using FDK;

namespace TJAPlayerPI
{
    /// <summary>
    /// 叩いたノーツがゲージやスコア方向へ飛んでいく演出を制御するクラス。
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
            public int nCourse;
        }

        protected STFLY[] stフライ = new STFLY[128];

        /// <summary>
        /// 1フレーム(1/60秒)ごとの相対座標テーブル
        /// Vector2(X方向の移動量, Y方向の移動量)
        /// 配列の要素数が演出の総フレーム数になります。
        /// </summary>
        private static readonly Vector2[] FlyingPath = new Vector2[]
        {
            new Vector2(0, 0),      // 0フレーム目 (ヒット位置)
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
            new Vector2(385, -440), // 11フレーム目
            new Vector2(450, -510), // 12フレーム目
            new Vector2(520, -585), // 13フレーム目
            new Vector2(595, -665), // 14フレーム目
            new Vector2(675, -750), // 15フレーム目
            // 必要な分だけ座標を追加してください
        };

        private int MaxFrameCount => FlyingPath.Length;

        /// <summary>
        /// 外部（CAct演奏Drumsレーン太鼓など）からセット・参照される描画開始座標
        /// </summary>
        public int[] StartPointX = new int[2];
        public int[] StartPointY = new int[2];

        //-----------------
        // コンストラクタ
        //-----------------

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public FlyingNotes() 
        {
            base.b活性化してない = true;
        }

        /// <summary>
        /// 外部クラスでの new FlyingNotes(arg1, arg2) 呼び出しに対応するためのコンストラクタ
        /// </summary>
        public FlyingNotes(object arg1, object arg2) : this()
        {
            // 引数は内部で保持する必要がないため無視します
        }

        //-----------------
        // メソッド
        //-----------------

        /// <summary>
        /// ノーツ飛翔アニメーションを開始します
        /// </summary>
        /// <param name="nLane">レーン番号(音符の種類判定用)</param>
        /// <param name="nPlayer">プレイヤー番号(0:1P, 1:2P)</param>
        /// <param name="nCourse">コース番号</param>
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
                    
                    // CCounter(開始, 終了, 1フレームのms, タイマー)
                    // 16.66ms間隔に設定することで、60fpsの挙動を再現します
                    stフライ[i].Counter = new CCounter(0, MaxFrameCount - 1, 16.66, TJAPlayerPI.app.Timer);
                    break;
                }
            }
        }

        // 2引数での呼び出しにも対応
        public void Start(int nLane, int nPlayer)
        {
            this.Start(nLane, nPlayer, 0);
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
        /// 進行描画。FDKのメインループから毎フレーム呼び出されます。
        /// </summary>
        public override int On進行描画()
        {
            if (base.b活性化してない) return 0;

            for (int i = 0; i < 128; i++)
            {
                if (!stフライ[i].b使用中) continue;

                // タイマー更新
                stフライ[i].Counter.t進行();
                
                // 終了判定
                if (stフライ[i].Counter.b終了値に達した)
                {
                    stフライ[i].b使用中 = false;
                    continue;
                }

                // 現在のフレーム番号(配列のインデックス)を取得
                int frameIdx = stフライ[i].Counter.n現在の値;
                
                // 配列の範囲内にクランプ（安全策）
                if (frameIdx >= MaxFrameCount) frameIdx = MaxFrameCount - 1;
                
                // テーブルから座標オフセットを取得
                Vector2 offset = FlyingPath[frameIdx];

                // 最終的な描画座標の計算
                float drawX = this.StartPointX[stフライ[i].nPlayer] + offset.X;
                float drawY = this.StartPointY[stフライ[i].nPlayer] + offset.Y;

                // テクスチャの描画
                // ※TJAPlayerPI.app経由でのアクセスに統一
                var txNotes = TJAPlayerPI.app.Tx.Flying_Notes;
                if (txNotes != null)
                {
                    // 3番以上のレーンは大音符として扱う（プロジェクト仕様）
                    int textureIndex = (stフライ[i].nLane >= 3) ? 1 : 0; 
                    
                    // スキンに定義された矩形範囲を使用して描画
                    Rectangle rect = TJAPlayerPI.app.Skin.stFlyingNotes_Rect[textureIndex];

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
