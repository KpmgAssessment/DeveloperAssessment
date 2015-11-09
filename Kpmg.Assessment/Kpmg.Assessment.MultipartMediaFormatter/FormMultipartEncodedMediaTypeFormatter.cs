using Kpmg.Assessment.MultipartMediaFormatter.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter
{
    public class FormMultipartEncodedMediaTypeFormatter : MediaTypeFormatter
    {
        private const string _supportedMediaType = "multipart/form-data";

        /// <summary>
        /// 
        /// </summary>
        public FormMultipartEncodedMediaTypeFormatter()
        {
            foreach (string mediaType in _supportedMediaType.Split(','))
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }
        }

        /// <summary>
        /// Validation logic to instruct the formatter of types it can serialize
        /// </summary>
        /// <param name="type"></param>
        /// <returns>bool</returns>
        public override bool CanReadType(Type type)
        {
            return true;
        }

        /// <summary>
        /// Validation logic to instruct the formatter of types it can serialize
        /// </summary>
        /// <param name="type"></param>
        /// <returns>bool</returns>
        public override bool CanWriteType(Type type)
        {
            return true;
        }

        /// <summary>
        /// Ensures that the incoming request is valid
        /// </summary>
        /// <param name="type"></param>
        /// <param name="headers"></param>
        /// <param name="mediaType"></param>
        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);

            //need add boundary
            //(if add when fill SupportedMediaTypes collection in class constructor then receive post with another boundary will not work - Unsupported Media Type exception will thrown)
            if (headers.ContentType == null)
            {
                headers.ContentType = new MediaTypeHeaderValue(_supportedMediaType);
            }

            if (!string.Equals(headers.ContentType.MediaType, _supportedMediaType, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Not a Multipart Content");
            }

            if (headers.ContentType.Parameters.All(m => m.Name != "boundary"))
            {
                headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", "MultipartDataMediaFormatterBoundary1q2w3e"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="readStream"></param>
        /// <param name="content"></param>
        /// <param name="formatterLogger"></param>
        /// <returns></returns>
        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
                                                               IFormatterLogger formatterLogger = null)
        {
            HttpContentToFormDataConverter httpContentToFormDataConverter = new HttpContentToFormDataConverter();
            FormData multipartFormData = await httpContentToFormDataConverter.Convert(content);

            FormDataToObjectConverter dataToObjectConverter = new FormDataToObjectConverter(multipartFormData);
            object result = dataToObjectConverter.Convert(type);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="writeStream"></param>
        /// <param name="content"></param>
        /// <param name="transportContext"></param>
        /// <returns></returns>
        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
                                                TransportContext transportContext)
        {
            if (!content.IsMimeMultipartContent())
            {
                throw new Exception("Not a Multipart Content");
            }

            NameValueHeaderValue boudaryParameter = content.Headers.ContentType.Parameters.FirstOrDefault(m => m.Name == "boundary" && !String.IsNullOrWhiteSpace(m.Value));

            if (boudaryParameter == null)
            {
                throw new Exception("multipart boundary not found");
            }

            ObjectToMultipartDataByteArrayConverter objectToMultipartDataByteArrayConverter = new ObjectToMultipartDataByteArrayConverter();
            byte[] multipartData = objectToMultipartDataByteArrayConverter.Convert(value, boudaryParameter.Value);

            await writeStream.WriteAsync(multipartData, 0, multipartData.Length);

            content.Headers.ContentLength = multipartData.Length;
        }
    }
}
