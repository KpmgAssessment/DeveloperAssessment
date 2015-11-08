using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.Common
{
    public static class ConfigManager
    {
        public static string GetAppSetting(string key, bool throwOnError = false, bool throwOnNull = false)
        {
            return GetAppSetting(key, string.Empty, throwOnError, throwOnNull);
        }

        public static T GetAppSetting<T>(string key, T defaultValue = default(T), bool throwOnError = false, bool throwOnNull = false, Action<string, T> onNullAction = null, Action<string, T> onValueSetAction = null)
        {
            T value = defaultValue;

            if (!string.IsNullOrEmpty(key))
            {
                // get the value from the config (will be null if it does not exist)
                string valueString = System.Configuration.ConfigurationManager.AppSettings[key];
                try
                {
                    if (valueString != null)
                    {
                        var theType = typeof(T);
                        // handle special types here
                        if (theType.IsEnum)
                        {
                            value = (T)Enum.Parse(theType, valueString, true);
                        }
                        else if (theType.IsAssignableFrom(typeof(Guid)))
                        {
                            value = (T)(object)Guid.Parse(valueString);
                        }
                        else
                        {
                            // treat the type as a standard convertible type
                            value = (T)Convert.ChangeType(valueString, theType);
                        }

                        if (onValueSetAction != null)
                        {
                            // call the value set function passed to us swallowing any exceptions in our own error handling
                            onValueSetAction(key, value);
                        }
                    }
                    else if (onNullAction != null)
                    {
                        // call the function which has been given to be called when the value from the configuration is null
                        // NOTE: exceptions from this method will be caught and reported by us based on the value of throwOnError
                        onNullAction(key, value);
                    }
                }
                catch (Exception x)
                {
                    var ourException = new System.Configuration.ConfigurationErrorsException(string.Format("Error retrieving application setting '{0}'", key), x);
                    if (throwOnError)
                    {
                        throw ourException;
                    }
                }

                // do the null check otherwise the exception will be swallowed up in our own try/catch block
                if (valueString == null && throwOnNull)
                {
                    throw new Exception(string.Format("Missing '{0}' Configuration Setting in Configuration", key));
                }
            }

            return value;
        }

        /// <summary>
        /// Gets a connection string from the application configuration 
        /// </summary>
        /// <param name="name">The connection string to retrieve</param>
        /// <returns>A connection string object which has been loaded from the application configuration</returns>
        public static System.Configuration.ConnectionStringSettings GetConnectionString(string name)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[name];
        }

        /// <summary>
        /// Gets the section from the application configuration file and casts it to the correct type
        /// </summary>
        /// <typeparam name="T">The type of the section to be returned</typeparam>
        /// <param name="sectionName">The name of the section required</param>
        /// <returns>The section cast to the object type specified</returns>
        /// <exception cref="System.Configuration.SettingsPropertyNotFoundException">
        /// Thrown if the section asked for has been omitted from the _requiredCustomConfigSectionNames. 
        /// This list is checked at application start-up to ensure that all configuration settings and sections are present and correct
        /// </exception>
        public static T GetConfigurationSection<T>(string sectionName)
        {
            return (T)System.Configuration.ConfigurationManager.GetSection(sectionName);
        }
    }
}
