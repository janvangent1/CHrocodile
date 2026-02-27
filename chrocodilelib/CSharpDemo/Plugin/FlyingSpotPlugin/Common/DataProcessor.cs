using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CHRocodileLib;

namespace FSSCommon
{
    /// <summary>
    /// a collection of basic data saving / manipulation functions
    /// </summary>
    public class DataProcessor
    {
        /// <summary>
        /// the current shape object for data manipulation
        /// </summary>
        public CHRLibPlugin.FSS_PluginShape Shape;

        /// <summary>
        /// Saves the shape data to CSV
        /// </summary>
        /// <param name="filePath">target file path: if empty, the file name will be generated based on shape's label</param>
        /// <returns>Path to the BCRF file created</returns>
        public string SaveAsCSV(string filePath = "")
        {
            var NF = CultureInfo.GetCultureInfo("de-DE").NumberFormat;
            var fname = filePath.Length > 0 ? filePath : $"Fig_{Shape.Label}_{Shape.ShapeIndex}.csv";

            using (var fs = File.CreateText(fname))
            {
                string str = "Signals: ";
                foreach (var s in Shape.SignalInfos)
                {
                    str += "; " + s.SignalID;
                }
                fs.WriteLine(str);

                foreach (var S in Shape.Samples())
                {
                    str = "";
                    for (int i = 0; i < Shape.SignalInfos.Length; i++)
                    {
                        str += ";" + S.Get(i).ToString(NF);
                    }
                    fs.WriteLine(str);
                }
            }
            return fname;
        }

        /// <summary>
        /// Plots raw scan data onto RGB bitmap for a given data signal 'sigIdx'
        /// </summary>
        /// <param name="sigIdx">data signal index obtained via SignalIndex() function</param>
        /// <param name="Xidx">signal index for an X-encoder, usually SignalIndex(65)</param>
        /// <param name="Yidx">signal index for an Y-encoder, usually SignalIndex(66)</param>
        /// <param name="imgWidth">canvas width in pixels</param>
        /// <param name="imgHeight">canvas height in pixels</param>
        /// <returns>Bitmap object</returns>
        public Bitmap RawDataToBitmapRGB(int sigIdx, int Xidx, int Yidx, int imgWidth, int imgHeight)
        {
            // Get data for the desired plot signal, encoder X and encoder Y
            double minV = double.MaxValue,
                   maxValue = double.MinValue,
                   minX = minV, maxX = maxValue,
                   minY = minV, maxY = maxValue;

            foreach (var S in Shape.Samples())
            {
                double val = S.Get(sigIdx),
                       X = S.Get(Xidx), Y = S.Get(Yidx);
                if (!(double.IsNaN(val) || double.IsInfinity(val)))
                {
                    minV = Math.Min(minV, val);
                    maxValue = Math.Max(maxValue, val);
                }
                minX = Math.Min(minX, X);
                maxX = Math.Max(maxX, X);
                minY = Math.Min(minY, Y);
                maxY = Math.Max(maxY, Y);
            }
            // Create a heat map instance
            var heatMap = new ColorHeatMap(minV, maxValue);

            // Calculate the width and height of the scanned rectangle
            // and scale to the size of the image box in the MainForm.
            var rectWidth = Math.Abs(maxX - minX);
            var rectHeight = Math.Abs(maxY - minY);
            var scale = Math.Min(rectWidth, rectHeight) / Math.Max(rectWidth, rectHeight);

            if (rectWidth < rectHeight)
                imgWidth = (int)(imgWidth * scale);
            else
                imgHeight = (int)(imgHeight * scale);

            var bmp = new Bitmap(imgWidth, imgHeight, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            using (Graphics gfx = Graphics.FromImage(bmp))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                gfx.FillRectangle(brush, rect);
            }
            var bdata = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            double sx = (double)imgWidth / (maxX - minX),
                   sy = (double)imgHeight / (maxY - minY);

            unsafe
            {
                Int32* ptr = (Int32*)bdata.Scan0;
                Int32 step = bdata.Stride / 4;
                // Loop over all samples to fill the image data buffer
                foreach (var S in Shape.Samples())
                {
                    // Determine x and y coordinate of the image based on the return values
                    // of the encoders.
                    var xImg = (int)((S.Get(Xidx) - minX) * sx);
                    var yImg = (int)((S.Get(Yidx) - minY) * sy);

                    // Clip x and y to bounds
                    if ((uint)xImg >= (uint)imgWidth || (uint)yImg >= (uint)imgHeight)
                        continue;

                    // Get the heat map color for the current value
                    var color = heatMap.GetColorForValue(S.Get(sigIdx)).ToArgb();
                    int ofs = yImg * step + xImg;
                    ptr[ofs] = color;
                    if (xImg > 0 && yImg > 0)
                    {
                        var top = ofs - step;
                        ptr[ofs - 1] = color;
                        ptr[top] = color;
                        ptr[top - 1] = color;
                    }
                }
            }

            bmp.UnlockBits(bdata);
            return bmp;
        }
        
        /// <summary>
        /// Converts 2D bitmap data to an RGB bitmap for a given signal
        /// </summary>
        /// <param name="sigIdx">signal index (not signal ID)</param>
        public Bitmap GridDataToBitmapRGB(int sigIdx)
        {
            if (Shape.Type != CHRLibPlugin.FSS_PluginDataType.Interpolated2D)
                throw new Exception("RGB Bitmap can only be created for 2D interpolated data!");

            var stype = Shape.SignalInfos[sigIdx].DataType;

            bool isFloat = (stype == DataType.Float);
            if (!(isFloat || stype == DataType.Double))
                throw new Exception("Only floating-point data is supported!");

            var sbitmap = Shape.Bitmap;
            int w = (int)sbitmap.Width, h = (int)sbitmap.Height;

            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            using (Graphics gfx = Graphics.FromImage(bmp))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                gfx.FillRectangle(brush, rect);
            }
            var bdata = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            unsafe
            {
                // get the type data pointer for raw/double data mode
                float* srcF = isFloat ? sbitmap.GetScanLineAs<float>(sigIdx) : null;
                double *srcD = !isFloat ? sbitmap.GetScanLineAs<double>(sigIdx) : null;

                // Determine min/max values needed for ColorHeatMap
                double minV = double.MaxValue, maxV = double.MinValue;
                // NumSamples equals bitmap width x height
                for (UInt32 i = 0; i < Shape.NumSamples; i++)
                {
                    var val = isFloat ? (double)srcF[i] : srcD[i];
                    if (!(double.IsNaN(val) || double.IsInfinity(val)))
                    {
                        minV = Math.Min(minV, val);
                        maxV = Math.Max(maxV, val);
                    }
                }
                var heatMap = new ColorHeatMap(minV, maxV);

                Int32* dst = (Int32*)bdata.Scan0;
                UInt32 dststep = (UInt32)bdata.Stride / 4;

                // go over the whole bitmap and convert floating-point values to colors
                for (int y = 0; y < h; y++, srcF += w, srcD += w, dst += dststep)
                {
                    for (int x = 0; x < w; x++)
                    {
                        // Get the heat map color for the current value
                        var val = isFloat ? (double)srcF[x] : srcD[x];
                        dst[x] = heatMap.GetColorForValue(val).ToArgb();
                    }
                }
            } // unsafe

            bmp.UnlockBits(bdata);
            return bmp;
        }

        [DllImport("kernel32.dll")]
        private static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, int NumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);

        /// <summary>
        /// Writes 2D bitmap data for a specific shape's signal to BCRF file and returns 
        /// </summary>
        /// <param name="sigIdx">signal index (not signal ID) obtained with SignalIndex() function</param>
        /// <param name="filePath">target file path: if empty, the file name will be generated based on signal ID</param>
        /// <returns>Path to the BCRF file created</returns>
        public string SaveAsBCRF(int sigIdx, string filePath = "")
        {
            if(Shape.Type != CHRLibPlugin.FSS_PluginDataType.Interpolated2D)
                throw new Exception("BCRF file can only be created for 2D interpolated data!");

            var info = Shape.SignalInfos;
            var sigID = info[sigIdx].SignalID;

            if (info[sigIdx].DataType != DataType.Float)
            {
                throw new Exception("Only floating-point data can be saved as a BCRF file!");
            }

            string header =
@"fileformat = {0}
headersize = {1}
xpixels = {2}
ypixels = {3}
xlength = {4}
ylength = {5}
scanspeed = -1000
xunit = mm
yunit = mm
zunit = mm
intelmode = 1
bit2nm = 1
xoffset = 0
yoffset = 0
voidpixels = {6}
zmin = 0
forcecurve = 0
";

            int headerSize = 2048, numVoid = 0;
            var bmp = Shape.Bitmap;
            double sizeX = bmp.X1 - bmp.X0, sizeY = bmp.Y1 - bmp.Y0;

            string fname = filePath.Length > 0 ? filePath : $"{Shape.Label}_signal_{sigID}.bcrf";
            string outs = String.Format(header, "bcrf",
                          headerSize, bmp.Width, bmp.Height, sizeX, sizeY, numVoid);

            using (var file = new FileStream(fname, FileMode.Create, FileAccess.Write))
            {
                var bytes = new ASCIIEncoding().GetBytes(outs);
                var pad = Enumerable.Repeat(Convert.ToByte('%'), headerSize - bytes.Length).ToArray();
                file.Write(bytes, 0, bytes.Length);
                file.Write(pad, 0, pad.Length);

                unsafe
                {
                    var H = file.SafeFileHandle.DangerousGetHandle();
                    // get the typed pointer to signal's data without copying
                    var buf = bmp.GetScanLineAs<float>(sigIdx);
                    WriteFile(H, (IntPtr)buf, (int)(Shape.NumSamples * 4), out int written, IntPtr.Zero);
                    Console.WriteLine($"Total bytes written: {written}");
                }
            }
            return fname;
        }
    }
}
