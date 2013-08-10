using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using GenArtCoreNative;

namespace GenArt.Core.Classes.SWRenderLibrary
{
    public class DNARenderer
    {
        private CanvasBGRA _drawCanvas = new CanvasBGRA(0,0);
        private SWTriangle _drawTriangle;

        public enum RenderType{WPF,GDI,SoftwareUniversalPolygon, SoftwareTriangle};

        NativeFunctions nativefunc = new NativeFunctions();

        public CanvasBGRA Canvas
        {
            get { return _drawCanvas; }
        }
       
        public DNARenderer(int width, int height)
        {
            this._drawTriangle = new SWTriangle();
            this._drawCanvas = new CanvasBGRA(width,height);
        }

        private readonly static Color _black = Color.FromArgb(255, 0, 0, 0); 

        public void RenderDNA(DnaDrawing dna, RenderType typeRender)
        {
            if (typeRender == RenderType.SoftwareTriangle) DnaRender_SoftwareTriangle(dna);
        }

        private void DnaRender_SoftwareTriangle(DnaDrawing dna)
        {
            nativefunc.ClearFieldByColor(this._drawCanvas.Data, dna.BackGround.BrushColor.ToArgb());
            //_drawCanvas.FastClearColor(dna.BackGround.BrushColor);

            DnaPolygon [] dnaPolygons = dna.Polygons;
            int polyCount = dnaPolygons.Length;
            for (int i = 0; i < polyCount; i++)
            {
                DnaPolygon polygon = dnaPolygons[i];
                this._drawTriangle.RenderTriangle(polygon.Points, _drawCanvas, polygon.Brush.BrushColor);

            }
        }

    }
}
