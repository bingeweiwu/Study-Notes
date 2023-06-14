using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Drawing;
using System.Drawing.Imaging;

namespace MS.Internal.SulpHur.UICompliance
{
    public static class Utility
    {
        public static byte[] BmpToBytes_MemStream(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Save to memory using the Jpeg format
                bmp.Save(ms, ImageFormat.Jpeg);

                // read to end
                byte[] bmpBytes = ms.GetBuffer();
                bmp.Dispose();
                ms.Close();

                return bmpBytes;
            }
        }

        public static Bitmap BytesToBmp_MemStream(System.Data.Linq.Binary binary)
        {
            byte[] bmpBytes = binary.ToArray();
            using (MemoryStream ms = new MemoryStream(bmpBytes))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                return (Bitmap)img;
            }

        }
    }
}
