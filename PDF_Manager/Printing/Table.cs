using PDF_Manager.Printing.Comon;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Manager.Printing
{
    public class Table
    {
        #region This Class Palamator
        /*
         * Cols=5 →       0             1             2             3             4...
         * Rows=4
         * ↓     Point[0, 0]   Point[0, 1]   Point[0, 2]   Point[0, 3]   Point[0, 4]   Point[0, 5]
         *           ・──────・──────・──────・──────・──────・
         *  0        │  Cell[0,0] │  Cell[0,1] │  Cell[0,2] │  Cell[0,3] │  Cell[0,4] │
         *           │ Align[0,0] │ Align[0,1] │ Align[0,2] │ Align[0,3] │ Align[0,4] │　　　　
         *           ├──────┼──────┼──────┼──────┼──────┤
         *  1        │  Cell[1,0] │  Cell[1,1] │  Cell[1,2] │  Cell[1,3] │  Cell[1,4] │
         *           │ Align[1,0] │ Align[1,1] │ Align[1,2] │ Align[1,3] │ Align[1,4] │　　　　
         *           ├──────┼──────┼──────┼──────┼──────┤
         *  2        │  Cell[2,0] │  Cell[2,1] │  Cell[2,2] │  Cell[2,3] │  Cell[2,4] │
         *           │ Align[2,0] │ Align[2,1] │ Align[2,2] │ Align[2,3] │ Align[2,4] │　　　　
         *           ├──────┼──────┼──────┼──────┼──────┤
         *  3        │  Cell[3,0] │  Cell[3,1] │  Cell[3,2] │  Cell[3,3] │  Cell[3,4] │
         *  :        │ Align[3,0] │ Align[3,1] │ Align[3,2] │ Align[3,3] │ Align[3,4] │　　　　
         *           ・──────・──────・──────・──────・──────・
         *        Point[4, 0]   Point[4, 1]   Point[4, 2]   Point[4, 3]   Point[4, 4]   Point[4, 5]
         *                                                                              
         *
         *           ┌-HolLW[0,0]-┬-HolLW[0,1]-┬-HolLW[0,2]-┬-HolLW[0,3]-┬-HolLW[0,4]-┐
         *           │            │            │            │            │            │　　　　
         * VtcLW[0,0]┥  VtcLW[0,1]┥  VtcLW[0,2]┥  VtcLW[0,3]┥  VtcLW[0,4]┥  VtcLW[0,5]┥　RowHeight[0] 　　　　
         *           │            │            │            │            │            │　　　　
         *           ├-HolLW[1,0]-┼-HolLW[1,1]-┼-HolLW[1,2]-┼-HolLW[1,3]-┼-HolLW[1,4]-┤
         *           │            │            │            │            │            │　　　　
         * VtcLW[1,0]┥  VtcLW[1,1]┥  VtcLW[1,2]┥  VtcLW[1,3]┥  VtcLW[1,4]┥  VtcLW[1,5]┥　RowHeight[1] 　　　　
         *           │            │            │            │            │            │　　　　
         *           ├-HolLW[2,0]-┼-HolLW[2,1]-┼-HolLW[2,2]-┼-HolLW[2,3]-┼-HolLW[2,4]-┤
         *           │            │            │            │            │            │　　　　
         * VtcLW[2,0]┥  VtcLW[2,1]┥  VtcLW[2,2]┥  VtcLW[2,3]┥  VtcLW[2,4]┥  VtcLW[2,5]┥　RowHeight[2] 　　　　
         *           │            │            │            │            │            │　　　　
         *           ├-HolLW[3,0]-┼-HolLW[3,1]-┼-HolLW[3,2]-┼-HolLW[3,3]-┼-HolLW[3,4]-┤
         *           │            │            │            │            │            │　　　　
         * VtcLW[3,0]┥  VtcLW[3,1]┥  VtcLW[3,2]┥  VtcLW[3,3]┥  VtcLW[3,4]┥  VtcLW[3,5]┥　RowHeight[3]　　　　
         *           │            │            │            │            │            │　　　　
         *           └-HolLW[4,0]-┴-HolLW[4,1]-┴-HolLW[4,2]-┴-HolLW[4,3]-┴-HolLW[4,4]-┘
         * 
         *           │            │            │            │            │            │　　　　
         *           └──────┴──────┴──────┴──────┴──────┘
         *              ColWidth[0]   ColWidth[1]   ColWidth[2]   ColWidth[3]   ColWidth[4]
         */
        #endregion

        private string[,] Cell;
        private int CellRows;
        private int CellCols;

        public int Rows { get { return this.CellRows; } }
        public int Columns { get { return this.CellCols; } }

        public string[,] AlignY;    // T:Top, B:Bottm, C:Center
        public string[,] AlignX;    // R:Right, L:Left, C:Center
        public double[,] HolLW;     // Holizonal Line Width
        public double[,] VtcLW;     // Vertical  Line Width
        public double[] RowHeight;
        public double[] ColWidth;

        // 印刷に関する設定
        public double LineSpacing3; // 小さい（テーブル内などの）改行高さ pt ポイント


        private const double DEFAULT_LINE_WIDTH = 0; // デフォルトの線幅

        public Table(int _rows, int _cols)
        {
            // 印刷に関する初期設定
            this.LineSpacing3 = printManager.FontHeight;

            // 表題部分を初期化する
            this.CellRows = 0;
            this.CellCols = 0;
            this.ReDim(_rows, _cols);
        }

        /// <summary>
        /// 行を追加する
        /// </summary>
        /// <param name="_rows">行数</param>
        /// <param name="_cols">列数</param>
        public void ReDim(int _rows, int _cols)
        {
            // 昔の情報を取っておく
            var oldCell = (this.Cell != null) ? this.Cell.Clone() as string[,] : null;
            var oldAlignX = (this.AlignX != null) ? this.AlignX.Clone() as string[,] : null;
            var oldAlignY = (this.AlignY != null) ? this.AlignY.Clone() as string[,] : null;
            var oldRowHeight = (this.RowHeight != null) ? this.RowHeight.Clone() as double[] : null;
            var oldColWidth = (this.ColWidth != null) ? this.ColWidth.Clone() as double[] : null;
            var oldHolLW = (this.HolLW != null) ? this.HolLW.Clone() as double[, ]: null;
            var oldVtcLW = (this.VtcLW != null) ? this.VtcLW.Clone() as double[,] : null;

            //** Init Cell
            this.Cell = new string[_rows, _cols];
            for (int i = 0; i < this.CellRows; ++i)
                for (int j = 0; j < this.CellCols; ++j)
                    this.Cell[i, j] = oldCell[i, j];

            //** Init Align
            this.AlignX = new string[_rows, _cols];
            for (int i = 0; i < this.CellRows; ++i)
                for (int j = 0; j < this.CellCols; ++j)
                    this.AlignX[i, j] = oldAlignX[i, j];

            this.AlignY = new string[_rows, _cols];
            for (int i = 0; i < this.CellRows; ++i)
                for (int j = 0; j < this.CellCols; ++j)
                    this.AlignY[i, j] = oldAlignY[i, j];

            //** Init RowHeight
            this.RowHeight = new double[_rows];
            for (int i = 0; i < this.CellRows; ++i)
                this.RowHeight[i] = oldRowHeight[i];

            //** Init ColWidth
            this.ColWidth = new double[_cols];
            for (int j = 0; j < this.CellCols; ++j)
                this.ColWidth[j] = oldColWidth[j];

            //** Init Holizonal Line Width
            this.HolLW = new double[_rows + 1, _cols];
            for (int i = 0; i < this.CellRows; ++i)
                this.SetHolLW(i, this.HolLW[i, 0]);
            for (int i = this.CellRows; i <= _rows; ++i)
                for (int j = 0; j < _cols; ++j)
                    this.HolLW[i, j] = Table.DEFAULT_LINE_WIDTH;

            //** Init Vertical Line Width
            this.VtcLW = new double[_rows, _cols + 1];
            for (int j = 0; j < this.CellCols; ++j)
                this.SetVtcLW(j, this.VtcLW[0, j]);
            for (int j = this.CellCols; j <= _cols; ++j)
                for (int i = 0; i < _rows; ++i)
                    this.VtcLW[i, j] = Table.DEFAULT_LINE_WIDTH;

            //** Row, Col Count
            this.CellRows = _rows;
            this.CellCols = _cols;

        }

        /// <summary>
        /// セルの値を取得・設定する
        /// </summary>
        /// <param name="row">行番号 0～</param>
        /// <param name="col">列番号 0～</param>
        /// <returns></returns>
        public string this[int row, int col]
        {
            set
            {
                this.Cell[row, col] = value;
            }
            get 
            { 
                return this.Cell[row, col]; 
            }
        }

        /// <summary>
        /// 現在の Table の簡易コピーを作成します。
        /// </summary>
        /// <returns></returns>
        public Table Clone()
        {
            // MemberwiseCloneメソッドを使用
            return (Table)this.MemberwiseClone();
        }

        /// <summary>
        /// テーブルの高さ
        /// </summary>
        /// <returns>pt</returns>
        public double GetTableHeight()
        {
            double result = 0;
            for (int i = 0; i < this.CellRows; ++i)
                if(this.RowHeight[i] == double.NaN || this.RowHeight[i] <= 0)
                    result += this.LineSpacing3;
                else
                    result += this.RowHeight[i];
            return result;
        }

        /// <summary>
        /// 横罫線幅の設定
        /// </summary>
        /// <param name="row">行番号</param>
        /// <param name="LineWidth">線幅</param>
        public void SetHolLW(int row, double LineWidth)
        {
            for (int j = 0; j < this.Columns; ++j)
                this.HolLW[row, j] = LineWidth;
        }

        /// <summary>
        /// 縦罫線幅の設定
        /// </summary>
        /// <param name="col">列番号</param>
        /// <param name="LineWidth">線幅</param>
        public void SetVtcLW(int col, double LineWidth)
        {
            for (int i = 0; i < this.Rows; ++i)
                this.VtcLW[i, col] = LineWidth;
        }

        /// <summary>
        /// 何行印刷できるか調べる
        /// </summary>
        /// <param name="mc"></param>
        /// <param name="H1">デフォルト（改ページした場合）の印字位置</param>
        /// <returns>
        /// return[0] = 1ページ目の印刷可能行数, 
        /// return[1] = 2ページ目以降の印刷可能行数
        /// </returns>
        internal int[] getPrintRowCount(PdfDocument mc, double H1)
        {
            // 表題の印字高さ + 改行高
            double H2 = this.GetTableHeight();

            // 1行当りの高さ + 改行高
            double H3 = this.LineSpacing3;

            // 2ページ目以降（ページ全体を使ってよい場合）の行数
            double Hx = mc.currentPageSize.Height;
            Hx -= H1;
            Hx -= H2;
            int rows2 = (int)(Hx / H3); // 切り捨て

            // 1ページ目（現在位置から）の行数
            Hx -= mc.contentY;
            int rows1 = (int)(Hx / H3); // 切り捨て

            return new int[] { rows1, rows2 };
        }

        /// <summary>
        /// 印刷する
        /// </summary>
        /// <param name="_mc"></param>
        internal void PrintTable(PdfDocument _mc)
        {
            #region Base Info
            
            XSize[,] textSize1 = new XSize[this.Rows, this.Columns];
            for (int i = 0; i < this.CellRows; ++i)
                for (int j = 0; j < this.CellCols; ++j)
                {
                    if (this.Cell[i, j] == null)
                        continue;
                    textSize1[i, j] = _mc.MeasureString(this.Cell[i, j]);
                }

            ///////////////////////////////////////////////
            for (int i = 0; i < this.CellRows; ++i)
            {
                if (this.RowHeight[i] == double.NaN || this.RowHeight[i] <= 0)
                    this.RowHeight[i] = this.LineSpacing3;
            }

            ///////////////////////////////////////////////
            for (int j = 0; j < this.CellCols; ++j)
            {
                if (this.ColWidth[j] == double.NaN || this.ColWidth[j] <= 0)
                    for (int i = 0; i < this.CellRows; ++i)
                        this.ColWidth[j] = Math.Max(this.ColWidth[j], textSize1[i, j].Width);
            }
            ///////////////////////////////////////////////
            XPoint[,] point = new XPoint[this.CellRows + 1, this.CellCols + 1];
            try
            {
                double y1 = _mc.currentPos.Y;
                for (int i = 0; i <= this.CellRows; ++i)
                {
                    if (0 < i)
                        y1 += this.RowHeight[i - 1];

                    double x1 = _mc.currentPos.X;
                    for (int j = 0; j <= this.CellCols; ++j)
                    {
                        if(0 < j)
                            x1 += this.ColWidth[j - 1];
                        point[i, j].Y = y1;
                        point[i, j].X = x1;
                    }
                }
            }
            catch { Text.PrtText(_mc, "Error: PrintTable() - Base Info"); }
            #endregion

            #region Draw Lines
            try
            {
                for (int i = 0; i <= this.CellRows; ++i)
                    for (int j = 0; j < this.CellCols; ++j)
                        if (HolLW[i, j] != double.NaN)
                            if (0 < HolLW[i, j])
                                Shape.DrawLine(_mc, point[i, j], point[i, j + 1], HolLW[i, j]);

                for (int i = 0; i < CellRows; ++i)
                    for (int j = 0; j <= CellCols; ++j)
                        if (VtcLW[i, j] != double.NaN)
                            if (0 < VtcLW[i, j])
                                Shape.DrawLine(_mc, point[i, j], point[i + 1, j], VtcLW[i, j]);
            }
            catch
            {
                Text.PrtText(_mc, "Error: PrintTable() - Draw Lines");
            }
            #endregion

            #region Draw String
            try
            {
                for (int i = 0; i < CellRows; ++i)
                {
                    for (int j = 0; j < CellCols; ++j)
                    {
                        if (this.Cell[i, j] == null)
                            continue;

                        if (this.Cell[i, j].Length == 0)
                            continue;

                        double XPos = 0;
                        double YPos = 0;

                        XStringFormat align;

                        switch (AlignY[i, j])
                        {
                            case "T": YPos = point[i, j].Y;
                                break;
                            case "C": YPos = (point[i, j].Y + point[i + 1, j].Y) / 2;
                                break;
                            default:  YPos = point[i + 1, j].Y;
                                break;
                        }
                        switch (AlignX[i, j])
                        {
                            case "L":
                                XPos = point[i, j].X;
                                switch (AlignY[i, j]){
                                    case "T": align = XStringFormats.TopLeft;   
                                        break;
                                    case "C": align = XStringFormats.CenterLeft;
                                        break;
                                    default: align = XStringFormats.BottomLeft;
                                        break;
                                }
                                break;
                            case "R":
                                XPos = point[i, j + 1].X;
                                switch (AlignY[i, j]) {
                                    case "T": align = XStringFormats.TopRight;
                                        break;
                                    case "C": align = XStringFormats.CenterRight;
                                        break;
                                    default: align = XStringFormats.BottomRight;
                                        break;
                                }
                                break;
                            default: // case "C"
                                XPos = (point[i, j].X + point[i, j + 1].X) / 2;
                                switch (AlignY[i, j]) {
                                    case "T": align = XStringFormats.TopCenter;
                                        break;
                                    case "C": align = XStringFormats.Center;
                                        break;
                                    default: align = XStringFormats.BottomCenter;
                                        break;
                                }
                                break;
                        }
                        _mc.currentPos.X = XPos;
                        _mc.currentPos.Y = YPos;
                        Text.PrtText(_mc, this.Cell[i, j], align: align);
                    }
                }
            }
            catch { Text.PrtText(_mc, "Error: PrintTable() - Draw String"); }
            #endregion

            _mc.currentPos.X = point[this.CellRows, this.CellCols].X;
            _mc.currentPos.Y = point[this.CellRows, this.CellCols].Y;
        }

    }
}
