using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.MultipartMediaFormatter
{
    public class FormData
    {
        private List<ValueFile> _files;
        private List<ValueString> _fields;

        /// <summary>
        /// 
        /// </summary>
        public List<ValueFile> Files
        {
            get
            {
                if (_files == null)
                {
                    _files = new List<ValueFile>();
                }
                return _files;
            }
            set
            {
                _files = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<ValueString> Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = new List<ValueString>();
                }
                return _fields;
            }
            set
            {
                _fields = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllKeys()
        {
            return Fields.Select(m => m.Name).Union(Files.Select(m => m.Name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            Fields.Add(new ValueString() { Name = name, Value = value });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, PostedFile value)
        {
            Files.Add(new ValueFile() { Name = name, Value = value });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string name, out string value)
        {
            ValueString field = Fields.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (field != null)
            {
                value = field.Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string name, out PostedFile value)
        {
            ValueFile field = Files.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (field != null)
            {
                value = field.Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public class ValueString
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ValueFile
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public PostedFile Value { get; set; }
        }
    }
}
