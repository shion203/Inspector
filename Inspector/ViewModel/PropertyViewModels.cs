using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MyApp.ViewModel
{
    /// <summary>
    /// プロパティ名と値を保持する表示用ViewModel。
    /// MyInspectorのPropertiesコレクションの要素として利用される。
    /// </summary>
    public class PropertyViewModelBase
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public PropertyViewModelBase(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Calss型のプロパティを表すViewModel。
    /// </summary>
    public class ClassPropertyViewModel : PropertyViewModelBase
    {

    // ...（省略）...

    public ClassPropertyViewModel(string name, object obj)
        : base(name, obj)
    {
        Properties.Clear();

        foreach (PropertyInfo prop in obj.GetType().GetProperties())
        {
            if (prop.CanRead && prop.CanWrite)
            {
                var value = prop.GetValue(obj);

                if (value == null)
                {
                    continue;
                }

                // コレクション型かどうか判定（stringは除外）
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        Properties.Add(new CollectionPropertyViewModel(prop.Name, value));
                    });
                }
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        Properties.Add(new ClassPropertyViewModel(prop.Name, value));
                    });
                }
                else
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        Properties.Add(new PropertyViewModelBase(prop.Name, value));
                    });
                }
            }
        }
    }

        public ObservableCollection<PropertyViewModelBase> Properties { get; set; } = new ObservableCollection<PropertyViewModelBase>();
    }

    public class CollectionPropertyViewModel : PropertyViewModelBase
    {
        public CollectionPropertyViewModel(string name, object value)
            : base(name, value)
        {

        }
    }

    public class EnumPropertyViewModel : PropertyViewModelBase
    {
        public EnumPropertyViewModel(string name, object value)
            : base(name, value)
        {
        }
    }

}

