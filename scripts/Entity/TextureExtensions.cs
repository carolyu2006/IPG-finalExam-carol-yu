using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Template
{
    public static class Texture2DExtensions
    {
        // Creates a horizontally flipped copy of the texture.
        public static Texture2D FlipHorizontally(this Texture2D source)
        {
            int width = source.Width;
            int height = source.Height;

            // Read pixel data
            Color[] srcData = new Color[width * height];
            source.GetData(srcData);

            // Create destination data with flipped columns
            Color[] dstData = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = row + x;
                    int dstIndex = row + (width - 1 - x);
                    dstData[dstIndex] = srcData[srcIndex];
                }
            }

            // Create new texture and set data
            Texture2D flipped = new Texture2D(source.GraphicsDevice, width, height);
            flipped.SetData(dstData);
            return flipped;
        }

        public static Texture2D FlipVertically(this Texture2D source)
        {
            int width = source.Width;
            int height = source.Height;

            // Read pixel data
            Color[] srcData = new Color[width * height];
            source.GetData(srcData);

            // Create destination data with flipped rows
            Color[] dstData = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                int srcRow = y * width;
                int dstRow = (height - 1 - y) * width;
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = srcRow + x;
                    int dstIndex = dstRow + x;
                    dstData[dstIndex] = srcData[srcIndex];
                }
            }

            // Create new texture and set data
            Texture2D flipped = new Texture2D(source.GraphicsDevice, width, height);
            flipped.SetData(dstData);
            return flipped;
        }


        public static Texture2D ChangeColor(this Texture2D source, Color targetColor, Color replacementColor)
        {
            int width = source.Width;
            int height = source.Height;

            Color[] srcData = new Color[width * height];
            source.GetData(srcData);

            for (int i = 0; i < srcData.Length; i++)
            {
                // Compare with a small tolerance (handles rounding)
                if (ApproximatelyEqual(srcData[i], targetColor))
                {
                    srcData[i] = replacementColor;
                }
            }

            Texture2D modified = new Texture2D(source.GraphicsDevice, width, height);
            modified.SetData(srcData);
            return modified;
        }

        private static bool ApproximatelyEqual(Color a, Color b, int tolerance = 5)
        {
            return Math.Abs(a.R - b.R) <= tolerance &&
                   Math.Abs(a.G - b.G) <= tolerance &&
                   Math.Abs(a.B - b.B) <= tolerance &&
                   Math.Abs(a.A - b.A) <= tolerance;
        }

    }
}
