using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter.Converters
{
    public class FormDataToObjectConverter
    {
        private readonly FormData _sourceData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceData"></param>
        public FormDataToObjectConverter(FormData sourceData)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException("No source data found.");
            }

            _sourceData = sourceData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinitionType"></param>
        /// <returns></returns>
        public object Convert(Type destinitionType)
        {
            if (destinitionType == null)
            {
                throw new ArgumentNullException("destinitionType");
            }

            if (destinitionType == typeof(FormData))
            {
                return _sourceData;
            }

            return CreateObject(destinitionType);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinitionType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private object CreateObject(Type destinitionType, string propertyName = "")
        {
            object propValue = null;

            object buffer;
            if (TryGetFromFormData(destinitionType, propertyName, out buffer)
                || TryGetAsGenericDictionary(destinitionType, propertyName, out buffer)
                || TryGetAsGenericListOrArray(destinitionType, propertyName, out buffer)
                || TryGetAsCustomType(destinitionType, propertyName, out buffer))
            {
                propValue = buffer;
            }
            else if (!IsFileOrConvertableFromString(destinitionType))
            {
                //Logger.LogError(propertyName, String.Format("Cannot parse type \"{0}\".", destinitionType.FullName));
            }

            return propValue;
        }

        private bool TryGetFromFormData(Type destinitionType, string propertyName, out object propValue)
        {
            bool existsInFormData = false;
            propValue = null;

            if (destinitionType == typeof(PostedFile))
            {
                PostedFile httpFile;

                if (_sourceData.TryGetValue(propertyName, out httpFile))
                {
                    propValue = httpFile;
                    existsInFormData = true;
                }
            }
            else
            {
                string val;

                if (_sourceData.TryGetValue(propertyName, out val))
                {
                    existsInFormData = true;

                    TypeConverter typeConverter = destinitionType.GetFromStringConverter();

                    if (typeConverter == null)
                    {
                        //Logger.LogError(propertyName, "Cannot find type converter for field - " + propertyName);
                    }
                    else
                    {
                        try
                        {
                            propValue = typeConverter.ConvertFromString(null, CultureInfo.CurrentCulture, val);
                        }
                        catch (Exception ex)
                        {
                            //Logger.LogError(propertyName, String.Format("Error parsing field \"{0}\": {1}", propertyName, ex.Message));
                        }
                    }
                }
            }

            return existsInFormData;
        }

        private bool TryGetAsGenericDictionary(Type destinitionType, string propertyName, out object propValue)
        {
            propValue = null;
            Type keyType, valueType;
            bool isGenericDictionary = IsGenericDictionary(destinitionType, out keyType, out valueType);

            if (isGenericDictionary)
            {
                Type dictType = typeof(Dictionary<,>).MakeGenericType(new[] { keyType, valueType });
                MethodInfo add = dictType.GetMethod("Add");
                dynamic pValue = Activator.CreateInstance(dictType);

                int index = 0;
                string origPropName = propertyName;
                bool isFilled = false;

                while (true)
                {
                    string propertyKeyName = string.Format("{0}[{1}].Key", origPropName, index);
                    object objKey = CreateObject(keyType, propertyKeyName);

                    if (objKey != null)
                    {
                        string propertyValueName = String.Format("{0}[{1}].Value", origPropName, index);
                        object objValue = CreateObject(valueType, propertyValueName);

                        if (objValue != null)
                        {
                            add.Invoke(pValue, new[] { objKey, objValue });
                            isFilled = true;
                        }
                    }
                    else
                    {
                        break;
                    }

                    index++;
                }

                if (isFilled)
                {
                    propValue = pValue;
                }
            }

            return isGenericDictionary;
        }

        private bool TryGetAsGenericListOrArray(Type destinitionType, string propertyName, out object propValue)
        {
            propValue = null;
            Type genericListItemType;
            bool isGenericList = IsGenericListOrArray(destinitionType, out genericListItemType);

            if (isGenericList)
            {
                Type listType = typeof(List<>).MakeGenericType(genericListItemType);
                MethodInfo add = listType.GetMethod("Add");
                dynamic pValue = Activator.CreateInstance(listType);

                int index = 0;
                string origPropName = propertyName;
                bool isFilled = false;

                while (true)
                {
                    propertyName = string.Format("{0}[{1}]", origPropName, index);
                    object objValue = CreateObject(genericListItemType, propertyName);

                    if (objValue != null)
                    {
                        add.Invoke(pValue, new[] { objValue });
                        isFilled = true;
                    }
                    else
                    {
                        break;
                    }

                    index++;
                }

                if (isFilled)
                {
                    if (destinitionType.IsArray)
                    {
                        MethodInfo toArrayMethod = listType.GetMethod("ToArray");
                        propValue = toArrayMethod.Invoke(pValue, new object[0]);
                    }
                    else
                    {
                        propValue = pValue;
                    }
                }
            }

            return isGenericList;
        }

        private bool TryGetAsCustomType(Type destinitionType, string propertyName, out object propValue)
        {
            propValue = null;
            bool isCustomNonEnumerableType = destinitionType.IsCustomNonEnumerableType();

            if (isCustomNonEnumerableType)
            {
                if (string.IsNullOrWhiteSpace(propertyName)
                    || _sourceData.AllKeys().Any(m => m.StartsWith(propertyName + ".", StringComparison.CurrentCultureIgnoreCase)))
                {
                    dynamic obj = Activator.CreateInstance(destinitionType);
                    bool isFilled = false;

                    foreach (PropertyInfo propertyInfo in destinitionType.GetPublicAccessibleProperties())
                    {
                        string propName = (!string.IsNullOrEmpty(propertyName) ? propertyName + "." : "") + propertyInfo.Name;
                        object objValue = CreateObject(propertyInfo.PropertyType, propName);

                        if (objValue != null)
                        {
                            propertyInfo.SetValue(obj, objValue);
                            isFilled = true;
                        }
                    }
                    if (isFilled)
                    {
                        propValue = obj;
                    }
                }
            }

            return isCustomNonEnumerableType;
        }


        private bool IsGenericDictionary(Type type, out Type keyType, out Type valueType)
        {
            keyType = null;
            valueType = null;
            Type iDictType = type.GetInterface(typeof(IDictionary<,>).Name);

            if (iDictType != null)
            {
                Type[] types = iDictType.GetGenericArguments();

                if (types.Length == 2)
                {
                    keyType = types[0];
                    valueType = types[1];

                    return true;
                }
            }

            return false;
        }

        private bool IsGenericListOrArray(Type type, out Type itemType)
        {
            itemType = null;

            if (type.GetInterface(typeof(IDictionary<,>).Name) == null) //not a dictionary
            {
                if (type.IsArray)
                {
                    itemType = type.GetElementType();
                    return true;
                }

                Type iListType = type.GetInterface(typeof(ICollection<>).Name);

                if (iListType != null)
                {
                    Type[] genericArguments = iListType.GetGenericArguments();

                    if (genericArguments.Length == 1)
                    {
                        itemType = genericArguments[0];

                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsFileOrConvertableFromString(Type type)
        {
            if (type == typeof(PostedFile))
            {
                return true;
            }

            return type.GetFromStringConverter() != null;
        }
    }
}
