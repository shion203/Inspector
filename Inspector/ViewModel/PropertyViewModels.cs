using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

// @file 特定の型に対応したViewModelの定義群

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

        public ObservableCollection<PropertyViewModelBase> Properties { get; set; } = new ();
    }

    public class CollectionPropertyViewModel : PropertyViewModelBase
    {
        public ObservableCollection<PropertyViewModelBase> Properties { get; set; } = new ();

        public CollectionPropertyViewModel(string name, object value)
            : base(name, value)
        {
            Properties.Clear();

            if (value is System.Collections.IEnumerable enumerable && value.GetType() != typeof(string))
            {
                foreach (var item in enumerable)
                {
                    if (item == null)
                        continue;

                    var itemType = item.GetType();

                    if (itemType.IsEnum)
                    {
                        Properties.Add(new EnumPropertyViewModel(itemType.Name, item));
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(itemType) && itemType != typeof(string))
                    {
                        Properties.Add(new CollectionPropertyViewModel(itemType.Name, item));
                    }
                    else if (itemType.IsClass && itemType != typeof(string))
                    {
                        Properties.Add(new ClassPropertyViewModel(itemType.Name, item));
                    }
                    else
                    {
                        Properties.Add(new PropertyViewModelBase(itemType.Name, item));
                    }
                }
            }
        }
    }

    public class EnumPropertyViewModel : PropertyViewModelBase
    {
        public ObservableCollection<string> Properties { get; set; } = new();

        public EnumPropertyViewModel(string name, object value)
            : base(name, value)
        {
            if (value != null)
            {
                var enumType = value.GetType();
                if (enumType.IsEnum)
                {
                    foreach (var enumValue in Enum.GetValues(enumType))
                    {
                        Properties.Add(enumValue.ToString()!);
                    }
                }
            }
        }
    }

}

