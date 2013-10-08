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
        private CanvasBGRA _drawCanvas = new CanvasBGRA(0,0);
        private SWTriangle _drawTriangle;
        private SWRectangle _drawRectangle;
        private SWElipse _drawElipse;

        public long FillPixels = 0;

        public enum RenderType{WPF,GDI,SoftwareUniversalPolygon, SoftwareTriangle};

        NativeFunctions nativefunc = new NativeFunctions();

        public CanvasBGRA Canvas
        {
            get { return _drawCanvas; }
        }
       
        public DNARenderer(int width, int height)
        {
            this._drawTriangle = new SWTriangle();
            this._drawRectangle = new SWRectangle();
            this._drawCanvas = new CanvasBGRA(width,height);
            this._drawElipse = new SWElipse();
        }

        private readonly static Color _black = Color.FromArgb(255, 0, 0, 0); 

        public void RenderDNA(DnaDrawing dna, RenderType typeRender)
        {
            if (typeRender == RenderType.SoftwareTriangle) DnaRender_SoftwareTriangle(dna);
        }

        private void DnaRender_SoftwareTriangle(DnaDrawing dna)
        {
            //nativefunc.ClearFieldByColor(this._drawCanvas.Data, dna.BackGround.BrushColor.ToArgb());
            nativefunc.ClearFieldByColor(this._drawCanvas.Data, _black.ToArgb());
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

            //this._drawElipse.Render(new DnaElipse() { Middle=new DnaPoint(100,100),Height= 4, Width=3 }, _drawCanvas);
            //this._drawElipse.Render(new DnaElipse() { Middle = new DnaPoint(90, 100), Height = 3, Width = 3 }, _drawCanvas);
            //this._drawElipse.Render(new DnaElipse() { Middle = new DnaPoint(80, 100), Height = 2, Width = 3 }, _drawCanvas);
            //this._drawElipse.Render(new DnaElipse() { Middle = new DnaPoint(70, 100), Height = 1, Width = 3 }, _drawCanvas);
            //this._drawElipse.Render(new DnaElipse() { Middle = new DnaPoint(60, 100), Height = 1, Width = 1 }, _drawCanvas);
            
            //this._drawRectangle.Render(new DnaRectangle()
            //{ StartPoint = new DnaPoint(10,150), EndPoint = new DnaPoint(34,199),Brush=new DnaBrush(255,255,0,0)}, this._drawCanvas);

            //this._drawRectangle.Render(new DnaRectangle() 
            //{ StartPoint = new DnaPoint(36, 150), EndPoint = new DnaPoint(60, 198), Brush = new DnaBrush(255, 255, 0, 0) }, this._drawCanvas);

            //this._drawElipse.Render(new DnaElipse() { Middle = new DnaPoint(10, 150), Height = 49, Width = 25, Brush = new DnaBrush(255, 0, 255, 0) }, _drawCanvas);
            //this._drawElipse.Render(new DnaElipse() { Middle = new DnaPoint(36, 150), Height = 48, Width = 25, Brush = new DnaBrush(255, 0, 255, 0) }, _drawCanvas);
         
        }

    }
}
