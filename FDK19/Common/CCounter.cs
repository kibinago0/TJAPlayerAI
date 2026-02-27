using System;

namespace FDK
{
    /// <summary>
    /// 一定間隔で単純増加する整数（カウント値）を扱う。
    /// </summary>
    public class CCounter
    {
        // 値プロパティ
        public int n開始値 { get; private set; }
        public int n終了値 { get; private set; }
        public int n現在の値 { get; set; }
        public double db開始値 { get; private set; }
        public double db終了値 { get; private set; }
        public double db現在の値 { get; set; }
        public long n間隔ms { get; private set; }
        public double db間隔 { get; private set; }

        // 状態プロパティ
        public bool b進行中 => (this.n前回更新時刻 != -1);
        public bool b停止中 => (this.n前回更新時刻 == -1);
        public bool b終了値に達した
        {
            get
            {
                if (db間隔 > 0) return (this.db現在の値 >= this.db終了値);
                return (this.n現在の値 >= this.n終了値);
            }
        }

        private long n前回更新時刻;
        private CTimerBase timer;

        // コンストラクタ
        public CCounter()
        {
            this.n前回更新時刻 = -1;
        }

        public CCounter(int n開始値, int n終了値, int n間隔ms, CTimerBase timer) : this()
        {
            this.t開始(n開始値, n終了値, n間隔ms, timer);
        }

        public CCounter(double db開始値, double db終了値, double db間隔, CTimerBase timer) : this()
        {
            this.t開始(db開始値, db終了値, db間隔, timer);
        }

        // 状態操作メソッド
        public void t開始(int n開始値, int n終了値, int n間隔ms, CTimerBase timer)
        {
            this.n開始値 = n開始値;
            this.n終了値 = n終了値;
            this.n間隔ms = n間隔ms;
            this.timer = timer;
            this.n現在の値 = n開始値;
            this.db間隔 = 0;
            this.n前回更新時刻 = (this.timer != null) ? this.timer.nシステム時刻ms : 0;
        }

        public void t開始(double db開始値, double db終了値, double db間隔, CTimerBase timer)
        {
            this.db開始値 = db開始値;
            this.db終了値 = db終了値;
            this.db間隔 = db間隔;
            this.timer = timer;
            this.db現在の値 = db開始値;
            this.n間隔ms = 0;
            this.n前回更新時刻 = (this.timer != null) ? this.timer.nシステム時刻ms : 0;
        }

        public void t停止()
        {
            this.n前回更新時刻 = -1;
        }

        public void t時間Reset()
        {
            if (this.timer != null)
                this.n前回更新時刻 = this.timer.nシステム時刻ms;
        }

        public void t進行()
        {
            if (this.n前回更新時刻 == -1 || this.timer == null) return;

            long n現在時刻 = this.timer.nシステム時刻ms;
            long n経過時間 = n現在時刻 - this.n前回更新時刻;

            if (n経過時間 >= this.n間隔ms)
            {
                long n増加量 = n経過時間 / this.n間隔ms;
                this.n現在の値 += (int)n増加量;
                this.n前回更新時刻 += n増加量 * this.n間隔ms;

                if (this.n現在の値 > this.n終了値)
                    this.n現在の値 = this.n終了値;
            }
        }

        public void t進行db()
        {
            if (this.n前回更新時刻 == -1 || this.timer == null) return;

            long n現在時刻 = this.timer.nシステム時刻ms;
            double db経過時間 = (n現在時刻 - this.n前回更新時刻) / 1000.0;

            if (db経過時間 >= this.db間隔)
            {
                double db増加量 = db経過時間 / this.db間隔;
                this.db現在の値 += db増加量;
                this.n前回更新時刻 += (long)(db増加量 * this.db間隔 * 1000.0);

                if (this.db現在の値 > this.db終了値)
                    this.db現在の値 = this.db終了値;
            }
        }

        public void t進行Loop()
        {
            if (this.n前回更新時刻 == -1 || this.timer == null) return;

            long n現在時刻 = this.timer.nシステム時刻ms;
            long n経過時間 = n現在時刻 - this.n前回更新時刻;

            if (n経過時間 >= this.n間隔ms)
            {
                long n増加量 = n経過時間 / this.n間隔ms;
                this.n現在の値 += (int)n増加量;
                this.n前回更新時刻 += n増加量 * this.n間隔ms;

                int n幅 = this.n終了値 - this.n開始値 + 1;
                if (n幅 > 0)
                    this.n現在の値 = this.n開始値 + (this.n現在の値 - this.n開始値) % n幅;
            }
        }

        public void t進行LoopDb()
        {
            if (this.n前回更新時刻 == -1 || this.timer == null) return;

            long n現在時刻 = this.timer.nシステム時刻ms;
            double db経過時間 = (n現在時刻 - this.n前回更新時刻) / 1000.0;

            if (db経過時間 >= this.db間隔)
            {
                double db増加量 = db経過時間 / this.db間隔;
                this.db現在の値 += db増加量;
                this.n前回更新時刻 += (long)(db増加量 * this.db間隔 * 1000.0);

                double db幅 = this.db終了値 - this.db開始値;
                if (db幅 > 0)
                    this.db現在の値 = this.db開始値 + Math.Abs(this.db現在の値 - this.db開始値) % db幅;
            }
        }

        public delegate void DGキー処理();
        private int nキー反復用カウンタ;

        public void tキー反復(bool bキー押下, DGキー処理 tキー処理)
        {
            if (!bキー押下)
            {
                this.nキー反復用カウンタ = 0;
                return;
            }

            this.nキー反復用カウンタ++;
            if (this.nキー反復用カウンタ == 1)
            {
                tキー処理();
            }
            else if (this.nキー反復用カウンタ >= 40) // 200ms (5ms * 40)
            {
                if ((this.nキー反復用カウンタ % 6) == 0) // 30ms (5ms * 6)
                {
                    tキー処理();
                }
            }
        }
    }
}