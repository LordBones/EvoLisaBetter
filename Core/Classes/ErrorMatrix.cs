using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenArt.Classes;

namespace GenArt.Core.Classes
{
    public class ErrorMatrix 
    {
        public int CONST_TileSize = 20;

        private int _inputPixelWidth;
        private int _inputPixelHeight;

        public int [] Matrix;
        public int MatrixWidth;
        public int MatrixHeight;

        private int [] _rouleteTable;
        const int CONST_MaxNormalizeValue = 1000000;

        public int InputPixelWidth
        {
            get { return _inputPixelWidth; }
        }

        public int InputPixelHeight
        {
            get { return _inputPixelHeight; }
        }

        public ErrorMatrix(int widthPixel, int heightPixel)
        {
            //CONST_TileSize = widthPixel / 2;
            //if (CONST_TileSize == 0) CONST_TileSize = widthPixel;

            this.MatrixWidth = widthPixel / CONST_TileSize + ((widthPixel % CONST_TileSize > 0) ? 1 : 0);
            this.MatrixHeight = heightPixel / CONST_TileSize + ((heightPixel % CONST_TileSize > 0) ? 1 : 0);

            Matrix = new int[this.MatrixHeight * this.MatrixWidth];
            for (int i  = 0; i < Matrix.Length; i++) Matrix[i] = 1;

            _rouleteTable = new int[this.MatrixHeight * this.MatrixWidth];
            this.FillRouleteTable();

            this._inputPixelHeight = heightPixel;
            this._inputPixelWidth = widthPixel;

        }

        public Rectangle GetTileByErrorMatrixIndex(int matrixIndex)
        {
            int y = matrixIndex / this.MatrixWidth;
            int x = matrixIndex % this.MatrixWidth;

            int originalTileHeight = this._inputPixelHeight - y * CONST_TileSize;
            if (originalTileHeight >= CONST_TileSize) originalTileHeight = CONST_TileSize;
            int originalTileWidth = this._inputPixelWidth - x * CONST_TileSize;
            if (originalTileWidth >= CONST_TileSize) originalTileWidth = CONST_TileSize;

            return new Rectangle(x * CONST_TileSize, y * CONST_TileSize, originalTileWidth, originalTileHeight);
        }
         
        

        /// <summary>
        /// return index in error matrix, where is bigger chance for choose index with bigger error
        /// </summary>
        /// <returns></returns>
        public int GetRNDMatrixRouleteIndex()
        {
            int rndIndex = Tools.GetRandomNumber(0, CONST_MaxNormalizeValue + 1);

            for (int index = 0; index < this._rouleteTable.Length; index++)
            {
                if (this._rouleteTable[index] > rndIndex)
                    return index;
            }

            return this._rouleteTable.Length - 1;
        }

        public void ComputeErrorMatrix(CanvasBGRA origImage, CanvasBGRA newImage)
        {
            if ((origImage.Width != newImage.Width || origImage.Height != newImage.Height) &&
                this._inputPixelWidth == origImage.WidthPixel && this._inputPixelHeight == origImage.HeightPixel)
                throw new ArgumentException();

            for (int matrixY = 0; matrixY < this.MatrixHeight; matrixY++)
            {
                for (int matrixX = 0; matrixX < this.MatrixWidth; matrixX++)
                {
                    int originalTileHeight = this._inputPixelHeight - matrixY * CONST_TileSize;
                    if(originalTileHeight >= CONST_TileSize)  originalTileHeight = CONST_TileSize;
                    int originalTileWidth = this._inputPixelWidth - matrixX * CONST_TileSize;
                    if (originalTileWidth >= CONST_TileSize) originalTileWidth = CONST_TileSize;

                    int indexTileStart = matrixY * CONST_TileSize * this._inputPixelWidth + matrixX * CONST_TileSize;

                    this.Matrix[matrixY*this.MatrixWidth+matrixX] = ComputeErrorTile(origImage, newImage, indexTileStart, originalTileWidth, originalTileHeight);
                }
            }

            FillRouleteTable();
        }

        private void FillRouleteTable()
        {
            long maxError = 0;


            for (int index = 0; index < Matrix.Length; index++)
            {
                if (maxError < Matrix[index]) maxError = Matrix[index];
            }

            long sumError = 0;
            for (int index = 0; index < Matrix.Length; index++)
            {
                long diffLive = Matrix[index]+1;
                sumError += diffLive;
            }

            
            int lastRouleteValue = 0;
            for (int index = 0; index < Matrix.Length; index++)
            {
                long diffLive = (Matrix[index]+1);

                int tmp = (int)(((long)diffLive * CONST_MaxNormalizeValue) / sumError);
                this._rouleteTable[index] = lastRouleteValue + tmp;
                lastRouleteValue = lastRouleteValue + tmp;
            }
        }

        private int ComputeErrorTile(CanvasBGRA origImage, CanvasBGRA newImage, int imageStartIndex, int imageLenX, int imageLenY)
        {
            int result = 0;

            int imageIndex = imageStartIndex*4; 

            for (int tileY = 0; tileY < imageLenY; tileY++)
            {
                int imageIndexX = imageIndex;
                for (int tileX = 0; tileX < imageLenX; tileX++)
                {
                    // copmute sumDiff

                    result += Tools.fastAbs(origImage.Data[imageIndexX] - newImage.Data[imageIndexX]);
                    result += Tools.fastAbs(origImage.Data[imageIndexX+1] - newImage.Data[imageIndexX+1]);
                    result += Tools.fastAbs(origImage.Data[imageIndexX+2] - newImage.Data[imageIndexX+2]);

                    imageIndexX += 4;
                }

                    imageIndex += this._inputPixelWidth * 4;
            }

                return result/(imageLenX*imageLenY*3) + 1;
        }

        #region ICloneable Members

        public ErrorMatrix Clone()
        {
            ErrorMatrix result = new ErrorMatrix(this._inputPixelWidth, this._inputPixelHeight);
            Array.Copy(this.Matrix, result.Matrix, this.Matrix.Length);
            return result;
        }

        #endregion
    }
}
