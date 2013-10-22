using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArt.Core.AST;
using GenArtCoreNative;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    public class DNARenderer
    {
        private CanvasBGRA _drawCanvas = new CanvasBGRA(0, 0);
        private SWTriangle _drawTriangle;
        private SWRectangle _drawRectangle;
        private SWElipse _drawElipse;

        private List<DnaPrimitive> [] _primitivesOnRows;

        public long FillPixels = 0;

        public enum RenderType { WPF, GDI, SoftwareUniversalPolygon, Software, SoftwareByRows };

        NativeFunctions _nativefunc = new NativeFunctions();

        public CanvasBGRA Canvas
        {
            get { return _drawCanvas; }
        }

        public DNARenderer(int width, int height)
        {
            this._drawTriangle = new SWTriangle();
            this._drawRectangle = new SWRectangle();
            this._drawCanvas = new CanvasBGRA(width, height);
            this._drawElipse = new SWElipse();
            _primitivesOnRows = new List<DnaPrimitive>[height];
            for (int i =0; i < height; i++)
            {
                _primitivesOnRows[i] = new List<DnaPrimitive>();
            }
        }

        private readonly static Color _black = Color.FromArgb(255, 0, 0, 0);
        private readonly static int _blackInt = _black.ToArgb();

        public void RenderDNA(DnaDrawing dna, RenderType typeRender)
        {
            if (typeRender == RenderType.Software) DnaRender_SoftwareTriangle(dna);
            else if (typeRender == RenderType.SoftwareByRows) DnaRender_SoftwareByRows(dna);

        }

        private void DnaRender_SoftwareTriangle(DnaDrawing dna)
        {
            //nativefunc.ClearFieldByColor(this._drawCanvas.Data, dna.BackGround.BrushColor.ToArgb());
            _nativefunc.ClearFieldByColor(this._drawCanvas.Data, _blackInt);
            //FillPixels += this._drawCanvas.CountPixels;
            //_drawCanvas.FastClearColor(dna.BackGround.BrushColor);

            DnaPrimitive [] dnaPolygons = dna.Polygons;
            int polyCount = dnaPolygons.Length;
            for (int i = 0; i < polyCount; i++)
            {
                DnaPrimitive polygon = dnaPolygons[i];
                //FillPixels += polygon.GetPixelSizePolygon();
                if (polygon is DnaPolygon)
                {
                    this._drawTriangle.RenderTriangle(polygon.Points, _drawCanvas, polygon.Brush.BrushColor);
                }
                else if (polygon is DnaRectangle)
                {
                    this._drawRectangle.Render((DnaRectangle)polygon, _drawCanvas);
                }
                else if (polygon is DnaElipse)
                {
                    this._drawElipse.Render((DnaElipse)polygon, _drawCanvas);
                }


            }

            /* this._drawElipse.Render(new DnaElipse() { StartPoint = new DnaPoint(100, 100), Height = 4, Width = 3 }, _drawCanvas);
             this._drawElipse.Render(new DnaElipse() { StartPoint = new DnaPoint(90, 100), Height = 3, Width = 3 }, _drawCanvas);
             this._drawElipse.Render(new DnaElipse() { StartPoint = new DnaPoint(80, 100), Height = 2, Width = 3 }, _drawCanvas);
             this._drawElipse.Render(new DnaElipse() { StartPoint = new DnaPoint(70, 100), Height = 1, Width = 3 }, _drawCanvas);
             this._drawElipse.Render(new DnaElipse() { StartPoint = new DnaPoint(60, 100), Height = 1, Width = 1 }, _drawCanvas);

             this._drawRectangle.Render(new DnaRectangle() { StartPoint = new DnaPoint(10, 150), EndPoint = new DnaPoint(34, 199), Brush = new DnaBrush(255, 255, 0, 0) }, this._drawCanvas);

             this._drawRectangle.Render(new DnaRectangle() { StartPoint = new DnaPoint(36, 150), EndPoint = new DnaPoint(60, 198), Brush = new DnaBrush(255, 255, 0, 0) }, this._drawCanvas);

             this._drawElipse.Render(new DnaElipse() { StartPoint = new DnaPoint(10, 150), Height = 49, Width = 25, Brush = new DnaBrush(255, 0, 255, 0) }, _drawCanvas);
             this._drawElipse.Render(new DnaElipse() { StartPoint = new DnaPoint(36, 150), Height = 48, Width = 25, Brush = new DnaBrush(255, 0, 255, 0) }, _drawCanvas);
          */
        }

        int [] listRowsForFill = new int[1000 * 3 + 3];

        private void DnaRender_SoftwareByRows(DnaDrawing dna)
        {
            DnaPrimitive [] dnaPolygons = dna.Polygons;
            int polyCount = dnaPolygons.Length;
            int pixelHigh = this._drawCanvas.HeightPixel;

            //int [] listRowsForFill = new int[dnaPolygons.Length * 3 + 3];
            int listRowsFFIndex = 3;


            int canvasIndexRow = 0;
            int colorBackground = _black.ToArgb();

            listRowsForFill[0] = 0;
            listRowsForFill[1] = _drawCanvas.WidthPixel*2;
            listRowsForFill[2] = colorBackground;

            for (int y = 0; y < pixelHigh; y++)
            {
                int startRowIndex = canvasIndexRow;

                for (int i = 0; i < polyCount; i++)
                {
                    int startX = 0;
                    int endX = 0;

                    DnaPrimitive primitive = dnaPolygons[i];
                    primitive.GetRangeWidthByRow(y, ref startX, ref endX);

                    if (startX <= endX)
                    {
                        listRowsForFill[listRowsFFIndex] = startRowIndex+startX;
                        listRowsForFill[listRowsFFIndex+1] = endX - startX +1;
                        listRowsForFill[listRowsFFIndex+2] = (int)primitive.Brush.ColorAsUInt;
                        listRowsFFIndex += 3;
                    }
                }

                canvasIndexRow += _drawCanvas.WidthPixel;

                if ((y & 1) == 1)
                {
                    _nativefunc.RenderOneRow(listRowsForFill, (listRowsFFIndex / 3) - 1, this._drawCanvas.Data);
                    listRowsFFIndex = 3;
                    listRowsForFill[0] = canvasIndexRow;
                }
                

                
                
                
                //SafeRenderOneRow(listRowsForFill, (listRowsFFIndex / 3) - 1, this._drawCanvas.Data);

                
            }

        }

        private void DnaRender_SoftwareByRows2(DnaDrawing dna)
        {
            DnaPrimitive [] dnaPolygons = dna.Polygons;
            int polyCount = dnaPolygons.Length;
            int pixelHigh = this._drawCanvas.HeightPixel;

            //int [] listRowsForFill = new int[dnaPolygons.Length * 3 + 3];
            int listRowsFFIndex = 3;


            int canvasIndexRow = 0;
            int colorBackground = _black.ToArgb();


            listRowsForFill[1] = _drawCanvas.WidthPixel;
            listRowsForFill[2] = colorBackground;

            for (int y = 0; y < pixelHigh; y++)
            {
                int startRowIndex = canvasIndexRow;

                listRowsForFill[0] = startRowIndex;

                for (int i = 0; i < polyCount; i++)
                {
                    int startX = 0;
                    int endX = 0;

                    DnaPrimitive primitive = dnaPolygons[i];
                    primitive.GetRangeWidthByRow(y, ref startX, ref endX);

                    if (startX <= endX)
                    {
                        listRowsForFill[listRowsFFIndex] = startRowIndex + startX;
                        listRowsForFill[listRowsFFIndex + 1] = endX - startX + 1;
                        listRowsForFill[listRowsFFIndex + 2] = (int)primitive.Brush.ColorAsUInt;
                        listRowsFFIndex += 3;
                    }
                }

                _nativefunc.RenderOneRow(listRowsForFill, (listRowsFFIndex / 3) - 1, this._drawCanvas.Data);
                //SafeRenderOneRow(listRowsForFill, (listRowsFFIndex / 3) - 1, this._drawCanvas.Data);

                listRowsFFIndex = 3;
                canvasIndexRow += _drawCanvas.WidthPixel;
            }

        }

        private void DnaRender_SoftwareByRowsSecondVersion(DnaDrawing dna)
        {
            DnaPrimitive [] dnaPolygons = dna.Polygons;
            int polyCount = dnaPolygons.Length;
            int pixelHigh = this._drawCanvas.HeightPixel;

            for (int y = 0; y < pixelHigh; y++)
            {
                this._primitivesOnRows[y].Clear();
            }
                

            for (int i = 0; i < polyCount; i++)
            {
                DnaPrimitive polygon = dnaPolygons[i];
                if (polygon is DnaRectangle)
                {
                    int startY = 0;
                    int endY = 0;
                    polygon.GetRangeHighSize(ref startY, ref endY);

                    for (int y =startY; y <= endY; y++)
                    {
                        this._primitivesOnRows[y].Add(polygon);
                    }
                }
            }

            int canvasIndexRow = 0;
            int colorBackground = _black.ToArgb();

            for (int y = 0; y < pixelHigh; y++)
            {
                _nativefunc.ClearFieldByColor(this._drawCanvas.Data, canvasIndexRow, _drawCanvas.WidthPixel, colorBackground);

                List<DnaPrimitive> primitiveList =this._primitivesOnRows[y];

                int partPolyCount = primitiveList.Count;

                for (int i = 0; i < partPolyCount; i++)
                {
                    DnaPrimitive polygon = primitiveList[i];
                    //FillPixels += polygon.GetPixelSizePolygon();
                    DnaPolygon poly = polygon as DnaPolygon;
                    DnaRectangle rec = polygon as DnaRectangle;
                    DnaElipse elips = polygon as DnaElipse;

                    if (poly != null)
                    {
                        //    this._drawTriangle.RenderTriangle(polygon.Points, _drawCanvas, polygon.Brush.BrushColor);
                    }
                    else if (rec != null)
                    {
                        this._drawRectangle.RenderRow(y,canvasIndexRow, (int)polygon.Brush.ColorAsUInt, rec, _drawCanvas);
                    }
                    else if (elips != null)
                    {
                        //   this._drawElipse.Render((DnaElipse)polygon, _drawCanvas); 
                    }
                }

                canvasIndexRow += _drawCanvas.Width;
            }

        }

        private void DnaRender_SoftwareByRowsFirstVersion(DnaDrawing dna)
        {
            
            //byte [] colorsAlpha = new byte[dna.Polygons.Length];

            //_drawCanvas.FastClearColor(dna.BackGround.BrushColor);
            DnaPrimitive [] dnaPolygons = dna.Polygons;
            int polyCount = dnaPolygons.Length;

            int canvasIndexRow = 0;
            int pixelHigh = this._drawCanvas.HeightPixel;
            int colorBackground = _black.ToArgb();

            //nativefunc.ClearFieldByColor(this._drawCanvas.Data, colorBackground);
            for (int y = 0; y < pixelHigh; y++)
            {
                _nativefunc.ClearFieldByColor(this._drawCanvas.Data,canvasIndexRow,_drawCanvas.WidthPixel, colorBackground);

                for (int i = 0; i < polyCount; i++)
                {
                    DnaPrimitive polygon = dnaPolygons[i];
                    //FillPixels += polygon.GetPixelSizePolygon();
                    DnaPolygon poly = polygon as DnaPolygon;
                    DnaRectangle rec = polygon as DnaRectangle;
                    DnaElipse elips = polygon as DnaElipse;

                    if (poly != null)
                    {
                        //    this._drawTriangle.RenderTriangle(polygon.Points, _drawCanvas, polygon.Brush.BrushColor);
                    }
                    else if (rec != null)
                    {
                        this._drawRectangle.RenderRow(y, canvasIndexRow,(int)polygon.Brush.ColorAsUInt, rec, _drawCanvas);
                    }
                    else if (elips != null)
                    {
                        //   this._drawElipse.Render((DnaElipse)polygon, _drawCanvas); 
                    }


                }
                
                canvasIndexRow += _drawCanvas.Width;
            }
            
        }

        private void SafeRenderOneRow(int [] listRowsForApply, int countRows, byte [] canvas)
        {
            _nativefunc.ClearFieldByColor(canvas, 
                listRowsForApply[0]*4, listRowsForApply[1], listRowsForApply[2]);

            int index = 3;
            for(int rows = 0;rows < countRows;rows++)
            {
                //SWHelpers.RowApplyBlendColorSafeARGB(canvas,
                //    listRowsForApply[index]*4, (listRowsForApply[index] + listRowsForApply[index + 1] - 1)*4,
                //    listRowsForApply[index + 2]);

                _nativefunc.NewRowApplyColor64(canvas,
                    listRowsForApply[index] * 4, listRowsForApply[index + 1],
                    listRowsForApply[index + 2], (int)((((uint)listRowsForApply[index + 2]) >> 24) & 0xff));

                index +=3;
            }
        }

    }
}
