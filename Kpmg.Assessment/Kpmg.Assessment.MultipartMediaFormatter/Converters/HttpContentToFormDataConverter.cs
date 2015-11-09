using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter.Converters
{
    public class HttpContentToFormDataConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<FormData> Convert(HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("No content posted with request.");
            }

            if (!content.IsMimeMultipartContent())
            {
                throw new Exception("Unsupported Media Type");
            }

            MultipartMemoryStreamProvider multipartProvider = await content.ReadAsMultipartAsync();

            return await Convert(multipartProvider);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multipartProvider"></param>
        /// <returns></returns>
        public async Task<FormData> Convert(MultipartMemoryStreamProvider multipartProvider)
        {
            int index = 0;
            FormData multipartFormData = new FormData();
            bool isMultipleUploads = multipartProvider.Contents.Where(x => IsFile(x.Headers.ContentDisposition)).Count() > 1;
            foreach (HttpContent file in multipartProvider.Contents.Where(x => IsFile(x.Headers.ContentDisposition)))
            {
                string name = UnquoteToken(file.Headers.ContentDisposition.Name);
                string fileName = FixFilename(file.Headers.ContentDisposition.FileName);
                //string mediaType = file.Headers.ContentType.MediaType;
                string mediaType = string.Empty;
                using (Stream stream = await file.ReadAsStreamAsync())
                {
                    byte[] buffer = ReadAllBytes(stream);

                    if (buffer.Length > 0)
                    {
                        if (isMultipleUploads)
                        {
                            name = string.Join("", new string[] { name, "[", index.ToString(), "]" });
                        }

                        multipartFormData.Add(name, new PostedFile(fileName, mediaType, buffer));
                    }
                }
                index++;
            }

            foreach (HttpContent part in multipartProvider.Contents.Where(x => x.Headers.ContentDisposition.DispositionType == "form-data"
                                                                  && !IsFile(x.Headers.ContentDisposition)))
            {
                string name = UnquoteToken(part.Headers.ContentDisposition.Name);
                string data = await part.ReadAsStringAsync();
                multipartFormData.Add(name, data);
            }

            return multipartFormData;
        }

        private bool IsFile(ContentDispositionHeaderValue disposition)
        {
            return !string.IsNullOrEmpty(disposition.FileName);
        }

        /// <summary>
        /// Remove bounding quotes on a token if present
        /// </summary>
        private static string UnquoteToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }

        /// <summary>
        /// Amend filenames to remove surrounding quotes and remove path from IE
        /// </summary>
        private static string FixFilename(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                return string.Empty;
            }

            string result = originalFileName.Trim();

            // remove leading and trailing quotes
            result = result.Trim('"');

            // remove full path versions
            if (result.Contains("\\"))
            {
                result = Path.GetFileName(result);
            }

            return result;
        }

        private byte[] ReadAllBytes(Stream input)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                input.CopyTo(stream);
                return stream.ToArray();
            }
        }
    }
}
