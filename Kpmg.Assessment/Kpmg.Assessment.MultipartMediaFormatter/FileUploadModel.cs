using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter
{
    public class FileUploadModel
    {
        /// <summary>
        /// Property maps to an instance of the client file upload
        /// </summary>
        public PostedFile Binary { get; set; }

        /// <summary>
        /// Property maps to the collection of client file uploads
        /// </summary>
        public IList<PostedFile> Binaries { get; set; }

        public string Filename { get; set; }
    }
}
