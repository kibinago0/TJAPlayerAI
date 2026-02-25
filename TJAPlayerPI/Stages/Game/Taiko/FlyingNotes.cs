using FDK;
using System;

namespace TJAPlayerPI;

internal class FlyingNotes : CActivity
{
    // コンストラクタ

    public FlyingNotes(CActChipEffects actChipEffects, FireWorks fireWorks)
    {
        this.actChipEffects = actChipEffects;
        this.FireWorks = fireWorks;
    }


    // メソッド
    public virtual void Start(int nLane, int nPlayer, bool isRoll = false)
    {
        if (TJAPlayerPI.app.Tx.Notes is null)
            return;

        for (int i = 0; i < 128; i++)
        {
            if (!Flying[i].IsUsing)
            {
                // 初期化
                Flying[i].IsUsing = true;
                Flying[i].Lane = nLane;
                Flying[i].Player = nPlayer;
                Flying[i].X = StartPointX[nPlayer];
                Flying[i].Y = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer];
                Flying[i].StartPointX = StartPointX[nPlayer];
                Flying[i].StartPointY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer];
                Flying[i].EndPointX = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[nPlayer];
                Flying[i].EndPointY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[nPlayer];
                Flying[i].OldValue = 0;
                Flying[i].IsRoll = isRoll;
                
                // フレームベースの座標指定が有効な場合は、そちらを使用する
                Flying[i].UseFrameBasedPosition = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.UseFrameBasedPosition;
                
                // 角度の決定
                Flying[i].Height = Math.Abs(TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[nPlayer] - TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer]);
                Flying[i].Width = Math.Abs((TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[nPlayer] - StartPointX[nPlayer])) / 2;

                // フレームベースの座標指定が無効な場合のみ、従来の計算を行う
                if (!Flying[i].UseFrameBasedPosition)
                {
                    Flying[i].Counter = new CCounter(0, 180, TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.Timer, TJAPlayerPI.app.Timer);
                    Flying[i].IncreaseX = (1.00 * Math.Abs((TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[nPlayer] - StartPointX[nPlayer]))) / 180;
                    Flying[i].IncreaseY = (1.00 * Math.Abs((TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[nPlayer] - TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[nPlayer]))) / 180;
                }
                else
                {
                    // フレームベースモード：60フレーム分のデータを持つため、カウンターは60までカウントする
                    Flying[i].Counter = new CCounter(0, 60, TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FrameTimer, TJAPlayerPI.app.Timer);
                    Flying[i].IncreaseX = 0;
                    Flying[i].IncreaseY = 0;
                }

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
       　　 Flying[i].UseFrameBasedPosition = false;
   　　 }
   　　 for (int i = 0; i < 2; i++)
    　　{
        　　StartPointX[i] = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointX[i];
    　　}

    　　// フレームベース座標の初期化（スキンで指定がない場合のデフォルト値を設定）
    　　if (TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[0].Length == 0)
    　　{
        　　// プレイヤー0の座標を開始点から終了点への直線移動に設定
        　　InitializeDefaultFramePositions(0);
    　　}
    　　if (TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[1].Length == 0)
    　　{
        　　// プレイヤー1の座標を開始点から終了点への直線移動に設定
        　　InitializeDefaultFramePositions(1);
    　　}

    　　base.On活性化();
　　}　　

　　/// <summary>
　　/// フレームベース座標のデフォルト値を初期化する（直線移動）
　　/// </summary>
　　private void InitializeDefaultFramePositions(int playerIndex)
　　{
    　　float startX = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointX[playerIndex];
    　　float startY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.StartPointY[playerIndex];
    　　float endX = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointX[playerIndex];
    　　float endY = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.EndPointY[playerIndex];

    　　// 60フレーム分の座標を直線補間で計算
    　　for (int frame = 0; frame < 60; frame++)
    　　{
        　　float t = frame / 59.0f; // 0.0～1.0の進行度
        　　TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[playerIndex][frame] = startX + (endX - startX) * t;
        　　TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsY[playerIndex][frame] = startY + (endY - startY) * t;
    　　}
　　}

    public override void On非活性化()
    {
        for (int i = 0; i < 128; i++)
        {
            Flying[i].Counter = null;
        }
        base.On非活性化();
    }

    public override int On進行描画()
    {
        if (!base.b活性化してない)
        {
            for (int i = 0; i < 128; i++)
            {
                if (Flying[i].IsUsing)
                {
                    Flying[i].OldValue = Flying[i].Counter.n現在の値;
                    Flying[i].Counter.t進行();

                    if (Flying[i].Counter.b終了値に達した)
                    {
                        Flying[i].Counter.t停止();
                        Flying[i].IsUsing = false;
                        actChipEffects.Start(Flying[i].Player, Flying[i].Lane);
                    }

                    // フレームベースの座標指定を使用する場合
                    if (Flying[i].UseFrameBasedPosition)
                    {
                        int currentFrame = Flying[i].Counter.n現在の値;

                        // カウンターが0から60の範囲内であり、スキンが座標データを持っている場合
                        if (currentFrame >= 0 && currentFrame < 60 &&
                            TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX != null &&
                            TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsY != null &&
                            TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[Flying[i].Player] != null &&
                            TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsY[Flying[i].Player] != null &&
                            currentFrame < TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[Flying[i].Player].Length &&
                            currentFrame < TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsY[Flying[i].Player].Length)
                        {
                            // スキンで指定された座標を使用
                            Flying[i].X = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsX[Flying[i].Player][currentFrame];
                            Flying[i].Y = TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FlyingNotes.FramePositionsY[Flying[i].Player][currentFrame];
                        }
                        else
                        {
                            // データが不足している場合は終了点に設定
                            Flying[i].X = Flying[i].EndPointX;
                            Flying[i].Y = Flying[i].EndPointY;
                        }

                        // フレームベースモードでは円形のエフェクト処理はスキップ
                    }
                    else
                    {
                        // 従来の円弧移動モード（フレームベースではない場合）
                        for (int n = Flying[i].OldValue; n < Flying[i].Counter.n現在の値; n++)
                        {
                            // 画面内エフェクト（火花）の生成
                            if (n % TJAPlayerPI.app.Skin.SkinConfig.Game.Effect.FireWorks.Timing == 0 && 
                                !Flying[i].IsRoll && Flying[i].Counter.n現在の値 > 18)
                            {
                                if (Flying[i].Lane == 3 || Flying[i].Lane == 4)
                                {
                                    FireWorks.Start(Flying[i].Lane, Flying[i].Player, Flying[i].X, Flying[i].Y);
                                }
                            }

                            // 円弧に沿った移動計算
                            float value = Flying[i].Counter.n現在の値 / 180.0f;
                            Vector2 begin = new Vector2(Flying[i].StartPointX, Flying[i].StartPointY);
                            Vector2 end = new Vector2(Flying[i].EndPointX, Flying[i].EndPointY);

                            // 円弧の半径を決定（プレイヤーごとに異なる方向）
                            float radius = Flying[i].Player == 0 ? -480 : 480;

                            // 円弧に沿った位置を計算
                            Vector2 position = MoveAlongArc(begin, end, radius, value);
                            Flying[i].X = position.X;
                            Flying[i].Y = position.Y;
                        }
                    }

                    // ノーツの描画
                    TJAPlayerPI.app.Tx.Notes?.t2D拡大率考慮描画(
                        TJAPlayerPI.app.Device,
                        CTexture.RefPnt.Center,
                        (int)Flying[i].X,
                        (int)Flying[i].Y,
                        new Rectangle(Flying[i].Lane * 130, 0, 130, 130)
                    );
                }
            }
        }
        return base.On進行描画();
    }


    #region [ private ]
    //-----------------

    /// <summary>
    /// ノーツの状態を保持する構造体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Status
    {
        /// <summary>ノーツのレーン（0-7）</summary>
        public int Lane;

        /// <summary>プレイヤー番号（0 or 1）</summary>
        public int Player;

        /// <summary>このノーツが現在使用中かどうか</summary>
        public bool IsUsing;

        /// <summary>アニメーション進行度を管理するカウンター</summary>
        public CCounter Counter;

        /// <summary>前フレームのカウンター値</summary>
        public int OldValue;

        /// <summary>ノーツの現在のX座標</summary>
        public double X;

        /// <summary>ノーツの現在のY座標</summary>
        public double Y;

        /// <summary>ノーツの高さ</summary>
        public int Height;

        /// <summary>ノーツの幅</summary>
        public int Width;

        /// <summary>1フレームあたりのX移動量（従来モード用）</summary>
        public double IncreaseX;

        /// <summary>1フレームあたりのY移動量（従来モード用）</summary>
        public double IncreaseY;

        /// <summary>ロール（連続ノーツ）かどうか</summary>
        public bool IsRoll;

        /// <summary>開始点のX座標</summary>
        public int StartPointX;

        /// <summary>開始点のY座標</summary>
        public int StartPointY;

        /// <summary>終了点のX座標</summary>
        public int EndPointX;

        /// <summary>終了点のY座標</summary>
        public int EndPointY;

        /// <summary>フレームベースの座標指定を使用するかどうか</summary>
        public bool UseFrameBasedPosition;
    }

    /// <summary>チップエフェクト（ノーツ到達時の効果）を管理するクラスへの参照</summary>
    private CActChipEffects actChipEffects;

    /// <summary>花火エフェクトを管理するクラスへの参照</summary>
    private FireWorks FireWorks;

    /// <summary>最大128個までのノーツの状態を保持する配列</summary>
    private Status[] Flying = new Status[128];

    /// <summary>プレイヤー別の開始点X座標</summary>
    public readonly int[] StartPointX = new int[2];


    /// <summary>
    /// 2点間を円弧に沿って移動する座標を計算する
    /// </summary>
    /// <param name="startPoint">開始点</param>
    /// <param name="endPoint">終了点</param>
    /// <param name="radius">円弧の半径（正値で上向き、負値で下向き）</param>
    /// <param name="t">進行度（0.0～1.0）</param>
    /// <returns>計算された現在の座標</returns>
    public static Vector2 MoveAlongArc(Vector2 startPoint, Vector2 endPoint, float radius, float t)
    {
        // 2点間の距離を計算
        float dx = endPoint.X - startPoint.X;
        float dy = endPoint.Y - startPoint.Y;
        float L = MathF.Sqrt(dx * dx + dy * dy);

        // 半径の絶対値
        float R_abs = MathF.Abs(radius);

        // 半径が小さすぎる場合は直線移動
        if (L / 2.0 > R_abs)
        {
            return new Vector2(
                startPoint.X + dx * t,
                startPoint.Y + dy * t
            );
        }

        // 中点を計算
        Vector2 midPoint = new Vector2(
            (startPoint.X + endPoint.X) / 2.0f,
            (startPoint.Y + endPoint.Y) / 2.0f
        );

        // 円の中心までの距離（垂直方向）を計算
        float H_abs = MathF.Sqrt(R_abs * R_abs - (L / 2.0f) * (L / 2.0f));
        float H = (radius >= 0) ? H_abs : -H_abs;

        // 2点を結ぶ線に対して垂直な単位ベクトルを計算
        float unitPerpX = dy / L;
        float unitPerpY = -dx / L;

        // 円の中心を計算
        Vector2 centerPoint = new Vector2(
            midPoint.X + H * unitPerpX,
            midPoint.Y + H * unitPerpY
        );

        // 開始点と終了点に対する円の中心からの角度を計算
        float theta_s = MathF.Atan2(startPoint.Y - centerPoint.Y, startPoint.X - centerPoint.X);
        float theta_e = MathF.Atan2(endPoint.Y - centerPoint.Y, endPoint.X - centerPoint.X);

        // 角度差を計算（最短経路を選択）
        float deltaTheta = theta_e - theta_s;

        if (deltaTheta > MathF.PI)
        {
            deltaTheta -= 2 * MathF.PI;
        }
        else if (deltaTheta < -MathF.PI)
        {
            deltaTheta += 2 * MathF.PI;
        }

        // 現在の角度を計算
        float theta_t = theta_s + t * deltaTheta;

        // 円弧上の現在位置を計算
        Vector2 currentPoint = new Vector2(
            centerPoint.X + R_abs * MathF.Cos(theta_t),
            centerPoint.Y + R_abs * MathF.Sin(theta_t)
        );

        return currentPoint;
    }

    //-----------------
    #endregion
}
