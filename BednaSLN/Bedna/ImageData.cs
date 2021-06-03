using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Bedna
{
    public class ImageData
    {
        public List<BitmapImage> Images { get; init; }

        public ImageData(string[] files)
        {
            Images = new();

            foreach (string path in files)
            {
                Images.Add(CreateBitMap(path));
            }
        }


        private BitmapImage CreateBitMap(string source)
        {
            BitmapImage image = new();
            using (FileStream stream = File.OpenRead(source))
            {
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }
            return image;
        }
    }
}
