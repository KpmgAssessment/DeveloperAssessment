using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter.Converters
{
    public class ObjectToMultipartDataByteArrayConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        public byte[] Convert(object value, string boundary)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new ArgumentNullException("boundary");
            }

            List<KeyValuePair<string, object>> propertiesList = ConvertObjectToFlatPropertiesList(value);

            return GetMultipartFormDataBytes(propertiesList, boundary);
        }

        private List<KeyValuePair<string, object>> ConvertObjectToFlatPropertiesList(object value)
        {
            List<KeyValuePair<string, object>> propertiesList = new List<KeyValuePair<string, object>>();

            if (value is FormData)
            {
                FillFlatPropertiesListFromFormData((FormData)value, propertiesList);
            }
            else
            {
                FillFlatPropertiesListFromObject(value, "", propertiesList);
            }

            return propertiesList;
        }

        private void FillFlatPropertiesListFromFormData(FormData formData, List<KeyValuePair<string, object>> propertiesList)
        {
            foreach (var field in formData.Fields)
            {
                propertiesList.Add(new KeyValuePair<string, object>(field.Name, field.Value));
            }

            foreach (var field in formData.Files)
            {
                propertiesList.Add(new KeyValuePair<string, object>(field.Name, field.Value));
            }
        }

        private void FillFlatPropertiesListFromObject(object obj, string prefix, List<KeyValuePair<string, object>> propertiesList)
        {
            if (obj != null)
            {
                Type type = obj.GetType();

                if (obj is IDictionary)
                {
                    IDictionary dict = obj as IDictionary;
                    int index = 0;

                    foreach (ICollection key in dict.Keys)
                    {
                        string indexedKeyPropName = string.Format("{0}[{1}].Key", prefix, index);
                        FillFlatPropertiesListFromObject(key, indexedKeyPropName, propertiesList);

                        string indexedValuePropName = string.Format("{0}[{1}].Value", prefix, index);
                        FillFlatPropertiesListFromObject(dict[key], indexedValuePropName, propertiesList);

                        index++;
                    }
                }
                else if (obj is ICollection)
                {
                    ICollection list = obj as ICollection;
                    int index = 0;

                    foreach (ICollection indexedPropValue in list)
                    {
                        string indexedPropName = String.Format("{0}[{1}]", prefix, index);
                        FillFlatPropertiesListFromObject(indexedPropValue, indexedPropName, propertiesList);

                        index++;
                    }
                }
                else if (type.IsCustomNonEnumerableType())
                {
                    foreach (var propertyInfo in type.GetPublicAccessibleProperties())
                    {
                        string propName = string.IsNullOrWhiteSpace(prefix)
                                              ? propertyInfo.Name
                                              : string.Format("{0}.{1}", prefix, propertyInfo.Name);

                        object propValue = propertyInfo.GetValue(obj);

                        FillFlatPropertiesListFromObject(propValue, propName, propertiesList);
                    }
                }
                else
                {
                    propertiesList.Add(new KeyValuePair<string, object>(prefix, obj));
                }
            }
        }

        private byte[] GetMultipartFormDataBytes(List<KeyValuePair<string, object>> postParameters, string boundary)
        {
            Encoding encoding = Encoding.UTF8;

            using (MemoryStream formDataStream = new System.IO.MemoryStream())
            {
                bool needsCLRF = false;

                foreach (var param in postParameters)
                {
                    // Add a CRLF to allow multiple parameters to be added.
                    // Skip it on the first parameter, add it to subsequent parameters.
                    if (needsCLRF)
                    {
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                    }

                    needsCLRF = true;

                    if (param.Value is PostedFile)
                    {
                        PostedFile httpFileToUpload = (PostedFile)param.Value;

                        // Add just the first part of this param, since we will write the file data directly to the Stream
                        string header =
                            string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                                boundary,
                                param.Key,
                                httpFileToUpload.Filename ?? param.Key,
                                httpFileToUpload.MediaType ?? "application/octet-stream");

                        formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        // Write the file data directly to the Stream, rather than serializing it to a string.
                        formDataStream.Write(httpFileToUpload.Buffer, 0, httpFileToUpload.Buffer.Length);
                    }
                    else
                    {
                        string objString = "";

                        if (param.Value != null)
                        {
                            TypeConverter typeConverter = param.Value.GetType().GetToStringConverter();

                            if (typeConverter != null)
                            {
                                objString = typeConverter.ConvertToString(null, CultureInfo.CurrentCulture, param.Value);
                            }
                            else
                            {
                                throw new Exception(String.Format("Type \"{0}\" cannot be converted to string", param.Value.GetType().FullName));
                            }
                        }

                        string postData =
                            string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                                          boundary,
                                          param.Key,
                                          objString);

                        formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                    }
                }

                // Add the end of the request.  Start with a newline
                string footer = "\r\n--" + boundary + "--\r\n";
                formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

                byte[] formData = formDataStream.ToArray();

                return formData;
            }
        }
    }
}
