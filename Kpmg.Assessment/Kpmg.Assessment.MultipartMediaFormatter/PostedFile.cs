using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter
{
    public class PostedFile
    {
        /// <summary>
        /// 
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MediaType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PostedFile() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mediaType"></param>
        /// <param name="buffer"></param>
        public PostedFile(string fileName, string mediaType, byte[] buffer)
        {
            Filename = fileName;
            MediaType = mediaType;
            Buffer = buffer;
        }
    }
}
