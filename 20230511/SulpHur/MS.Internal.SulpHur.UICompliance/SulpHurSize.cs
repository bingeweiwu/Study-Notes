using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MS.Internal.SulpHur.UICompliance
{
    [DataContract]
    public class SulpHurSize
    {
        public SulpHurSize() { }

        public SulpHurSize(System.Drawing.Size s)
        {
            this.Width = s.Width;
            this.Height = s.Height;
        }

        public SulpHurSize(int width, int height) {
            this.Width = width;
            this.Height = height;
        }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }
    }
}
