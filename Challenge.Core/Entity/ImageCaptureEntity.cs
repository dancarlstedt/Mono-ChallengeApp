using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;

namespace Challenge.Core
{
    public class ImageCaptureEntity
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public byte[] Image { get; set; }
    }
}
