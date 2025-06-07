using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;

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




    /// <summary>
    /// Collection用
    /// </summary>
    public class CollectionPropertyViewModel : PropertyViewModelBase
    {
        public ICommand AddItemCommand { get; }

        public CollectionPropertyViewModel(string name, object value)
            : base(name, value)
        {
            Model = value;

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

            AddItemCommand = new RelayCommand(AddItem, CanAddItem);
        }

        private void AddItem()
        {
            var listType = Model.GetType();
            var addMethod = listType.GetMethod("Add");
            if (addMethod != null)
            {
                var elementType = listType.IsGenericType
                    ? listType.GetGenericArguments()[0]
                    : typeof(object);

                object? newItem = null;
                try
                {
                    newItem = Activator.CreateInstance(elementType);
                }
                catch
                {
                    // インスタンス化できない場合は何もしない
                    return;
                }

                addMethod.Invoke(Model, new object[] { newItem! });

                // @todo:CollectionChanged見て作っ他方が良さそう
                Properties.Add(CreateViewModel(elementType.Name, newItem!));
            }
        }

        private bool CanAddItem()
        {
            var listType = Model.GetType();
            return listType.GetMethod("Add") != null;
        }
        public ObservableCollection<PropertyViewModelBase> Properties { get; set; } = new ();

        private object Model
        {
            get;set;
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

