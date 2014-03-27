using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sinon
{
    public class Group<T> : ObservableCollection<T>
    {
        public Group(string name, IEnumerable<T> items)
        {
            this.Key = name;
            foreach (T item in items)
            {
                this.Add(item);
            }
        }

        public override bool Equals(object obj)
        {
            Group<T> that = obj as Group<T>;

            return (that != null) && (this.Key.Equals(that.Key));
        }

        public string Key
        {
            get;
            set;
        }

        public bool HasItems
        {
            get { return Items.Count > 0; }
        }
    }
}
