using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sinon
{
    public class ApplicationResources
    {

        public static T GetResource<T>(object key)
        {
            object value = AppResources[key];
            return (value == null) ? default(T) : (T)value;
        }

        public static void SetResource(object key, object resource)
        {
            if (AppResources.Contains(key))
                AppResources.Remove(key);

            AppResources.Add(key, resource);
        }

        // Helper methods

        private static ResourceDictionary AppResources
        { 
            get { return Application.Current.Resources; } 
        }
    }
}
