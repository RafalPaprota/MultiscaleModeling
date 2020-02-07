using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Grains.Utilities.ImageHandler
{
    public static class ImageHandler
    {
        public static void ExportToImage(Canvas canvas, string path)
        {
            var rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width,
           (int)canvas.RenderSize.Height, 96d, 96d, PixelFormats.Default);

            rtb.Render(canvas);

            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, 600, 600));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));

            using (var fs = File.OpenWrite(path))
            {
                pngEncoder.Save(fs);
            }
        }

        public async static Task ImportFromImage(int[,] array, int xDimension, int yDimension, int targetResolution, string path)
        {
            await Task.Run(() =>
            {
                var originalBitmap = new Bitmap(path);
                var bitmap = new Bitmap(originalBitmap, targetResolution, targetResolution);

                int id = 0;
                var ids = new Dictionary<string, int>();

                int x = bitmap.Size.Width;
                int y = bitmap.Size.Height;

                int ratio = targetResolution / xDimension;

                for (int i = 0; i < xDimension; i++)
                {
                    for (int j = 0; j < yDimension; j++)
                    {
                        var pixelColor = bitmap.GetPixel(i * ratio, j * ratio).ToString();

                        if (ids.ContainsKey(pixelColor))
                        {
                            array[i, j] = ids[pixelColor];
                        }
                        else
                        {
                            int newId = ++id;
                            ids.Add(pixelColor, newId);
                            array[i, j] = newId;
                        }
                    }
                }
            });
        }
    }
}
