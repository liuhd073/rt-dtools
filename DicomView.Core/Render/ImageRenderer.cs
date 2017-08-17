﻿using DicomPanel.Core.Radiotherapy.Imaging;
using DicomPanel.Core.Utilities;
using DicomPanel.Core.Utilities.RTMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomPanel.Core.Render
{
    public class ImageRenderer
    {
        public void Render(DicomImageObject image, Camera camera, IRenderContext context, Recti screenRect)
        {
            if (image == null || image.Grid == null || context == null)
                return;

            //Translates screen to world points and vice versa
            var t = camera.CreateTranslator(context);

            // This render function loops through each pixel in the camera's FOV,
            // converts the location to a location in the patient coordinate system,
            // then store in a byte array to render to the image layer.
            int rows = screenRect.Height;
            int cols = screenRect.Width;

            Point3d worldCoords = new Point3d();

            int bytespp = 4;
            byte[] bytearray = new byte[rows * cols * bytespp];
            int k = 0;

            Point3d initPosn = t.ConvertScreenToWorldCoords(screenRect.Y, screenRect.X);
            Point3d posn = t.ConvertScreenToWorldCoords(screenRect.Y, screenRect.X);
            // Get the direction we move in each iteration from the top left of the camera
            // Use doubles rather than Vector<doubles> because the addition is faster.
            double ix = initPosn.X;
            double iy = initPosn.Y;
            double iz = initPosn.Z;
            double px = initPosn.X;
            double py = initPosn.Y;
            double pz = initPosn.Z;
            double cx = camera.ColDir.X / camera.Scale * camera.MMPerPixel;
            double cy = camera.ColDir.Y / camera.Scale * camera.MMPerPixel;
            double cz = camera.ColDir.Z / camera.Scale * camera.MMPerPixel;
            double rx = camera.RowDir.X / camera.Scale * camera.MMPerPixel;
            double ry = camera.RowDir.Y / camera.Scale * camera.MMPerPixel;
            double rz = camera.RowDir.Z / camera.Scale * camera.MMPerPixel;
            int maxImagePixel = 255;
            for (int r = 0; r < rows; r += 1)
            {
                px = ix + rx * r; py = iy + ry * r; pz = iz + rz * r;
                for (int c = 0; c < cols; c += 1)
                {
                    ushort value = (ushort)(ComputePixel((image.Grid.Interpolate(px, py, pz).Value), maxImagePixel, image.Window, image.Level));

                    bytearray[k] = (byte)(value);
                    bytearray[k + 1] = (byte)value;
                    bytearray[k + 2] = (byte)value;
                    //bytearray[k + 3] = 128;
                    k += bytespp;
                    px += cx; py += cy; pz += cz;
                }
            }
            if (bytearray.Length > 0)
            {
                context.FillPixels(bytearray, screenRect);
            }

            bytearray = null;
        }

        private float ComputePixel(float gray, float maxPixel, int Window, int Level)
        {
            if(gray > 0)
            {

            }
            if (gray < (Level - Window / 2))
                gray = 0;
            else if (gray > Level + Window / 2)
                gray = maxPixel;
            else
                gray = (float)(((double)(gray - (Level - Window / 2)) / (double)(Window)) * maxPixel);
            if (gray < 0)
                gray = 0;
            if (gray > maxPixel)
                gray = maxPixel;
            return gray;
        }
    }
}
