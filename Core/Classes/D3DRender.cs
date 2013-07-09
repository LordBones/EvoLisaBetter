using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GenArt.AST;
using SlimDX;
using SlimDX.Direct3D9;


namespace GenArt.Core.Classes
{

    public class D3DDnaRender : IDisposable
    {
        private int _width = 0;
        private int _height = 0;
        private Direct3D _d3d = null; 
        private Device _device = null;  
        
        private Surface _surface = null;
        private VertexDeclaration _vertexDecl = null;
        private byte [] _imageOutput = null;

        public D3DDnaRender(int canvasWidth, int canvasHeight)
        {
            PresentParameters present_params = new PresentParameters() { Windowed = true, SwapEffect = SwapEffect.Discard, BackBufferHeight = canvasHeight, BackBufferWidth = canvasWidth, BackBufferFormat = Format.X8R8G8B8};
        
            this._d3d = new Direct3D();
            this._device = new Device(_d3d, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing | CreateFlags.FpuPreserve , present_params);

            //new Device(_d3d, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, present_params);

            
            this._device.SetRenderState(RenderState.AlphaBlendEnable, true);
            this._device.SetRenderState(RenderState.AlphaTestEnable, true);
            this._device.SetRenderState(RenderState.ZEnable, false);
            this._device.SetRenderState(RenderState.ZWriteEnable, false);
            this._device.SetRenderState(RenderState.CullMode, SlimDX.Direct3D9.Cull.None);
            
            this._device.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceColor);
            this._device.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.One);
            this._device.SetRenderState(RenderState.Lighting, false);
            
            Matrix m = Matrix.OrthoOffCenterRH(0, canvasWidth - 1, 0, canvasHeight - 1, 0, 1);
            this._device.SetTransform(TransformState.Projection, m);

           
            this._surface = Surface.CreateOffscreenPlain(this._device, canvasWidth, canvasHeight, Format.X8R8G8B8, Pool.SystemMemory);
            //this._surface2 = Surface.CreateRenderTarget(this._device, 1, 1, Format.X8R8G8B8, MultisampleType.None, 0, true);

            //this._surface = this._texture.GetSurfaceLevel(0);

            //this._device.SetRenderTarget(0, this._surface2);
            this._device.SetRenderTarget(0, this._device.GetBackBuffer(0,0));


            this._vertexDecl = new VertexDeclaration(this._device, new[] {
                                new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0), 
                                new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0), 
                                VertexElement.VertexDeclarationEnd
                });

            this._device.VertexDeclaration = this._vertexDecl;
            this._width = canvasWidth;
            this._height = canvasHeight;
            this._imageOutput = new byte[canvasWidth * canvasHeight * 4];
        }

        public byte [] Render(DnaDrawing drawing, Color background)
        {
            this._device.Clear(ClearFlags.Target, new Color4(1.0f, 0.0f, 0.0f,0.0f), 1.0f, 0);


            //RenderSceneOld(drawing);
            //RenderScene(drawing);


            


            this._device.GetRenderTargetData(this._device.GetBackBuffer(0, 0), this._surface);
            
            byte [] bmp2 = this._imageOutput;
         
            //this._device.StretchRectangle(this._device.GetBackBuffer(0, 0), this._surface2, TextureFilter.None);
            //bmp2 = GetSurfaceBitmap(this._device.GetBackBuffer(0, 0));  //this._surface2);
            //bmp2 = GetSurfaceBitmap(this._device.GetBackBuffer(0, 0));
            //bmp2 = GetSurfaceBitmap(this._surface);
            
          
            

            //for (int i = 0; i < 1000000;i++ )
            

            //byte [] bmp = GetSurfaceBitmap(surface, width, height);
            //DataStream ds = SlimDX.Direct3D9.Surface.ToStream(surface, ImageFileFormat.Bmp);


            //Bitmap bmp = new Bitmap(ds);
            //ds.Close();
            //ds.Dispose();


            //Bitmap bmp = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(surface, ImageFileFormat.Bmp, new Rectangle(0, 0, width, height)));
            //SlimDX.Direct3D9.Surface. surface.
            //System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)(System.Drawing.Bitmap.FromStream(SlimDX.Direct3D9.Surface.ToStream(_surface, ImageFileFormat.Bmp)));

            //bmp.Save("D3Dtest.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            
           // byte [] bmp2 = GetSurfaceBitmap(this._surface2);

            //return new Bitmap(1, 1);
          //  return new byte[0];
            return bmp2;
        }

        private void RenderScene(DnaDrawing drawing)
        {
            DnaPolygon [] polygons = drawing.Polygons;
            int maxPolygonsPoints = polygons.Max(x => x.Points.Length);

            

            VertexBuffer vertexBuffer = new VertexBuffer(this._device, maxPolygonsPoints * Marshal.SizeOf(typeof(ColoredVertex)), Usage.WriteOnly, VertexFormat.None, Pool.Default);

            DataStream stream = vertexBuffer.Lock(0, 0, LockFlags.None);
            ColoredVertex [] vertex = new ColoredVertex[maxPolygonsPoints];

            this._device.BeginScene();

            for (int index = 0; index < drawing.Polygons.Count(); index++)
            {
                DnaPolygon plg = drawing.Polygons[index];
                
                int fillColor = plg.Brush.BrushColor.ToArgb();
                DnaPoint [] points = plg.Points;
                for (int ip = 0;ip < points.Length; ip++)
                {
                    vertex[ip] = new ColoredVertex(new Vector3(points[ip].X, this._height - points[ip].Y - 1, 0.0f), fillColor);
                    //vertex[index] = new ColoredVertex(new Vector3(points[index].X, points[index].Y, 0.0f), fillColor);

                }

                stream.Position = 0;
                stream.WriteRange(vertex);

                this._device.SetStreamSource(0, vertexBuffer, 0, Marshal.SizeOf(typeof(ColoredVertex)));
                //this._device.DrawPrimitives(PrimitiveType.LineStrip, 0, 2);
                for (int primitiv = 0; primitiv < polygons[index].Points.Length - 2; primitiv++)
                    this._device.DrawPrimitives(PrimitiveType.TriangleList, primitiv, 1);

            }

            stream.Close();
            stream.Dispose();
            vertexBuffer.Unlock();
            vertexBuffer.Dispose();
              

            this._device.EndScene();
        }

        private void RenderSceneOld(DnaDrawing drawing)
        {
            
            

            this._device.BeginScene();

            for (int index = 0; index < drawing.Polygons.Count(); index++)
            {
                VertexBuffer vertexBuffer = CreateVertexBuffer(this._device, drawing.Polygons[index], _width, _height);
                this._device.SetStreamSource(0, vertexBuffer, 0, Marshal.SizeOf(typeof(ColoredVertex)));
                //this._device.DrawPrimitives(PrimitiveType.LineStrip, 0, 2);
                for (int primitiv = 0; primitiv < drawing.Polygons[index].Points.Length - 2; primitiv++)
                    this._device.DrawPrimitives(PrimitiveType.TriangleList, primitiv, 1);

                vertexBuffer.Dispose();
            }
            this._device.EndScene();
        }

        private VertexBuffer CreateVertexBuffer(Device d, DnaPolygon plg, int width, int height)
        {
            DnaPoint [] plgpoints = plg.Points;
            VertexBuffer vertexBuffer = new VertexBuffer(d, plgpoints.Length * Marshal.SizeOf(typeof(ColoredVertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed
                );

            ColoredVertex [] vertex = new ColoredVertex[plgpoints.Length];

            int fillColor = plg.Brush.BrushColor.ToArgb();
            DnaPoint [] points = plg.Points;
            for (int index = 0; index < points.Length; index++)
            {
                vertex[index] = new ColoredVertex(new Vector3(plgpoints[index].X, height - plgpoints[index].Y - 1, 0.0f), fillColor);
                //vertex[index] = new ColoredVertex(new Vector3(points[index].X, points[index].Y, 0.0f), fillColor);

            }

            DataStream stream = vertexBuffer.Lock(0, 0, LockFlags.None);
            stream.WriteRange(vertex);
            stream.Close();
            stream.Dispose();
            vertexBuffer.Unlock();

            return vertexBuffer;
        }

        private byte[] GetSurfaceBitmap(Surface surface)
        {
            
            //byte [] test = null;
            //this._device.SetRenderTarget(0, _surface2);

            DataRectangle dr = surface.LockRectangle(LockFlags.ReadOnly | LockFlags.NoDirtyUpdate | LockFlags.NoSystemLock);
            DataStream ds = dr.Data;
            //test = new byte[dr.Data.Length];
            int destIndex = 0;
            int oneLineSize = this._width * 4;
            long length  = dr.Data.Length;
            int lengthLine = dr.Pitch;
            for (int index = 0; index < length; index += lengthLine)
            {
                ds.Read(this._imageOutput, destIndex, oneLineSize);
                ds.Position = index;
                destIndex += oneLineSize;
            }
                
            //    dr.Data.Read(test, 0, test.Length);
            surface.UnlockRectangle();

            //this._device.SetRenderTarget(0, _surface);
            
            //byte [] field = null;
            //this._texture.
            //using (DataStream ds = Surface.ToStream(surface, ImageFileFormat.Bmp))
            //using (DataStream ds = Texture.ToStream(this._texture, ImageFileFormat.Bmp))
            //{
            //    //field = new byte[ds.Length];
            //    ds.Position = 54;
            //    ds.Read(this._imageOutput, 0, this._imageOutput.Length);
            //    ds.Close();
            //}

            return this._imageOutput;
        }

        /// <summary>
        /// Represents a vertex with a position and a color.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ColoredVertex : IEquatable<ColoredVertex>
        {
            /// <summary>
            /// Gets or sets the position of the vertex.
            /// </summary>
            public Vector3 Position
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the color of the vertex.
            /// </summary>
            public int Color
            {
                get;
                set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ColoredVertex"/> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="color">The color.</param>
            public ColoredVertex(Vector3 position, int color)
                : this()
            {
                Position = position;
                Color = color;
            }

            /// <summary>
            /// Implements operator ==.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator ==(ColoredVertex left, ColoredVertex right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Implements operator !=.
            /// </summary>
            /// <param name="left">The left side of the operator.</param>
            /// <param name="right">The right side of the operator.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator !=(ColoredVertex left, ColoredVertex right)
            {
                return !(left == right);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            public override int GetHashCode()
            {
                return Position.GetHashCode() + Color.GetHashCode();
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <param name="obj">Another object to compare to.</param>
            /// <returns>
            /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (GetType() != obj.GetType())
                    return false;

                return Equals((ColoredVertex)obj);
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            public bool Equals(ColoredVertex other)
            {
                return (Position == other.Position && Color == other.Color);
            }
        }

        public void Dispose()
        {
            this._surface.Dispose();
            
            this._vertexDecl.Dispose();

            this._device.Dispose();
            this._d3d.Dispose();
            
            this._d3d = null;
            this._device =null;
            this._surface = null;
            
            this._vertexDecl = null;
            
        }
    }

    public static class D3DRender
    {
        private static  PresentParameters present_params = new PresentParameters(){ Windowed = true, SwapEffect = SwapEffect.Discard, BackBufferHeight = 10, BackBufferWidth = 10 };
           
        private static Direct3D d3d = new Direct3D();
        private static Device d = new Device(d3d, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, present_params);

       
        //Render a Drawing
        public static byte [] Render(DnaDrawing drawing, Color background, int width, int height)
        {
            //PresentParameters present_params = new PresentParameters();
            //present_params.Windowed = true;

            //present_params.SwapEffect = SwapEffect.Discard;
            //using (Direct3D d3d = new Direct3D())
            //using (Device d = new Device(d3d, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, present_params))
            
                {
                    d.SetRenderState(RenderState.AlphaBlendEnable, true);
                    d.SetRenderState(RenderState.AlphaTestEnable, true);
                    d.SetRenderState(RenderState.ZEnable, false);
                    d.SetRenderState(RenderState.ZWriteEnable, false);
                    d.SetRenderState(RenderState.CullMode, SlimDX.Direct3D9.Cull.None);
                    
                    d.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceAlpha);
                    d.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.InverseDestinationAlpha);

                    Matrix m = Matrix.OrthoOffCenterRH(0, width - 1, 0, height - 1, 0, 1);
                    d.SetTransform(TransformState.Projection, m);

            
                    Texture texture = new Texture(d, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                    Surface surface = texture.GetSurfaceLevel(0);
                    d.SetRenderTarget(0, surface);
                    d.Clear(ClearFlags.Target , new Color4(0.0f, 0.0f, 0.0f), 1.0f, 0);
                    d.SetRenderState(RenderState.Lighting, false);
                    var vertexDecl = new VertexDeclaration(d, new[] {
                                new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0), 
                                new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0), 
                                VertexElement.VertexDeclarationEnd
                });

                    d.VertexDeclaration = vertexDecl;


                    d.BeginScene();

                    for (int index = 0; index < drawing.Polygons.Count(); index++)
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(d, drawing.Polygons[index], width, height);
                        d.SetStreamSource(0, vertexBuffer, 0, Marshal.SizeOf(typeof(ColoredVertex)));
                        d.DrawPrimitives(PrimitiveType.LineStrip, 0, 2);
                        vertexBuffer.Dispose();

                    }

                    d.EndScene();

                    //for (int i = 0; i < 1000000;i++ )
                    //d.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);

                    
                    //byte [] bmp = GetSurfaceBitmap(surface, width, height);
                    //DataStream ds = SlimDX.Direct3D9.Surface.ToStream(surface, ImageFileFormat.Bmp);
                    

                    //Bitmap bmp = new Bitmap(ds);
                    //ds.Close();
                    //ds.Dispose();


                    //Bitmap bmp = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(surface, ImageFileFormat.Bmp, new Rectangle(0, 0, width, height)));
                    //SlimDX.Direct3D9.Surface. surface.
                    //System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)(System.Drawing.Bitmap.FromStream(SlimDX.Direct3D9.Surface.ToStream(surface, ImageFileFormat.Bmp)));

                    //bmp.Save("D3Dtest.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    //byte [] bmp2 = GetSurfaceBitmap(surface, width, height);

                    surface.Dispose();
                    texture.Dispose();
                    vertexDecl.Dispose();

                    //return new Bitmap(1, 1);
                    return new byte[0];// bmp2;
                }
        }

        private static VertexBuffer CreateVertexBuffer(Device d, DnaPolygon plg, int width, int height)
        {
            VertexBuffer vertexBuffer = new VertexBuffer(d, plg.Points.Length * Marshal.SizeOf(typeof(ColoredVertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed
                );
            
            ColoredVertex [] vertex = new ColoredVertex[plg.Points.Length];
            
            Color fillColor = plg.Brush.BrushColor;

            for(int index = 0; index < plg.Points.Length;index++)
            {
                vertex[index] =  new ColoredVertex( new Vector3(plg.Points[index].X , height - plg.Points[index].Y, 0.0f),  fillColor.ToArgb());
            }

            DataStream stream = vertexBuffer.Lock(0, 0, LockFlags.None);
            stream.WriteRange(vertex);
            stream.Close();
            stream.Dispose();
            vertexBuffer.Unlock();

            return vertexBuffer;
        }

        private static byte [] GetSurfaceBitmap(Surface surface, int width, int height)
        {
            byte [] field = null;

            using (DataStream ds = Surface.ToStream(surface, ImageFileFormat.Bmp))
            {
                field = new byte[ds.Length];
                ds.Read(field, 0, field.Length);
                ds.Close();
            }

            return field;
        }

        private static Bitmap GetSurfaceBitmap2(Surface surface, int width, int height)
        {
            byte [] field = null;

            using (DataStream ds = Surface.ToStream(surface, ImageFileFormat.Bmp))
            {
                field = new byte[ds.Length];
                ds.Read(field, 0, field.Length);
                ds.Close();
            }

            MemoryStream ms = new MemoryStream(field);

            return (Bitmap)Bitmap.FromStream(ms);
        }


         /*public void TakeSnapshot()
         {

             // Create a surface to render to.
              new SlimDX.Windows.RenderForm()
             {
             
             }
             Texture texture = new Texture(device, width, height, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

             Surface surface = texture.GetSurfaceLevel(0);

             // Set the render target to the surface we created.

             device.SetRenderTarget(0, surface);

             // Draw what we want to draw.

             // TODO: Implement this part to render the polygons.

             // Now we convert the surface to a bitmap and return the bitmap.

             return (System.Drawing.Bitmap)(System.Drawing.Bitmap.FromStream(SurfaceLoader.SaveToStream(ImageFileFormat.Bmp, surface)));

         }*/

        public static void Test()
        {
            PresentParameters present_params = new PresentParameters();
            present_params.Windowed = true;
           
            present_params.SwapEffect = SwapEffect.Discard;
            using (Device d = new Device(new Direct3D(), 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing , present_params))
            {
                d.SetRenderState(RenderState.AlphaBlendEnable, true);
                d.SetRenderState(RenderState.AlphaTestEnable, true);
                //d.SetRenderState(RenderState.DiffuseMaterialSource, SlimDX.Direct3D9.ColorSource.Color1);

                d.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceAlpha);
                d.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.InverseDestinationAlpha);

                //lpdevice->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);
                //lpdevice->SetRenderState(D3DRS_LIGHTING, FALSE);
                //lpdevice->SetRenderState(D3DRS_ZENABLE, FALSE);
                //lpdevice->SetRenderState(D3DRS_ZWRITEENABLE, FALSE);

                //Matrix m = Matrix.Translation(0.0f, 1.0f, 0.0f);
                
                //d.SetTransform(TransformState.World, m);
                Matrix m = Matrix.OrthoOffCenterRH(0, 199, 0, 199, 0, 1);
                d.SetTransform(TransformState.Projection, m);

                Texture texture = new Texture(d, 200, 200, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);

                Surface surface = texture.GetSurfaceLevel(0);

                // Set the render target to the surface we created.

                d.SetRenderTarget(0, surface);




                var vertexBuffer = new VertexBuffer(d,
                3 * Marshal.SizeOf(typeof(ColoredVertex)),
                Usage.WriteOnly,
                VertexFormat.None,
                Pool.Managed
            );


                var stream = vertexBuffer.Lock(0, 0, LockFlags.None);
                stream.WriteRange(new[] {
                    //new ColoredVertex( new Vector3(0.0f, 0.5f, 0.5f), Color.Red.ToArgb() ),
                    //            new ColoredVertex( new Vector3(0.5f, -0.5f, 0.5f), Color.Blue.ToArgb() ),
                    //            new ColoredVertex( new Vector3(-0.5f, -0.5f, 0.5f), Color.Green.ToArgb() ),
                                //new ColoredVertex( new Vector3(0.0f, 0.0f, 0.0f),  Color.FromArgb(255,255,0,0).ToArgb()),
                                //new ColoredVertex( new Vector3(99.0f, 199.0f, 0.0f), Color.FromArgb(255,0,255,0).ToArgb() ),
                                //new ColoredVertex( new Vector3(199.0f, 0.0f, 0.0f), Color.FromArgb(255,0,0,255).ToArgb() ),
                      
                                
                                          new ColoredVertex( new Vector3(0.0f, 0.0f, 0.0f),  Color.FromArgb(255,255,0,0).ToArgb()),
                                new ColoredVertex( new Vector3(0.0f, 199.0f, 0.0f), Color.FromArgb(255,0,255,0).ToArgb() ),
                                new ColoredVertex( new Vector3(199.0f, 0.0f, 0.0f), Color.FromArgb(255,0,0,255).ToArgb() ),
                      //        new ColoredVertex( new Vector3(0.0f, 0.0f, 0.0f),  Color.FromArgb(50,255,0,0).ToArgb()),
                        //        new ColoredVertex( new Vector3(40.0f, 200.0f, 0.0f), Color.FromArgb(100,0,255,0).ToArgb() ),
                        //        new ColoredVertex( new Vector3(40.0f, 0.0f, 0.0f), Color.FromArgb(255,0,0,255).ToArgb() ),
                        });

                vertexBuffer.Unlock();


                d.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0.0f, 0.0f, 0.0f), 1.0f, 0);
                d.BeginScene();
                d.ColorFill(surface, new Rectangle(0, 0, 200, 200), new SlimDX.Color4(255, 0, 0, 0));
                d.ColorFill(surface, new Rectangle(10, 10, 50, 50), new SlimDX.Color4(255, 100, 0, 0));

                //d.Viewport = new Viewport(;
                d.SetRenderState(RenderState.Lighting, false);

                var vertexDecl = new VertexDeclaration(d, new[] {
                                new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0), 
                                new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0), 
                                VertexElement.VertexDeclarationEnd
                });

               

                d.SetStreamSource(0, vertexBuffer, 0, Marshal.SizeOf(typeof(ColoredVertex)));
                d.VertexDeclaration = vertexDecl;
                //Context9.Device.VertexDeclaration = vertexDecl;

                //for (int i = 0; i < 1000000;i++ )
                    //d.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                    d.DrawPrimitives(PrimitiveType.LineStrip, 0, 2);

                d.EndScene();
                //d.Present();


                // Create the vertex buffer and fill with the triangle vertices. (Non-indexed)
                // Remember 3 vetices for a triangle, 2 tris per quad = 6.
                //VertexBuffer Vertices = new VertexBuffer(d, 6 * sizeof(ColoredVertex), Usage.WriteOnly, VertexFormat.Diffuse, Pool.Managed);
                //DataStream stream = Vertices.Lock(0, 0, LockFlags.None);
                //stream.WriteRange(BuildVertexData());
                //Vertices.Unlock();

                //VertexBuffer vb = new VertexBuffer(d,1024,Usage.WriteOnly,)

                // Draw what we want to draw.

                // TODO: Implement this part to render the polygons.

                // Now we convert the surface to a bitmap and return the bitmap.

                System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)(System.Drawing.Bitmap.FromStream(SlimDX.Direct3D9.Surface.ToStream(surface, ImageFileFormat.Bmp)));

                bmp.Save("D3Dtest.bmp", System.Drawing.Imaging.ImageFormat.Bmp);


            }
        }

        /// <summary>
        /// Represents a vertex with a position and a color.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ColoredVertex : IEquatable<ColoredVertex>
        {
            /// <summary>
            /// Gets or sets the position of the vertex.
            /// </summary>
            public Vector3 Position
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the color of the vertex.
            /// </summary>
            public int Color
            {
                get;
                set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ColoredVertex"/> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="color">The color.</param>
            public ColoredVertex(Vector3 position, int color)
                : this()
            {
                Position = position;
                Color = color;
            }

            /// <summary>
            /// Implements operator ==.
            /// </summary>
            /// <param name="left">The left.</param>
            /// <param name="right">The right.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator ==(ColoredVertex left, ColoredVertex right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Implements operator !=.
            /// </summary>
            /// <param name="left">The left side of the operator.</param>
            /// <param name="right">The right side of the operator.</param>
            /// <returns>The result of the operator.</returns>
            public static bool operator !=(ColoredVertex left, ColoredVertex right)
            {
                return !(left == right);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            public override int GetHashCode()
            {
                return Position.GetHashCode() + Color.GetHashCode();
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <param name="obj">Another object to compare to.</param>
            /// <returns>
            /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (GetType() != obj.GetType())
                    return false;

                return Equals((ColoredVertex)obj);
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            public bool Equals(ColoredVertex other)
            {
                return (Position == other.Position && Color == other.Color);
            }
        }

        private static SlimDX.Direct3D9.Direct3D _direct3D9 = new SlimDX.Direct3D9.Direct3D();
        private static Dictionary<IntPtr, SlimDX.Direct3D9.Device> _direct3DDeviceCache = new Dictionary<IntPtr, SlimDX.Direct3D9.Device>();

        private static SlimDX.Direct3D9.AdapterCollection _adaptersInfo = _direct3D9.Adapters;

        public static Bitmap CaptureDXScreen(IntPtr hWnd)
        {
            Bitmap bitmap = null;

            SlimDX.Direct3D9.AdapterInformation adapterInfo = _direct3D9.Adapters[0];

            SlimDX.Direct3D9.Device device;

            int DisplayWidth = adapterInfo.CurrentDisplayMode.Width;
            int DisplayHeight = adapterInfo.CurrentDisplayMode.Height;

            #region Get Direct3D Device

            // Retrieve the existing Direct3D device if we already created one for the given handle
            if (_direct3DDeviceCache.ContainsKey(hWnd))
            {
                device = _direct3DDeviceCache[hWnd];
            }
            // We need to create a new device
            else
            {
                SlimDX.Direct3D9.PresentParameters presentParams = new SlimDX.Direct3D9.PresentParameters();

                presentParams.Windowed = true;
                presentParams.BackBufferCount = 1;
                presentParams.BackBufferFormat = adapterInfo.CurrentDisplayMode.Format;
                presentParams.BackBufferWidth = DisplayWidth;
                presentParams.BackBufferHeight = DisplayHeight;
                presentParams.SwapEffect = SlimDX.Direct3D9.SwapEffect.Flip;
                presentParams.DeviceWindowHandle = hWnd;
                presentParams.PresentationInterval = SlimDX.Direct3D9.PresentInterval.One;
                presentParams.FullScreenRefreshRateInHertz = 0;
                presentParams.PresentFlags = SlimDX.Direct3D9.PresentFlags.LockableBackBuffer; //Massive speed-up?
                //presentParams.PresentFlags = SlimDX.Direct3D9.PresentFlags.None;

                presentParams.EnableAutoDepthStencil = true;
                while (true)
                {
                    presentParams.AutoDepthStencilFormat = SlimDX.Direct3D9.Format.D32;
                    if (_direct3D9.CheckDeviceFormat(0, SlimDX.Direct3D9.DeviceType.Hardware, presentParams.BackBufferFormat, SlimDX.Direct3D9.Usage.DepthStencil, SlimDX.Direct3D9.ResourceType.Surface, presentParams.AutoDepthStencilFormat)) break;

                    presentParams.AutoDepthStencilFormat = SlimDX.Direct3D9.Format.D24X8;
                    if (_direct3D9.CheckDeviceFormat(0, SlimDX.Direct3D9.DeviceType.Hardware, presentParams.BackBufferFormat, SlimDX.Direct3D9.Usage.DepthStencil, SlimDX.Direct3D9.ResourceType.Surface, presentParams.AutoDepthStencilFormat)) break;

                    presentParams.AutoDepthStencilFormat = SlimDX.Direct3D9.Format.D16;
                    if (_direct3D9.CheckDeviceFormat(0, SlimDX.Direct3D9.DeviceType.Hardware, presentParams.BackBufferFormat, SlimDX.Direct3D9.Usage.DepthStencil, SlimDX.Direct3D9.ResourceType.Surface, presentParams.AutoDepthStencilFormat)) break;

                    // Your graphics card does not support 16bit, 24bit nor 32bit depth stencil buffers!
                }

                device = new SlimDX.Direct3D9.Device(_direct3D9, adapterInfo.Adapter, SlimDX.Direct3D9.DeviceType.Hardware, hWnd, SlimDX.Direct3D9.CreateFlags.HardwareVertexProcessing, presentParams);
                _direct3DDeviceCache.Add(hWnd, device);

            }
            #endregion

          

            //* /* Method 2: GetBackBuffer [!DOES NOT WORK!] [!FASTER?!]
            //------------------------------------------------------------
            //--NOTE--: Output picture is black. DWM no matters I guess...
            //------------------------------------------------------------
            int _width = DisplayWidth /2;// _GlobalVariables.var_ScreenDivide;
            int _height = DisplayHeight / 2;//_GlobalVariables.var_ScreenDivide;

            Surface _renderTarget = null;

            using (Surface backBuffer = device.GetBackBuffer(0, 0))
            {
                Surface.ToFile(backBuffer, "__m2_backbuffer.jpg", ImageFileFormat.Jpg);
                /// First Get Render Target, so we know the exactly size of the surface
                Surface renderTargetTemp = device.GetRenderTarget(0);

                // Create our Surface  
                _renderTarget = Surface.CreateRenderTarget(device, _width, _height, renderTargetTemp.Description.Format, MultisampleType.None, 0, false);
                // Resize from Render Surface to Our Surface || src,dest
                device.StretchRectangle(renderTargetTemp, _renderTarget, TextureFilter.None);
                // Get Render Data
                renderTargetTemp = _renderTarget;

                // Create offscreen surface to use as copy of render target data
                _renderTarget = Surface.CreateOffscreenPlain(device, _width, _height, renderTargetTemp.Description.Format, Pool.SystemMemory);

                // copies renderTarget data from device memory to system memory || src,dest
                device.GetRenderTargetData(renderTargetTemp, _renderTarget);

                // And release our surface
                renderTargetTemp.Dispose();

                //enkel als debug:
                Surface.ToFile(_renderTarget, "_m2_surface.jpg", ImageFileFormat.Jpg);

                bitmap = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(_renderTarget, SlimDX.Direct3D9.ImageFileFormat.Bmp, new Rectangle(0, 0, _width, _height)));
            }
            

            return bitmap;

        }

        
        //Convert a list of DnaPoint to a list of System.Drawing.Point's
        private static Point[] GetGdiPoints(IList<DnaPoint> points, int scale)
        {
            Point[] pts = new Point[points.Count];
            int i = 0;
            foreach (DnaPoint pt in points)
            {
                pts[i++] = new Point(pt.X * scale, pt.Y * scale);
            }
            return pts;
        }
    }
}
