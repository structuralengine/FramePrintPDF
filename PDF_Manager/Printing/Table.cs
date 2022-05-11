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
         * Cols=5 →       1             2             3             4             5...
         * Rows=4
         * ↓     Point[0, 0]   Point[0, 1]   Point[0, 2]   Point[0, 3]   Point[0, 4]   Point[0, 5]
         *           ・──────・──────・──────・──────・──────・
         *  1        │  Cell[1,1] │  Cell[1,2] │  Cell[1,3] │  Cell[1,4] │  Cell[1,5] │
         *           │ Align[1,1] │ Align[1,2] │ Align[1,3] │ Align[1,4] │ Align[1,5] │　　　　
         *           ├──────┼──────┼──────┼──────┼──────┤
         *  2        │  Cell[2,1] │  Cell[2,2] │  Cell[2,3] │  Cell[2,4] │  Cell[2,5] │
         *           │ Align[2,1] │ Align[2,2] │ Align[2,3] │ Align[2,4] │ Align[2,5] │　　　　
         *           ├──────┼──────┼──────┼──────┼──────┤
         *  3        │  Cell[3,1] │  Cell[3,2] │  Cell[3,3] │  Cell[3,4] │  Cell[3,5] │
         *           │ Align[3,1] │ Align[3,2] │ Align[3,3] │ Align[3,4] │ Align[3,5] │　　　　
         *           ├──────┼──────┼──────┼──────┼──────┤
         *  4        │  Cell[4,1] │  Cell[4,2] │  Cell[4,3] │  Cell[4,4] │  Cell[4,5] │
         *  :        │ Align[4,1] │ Align[4,2] │ Align[4,3] │ Align[4,4] │ Align[4,5] │　　　　
         *           ・──────・──────・──────・──────・──────・
         *        Point[4, 0]   Point[4, 1]   Point[4, 2]   Point[4, 3]   Point[4, 4]   Point[4, 5]
         *                                                                              
         *
         *           ┌-HolLW[0,1]-┬-HolLW[0,2]-┬-HolLW[0,3]-┬-HolLW[0,4]-┬-HolLW[0,5]-┐
         *           │            │            │            │            │            │　　　　
         * VtcLW[1,0]┥  VtcLW[1,1]┥  VtcLW[1,2]┥  VtcLW[1,3]┥  VtcLW[1,4]┥  VtcLW[1,5]┥　RowHeight[1] 　　　　
         *           │            │            │            │            │            │　　　　
         *           ├-HolLW[1,1]-┼-HolLW[1,2]-┼-HolLW[1,3]-┼-HolLW[1,4]-┼-HolLW[1,5]-┤
         *           │            │            │            │            │            │　　　　
         * VtcLW[2,0]┥  VtcLW[2,1]┥  VtcLW[2,2]┥  VtcLW[2,3]┥  VtcLW[2,4]┥  VtcLW[2,5]┥　RowHeight[2] 　　　　
         *           │            │            │            │            │            │　　　　
         *           ├-HolLW[2,1]-┼-HolLW[2,2]-┼-HolLW[2,3]-┼-HolLW[2,4]-┼-HolLW[2,5]-┤
         *           │            │            │            │            │            │　　　　
         * VtcLW[3,0]┥  VtcLW[3,1]┥  VtcLW[3,2]┥  VtcLW[3,3]┥  VtcLW[3,4]┥  VtcLW[3,5]┥　RowHeight[3] 　　　　
         *           │            │            │            │            │            │　　　　
         *           ├-HolLW[3,1]-┼-HolLW[3,2]-┼-HolLW[3,3]-┼-HolLW[3,4]-┼-HolLW[3,5]-┤
         *           │            │            │            │            │            │　　　　
         * VtcLW[4,0]┥  VtcLW[4,1]┥  VtcLW[4,2]┥  VtcLW[4,3]┥  VtcLW[4,4]┥  VtcLW[4,5]┥　RowHeight[4]　　　　
         *           │            │            │            │            │            │　　　　
         *           └-HolLW[4,1]-┴-HolLW[4,2]-┴-HolLW[4,3]-┴-HolLW[4,4]-┴-HolLW[4,4]-┘
         * 
         *           │            │            │            │            │            │　　　　
         *           └──────┴──────┴──────┴──────┴──────┘
         *              ColWidth[1]   ColWidth[2]   ColWidth[3]   ColWidth[4]   ColWidth[5]
         */
        #endregion

        private string[,] Cell;
        private int CellRows;
        private int CellCols;

        public int Rows { get { return this.CellRows; } }
        public int Cols { get { return this.CellCols; } }
        public string[,] AlignY;    // T:Top, B:Bottm, C:Center
        public string[,] AlignX;    // R:Right, L:Left, C:Center
        public double[,] HolLW;     // Holizonal Line Width
        public double[,] VtcLW;     // Vertical  Line Width
        public double[] RowHeight;
        public double[] ColWidth;

        public Table()
        {

        }
        public Table(int _rows, int _cols)
        {
            //** Row, Col Count
            this.CellRows = _rows;
            this.CellCols = _cols;

            this.setRowCount(_rows);
        }

        public void setRowCount(int _rows)
        {
            // 昔の情報を取っておく
            var oldCell = new string[_rows + 1, this.CellCols + 1];
            var oldAlignX = new string[_rows + 1, this.CellCols + 1];
            var oldAlignY = new string[_rows + 1, this.CellCols + 1];
            if (this.Cell != null)
            {
                oldCell = this.Cell.Clone() as string[,];
                oldAlignX = this.AlignX.Clone() as string[,];
                oldAlignY = this.AlignY.Clone() as string[,];
            }
            int oldRows = this.CellRows; // 昔のデータの行数

            //** Row, Col Count
            this.CellRows = _rows;

            //** Init Cell
            this.Cell = new string[_rows + 1, this.CellCols + 1];
            for (int j = 1; j <= this.CellCols; ++j)
                for (int i = 1; i <= oldRows; ++i)
                    this.Cell[i, j] = oldCell[i,j];


            //** Init Align
            this.AlignX = new string[_rows + 1, this.CellCols + 1];
            for (int j = 1; j <= this.CellCols; ++j) { 
                for (int i = 1; i <= oldRows; ++i)
                    this.AlignX[i, j] = oldAlignX[i, j];
                for (int i = oldRows + 1; i <= _rows; ++i)
                    this.AlignX[i, j] = "C";
            }

            this.AlignY = new string[_rows + 1, this.CellCols + 1];
            for (int j = 1; j <= this.CellCols; ++j)
            {
                for (int i = 1; i <= oldRows; ++i)
                    this.AlignY[i, j] = oldAlignY[i, j];
                for (int i = oldRows + 1; i <= _rows; ++i)
                    this.AlignX[i, j] = "C";
            }

            //** Init RowHeight
            this.RowHeight = new double[_rows + 1];
            this.RowHeight[0] = 0;
            for (int i = 1; i <= _rows; ++i)
                this.RowHeight[i] = -1;  // 0 > Fit Strings Size

            //** Init ColWidth
            this.ColWidth = new double[this.CellCols + 1];
            this.ColWidth[0] = 0;
            for (int j = 1; j <= this.CellCols; ++j)
                this.ColWidth[j] = -1;   // 0 > Fit Strings Size

            //** Init Holizonal Line Width
            this.HolLW = new double[_rows + 1, this.CellCols + 1];
            for (int i = 0; i <= _rows; ++i)
                this.SetHolLW(i, 0.2F);
            this.SetHolLW(0, 1);
            this.SetHolLW(_rows, 1);

            //** Init Holizonal Line Width
            this.VtcLW = new double[_rows + 1, this.CellCols + 1];
            for (int j = 0; j <= this.CellCols; ++j)
                this.SetVtcLW(j, 0.2F);
            this.SetVtcLW(0, 1);
            this.SetVtcLW(this.CellCols, 1);
        }

        public string this[int row, int col]
        {
            set
            {
                if (value != null)
                    this.Cell[row, col] = value;
            }
            get { return this.Cell[row, col]; }
        }

        internal void PrintTable(PdfDocument _mc)
        {
            #region Base Info
            XSize stringSize = _mc.MeasureString("W");
            ///////////////////////////////////////////////
            for (int i = 1; i <= CellRows; ++i)
            {
                if (RowHeight[i] < 0)
                    RowHeight[i] = stringSize.Height * 1.4F;
            }
            ///////////////////////////////////////////////
            for (int j = 1; j <= CellCols; ++j)
            {
                if (ColWidth[j] < 0)
                {
                    for (int i = 1; i <= CellRows; ++i)
                        ColWidth[j] = Math.Max(ColWidth[j], (Cell[i, j] + " ").Length * stringSize.Width);
                }
            }
            ///////////////////////////////////////////////
            XPoint[,] point = new XPoint[CellRows + 1, CellCols + 1];
            try
            {
                double y1 = _mc.currentPos.Y;
                for (int i = 0; i <= CellRows; ++i)
                {
                    y1 += RowHeight[i];
                    double x1 = _mc.currentPos.X;
                    for (int j = 0; j <= CellCols; ++j)
                    {
                        x1 += ColWidth[j];
                        point[i, j].Y = y1;
                        point[i, j].X = x1;
                    }
                }
            }
            catch { Text.PrtText(_mc, "Error: PrintTable() - Case1"); }
            #endregion

            #region Draw Lines
            try
            {
                //for (int i = 0; i < CellRows; ++i)
                    for (int i = 0; i <= CellRows; ++i)
                        for (int j = 0; j < CellCols; ++j)
                        if (HolLW[i, j + 1] > 0)
                            Shape.DrawLine(_mc, point[i, j], point[i, j + 1], HolLW[i, j + 1]);

                //for (int i = 0; i < CellRows - 1; ++i)
                    for (int i = 0; i < CellRows; ++i)
                        for (int j = 0; j <= CellCols; ++j)
                        if (VtcLW[i + 1, j] > 0)
                            Shape.DrawLine(_mc, point[i, j], point[i + 1, j], VtcLW[i + 1, j]);

            }
            catch {
                Text.PrtText(_mc, "Error: PrintTable() - Case2");
            }
            #endregion

            #region Draw String
            try
            {
                for (int i = 1; i <= CellRows; ++i)
                {
                    for (int j = 1; j <= CellCols; ++j)
                    {
                        if (this.Cell[i, j].Length > 0)
                        {
                            XSize textSize1 = _mc.MeasureString(this.Cell[i, j]);
                            XSize textSize2 = _mc.MeasureString(" ");
                            double XPos = 0;
                            double YPos = 0;
                            switch (AlignX[i, j])
                            {
                                case "L":
                                    XPos = point[i, j - 1].X + textSize2.Width;
                                    break;
                                case "R":
                                    XPos = point[i, j].X - (textSize1.Width + textSize2.Width);
                                    break;
                                case "C":
                                    XPos = point[i, j - 1].X + (ColWidth[j] - textSize1.Width) / 2;
                                    break;
                            }
                            switch (AlignY[i, j])
                            {
                                case "T":
                                    YPos = point[i - 1, j].Y - RowHeight[i];
                                    break;
                                case "B":
                                    YPos = point[i, j].Y;
                                    break;
                                case "C":
                                    YPos = point[i , j].Y -(( RowHeight[i]+ textSize1.Height )/2) ;
                                    break;
                            }
                            _mc.currentPos.X = XPos;
                            _mc.currentPos.Y = YPos;
                            Text.PrtText(_mc, this.Cell[i, j]);
                        }
                    }
                }
            }
            catch { Text.PrtText(_mc, "Error: PrintTable() - Case3"); }
            #endregion

            _mc.currentPos.X = point[CellRows, CellCols].X;
            _mc.currentPos.Y = point[CellRows, CellCols].Y;
        }

        public double GetTableHeight()
        {
            double result = 0;
            for (int i = 0; i <= CellRows; ++i)
                result += RowHeight[i];
            return result;
        }
        public double GetTableWidth()
        {
            double result = 0;
            for (int j = 0; j <= CellCols; ++j)
                result += ColWidth[j];
            return result;
        }
        public void SetHolLW(int row, double LineWidth)
        {
            for (int j = 0; j <= Cols; ++j)
                this.HolLW[row, j] = LineWidth;
        }
        public void SetVtcLW(int col, double LineWidth)
        {
            for (int i = 0; i <= Rows; ++i)
                this.VtcLW[i, col] = LineWidth;
        }
    }
}
