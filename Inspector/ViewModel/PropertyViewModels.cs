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

        public static PropertyViewModelBase CreateViewModel(string name, object item)
        {
            var itemType = item.GetType();

            if (itemType.IsEnum)
            {
                return new EnumPropertyViewModel(name, item);
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(itemType) && itemType != typeof(string))
            {
                return new CollectionPropertyViewModel(name, item);
            }
            else if (itemType.IsClass && itemType != typeof(string))
            {
                return new ClassPropertyViewModel(name, item);
            }
            else
            {
                return new PropertyViewModelBase(name, item);
            }

            throw new InvalidOperationException();
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

                    Properties.Add(CreateViewModel(prop.Name, value));
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
            // Collectionはプロパティではないので、プロパティネームが取れない
            if (value is System.Collections.IEnumerable enumerable && value.GetType() != typeof(string))
            {
                foreach (var item in enumerable)
                {
                    if (item == null)
                        continue;

                    Properties.Add(CreateViewModel(item.GetType().Name, item));
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

