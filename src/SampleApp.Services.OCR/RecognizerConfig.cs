using System;
using System.Collections.Generic;
using System.Text;

namespace SampleApp.Services.OCR
{
    public class RecognizerConfig
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public string  ModelUrl { get; set; }
        public string ModelId { get; set; }
    }
}
