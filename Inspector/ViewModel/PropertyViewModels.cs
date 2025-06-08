using MyApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

// @file 特定の型に対応したViewModelの定義群

namespace MyApp.ViewModel
{
    /// <summary>
    /// プロパティ名と値を保持する表示用ViewModel。
    /// MyInspectorのPropertiesコレクションの要素として利用される。
    /// </summary>
    public class PropertyViewModelBase : NotifyPropertyChangedBase
    {
        public object Model 
        {
            get
            {
                return model;
            }
            set
            {
                // 別にここでundo対応しても構わない？
                // propertychangedでやる意味は、無いっちゃないかな…
                SetProperty(ref model, value);
            }
        }
        private object model;

        public object? Parent { get; set; }

        public string Name { get; set; }
        public ICommand DeleteItemCommand => new RelayCommand(DeleteItem, CanDeleteItem);

        public PropertyViewModelBase(string name, object value, object? parent)
        {
            model = value;
            Parent = parent;
            Name = name;
        }

        public static PropertyViewModelBase CreateViewModel(string name, object item, object? parent = null)
        {
            var itemType = item.GetType();

            if (itemType.IsEnum)
            {
                return new EnumPropertyViewModel(name, item, parent);
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(itemType) && itemType != typeof(string))
            {
                return new CollectionPropertyViewModel(name, item, parent);
            }
            else if (itemType.IsClass && itemType != typeof(string))
            {
                return new ClassPropertyViewModel(name, item, parent);
            }
            else
            {
                return new PropertyViewModelBase(name, item, parent);
            }

            throw new InvalidOperationException();
        }

        private void DeleteItem()
        {
            if (Parent is System.Collections.IList list)
            {
                list.Remove(Model);
            }
        }

        private bool CanDeleteItem()
        {
            return Parent is System.Collections.IList list && list.Contains(Model);
        }

    }

    /// <summary>
    /// Calss型のプロパティを表すViewModel。
    /// </summary>
    public class ClassPropertyViewModel : PropertyViewModelBase
    {
        public ClassPropertyViewModel(string name, object obj, object? parent = null)
            : base(name, obj, parent)
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

        public CollectionPropertyViewModel(string name, object value, object? parent = null)
            : base(name, value, parent)
        {
            Model = value;

            if (value is System.Collections.IEnumerable enumerable && value.GetType() != typeof(string))
            {
                foreach (var item in enumerable)
                {
                    if (item == null)
                        continue;

                    Properties.Add(CreateViewModel(item.GetType().Name, item, Model));
                }
            }

            // CollectionChangedイベントの購読
            if (value is System.Collections.Specialized.INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += OnCollectionChanged;
            }

            AddItemCommand = new RelayCommand(AddItem, CanAddItem);
        }

        private void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 追加
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem == null) continue;
                    Properties.Add(CreateViewModel(newItem.GetType().Name, newItem, Model));
                }
            }
            // 削除
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    foreach(var item in Properties)
                    {
                        if(item.Model.Equals(oldItem))
                        {
                            Properties.Remove(item);
                            break;
                        }
                    }
                }
            }
            // リセット
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Properties.Clear();
                if (Model is System.Collections.IEnumerable enumerable && Model.GetType() != typeof(string))
                {
                    foreach (var item in enumerable)
                    {
                        if (item == null)
                            continue;
                        Properties.Add(CreateViewModel(item.GetType().Name, item, Model));
                    }
                }
            }
            // 置換
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace && e.NewItems != null && e.OldItems != null)
            {
                int index = e.OldStartingIndex;
                foreach (var oldItem in e.OldItems)
                {
                    var vm = Properties.FirstOrDefault(p => p.Model.Equals(oldItem));
                    if (vm != null)
                        Properties.Remove(vm);
                }
                foreach (var newItem in e.NewItems)
                {
                    if (newItem == null) continue;
                    Properties.Insert(index, CreateViewModel(newItem.GetType().Name, newItem, Model));
                    index++;
                }
            }
            // 移動
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move && e.OldItems != null)
            {
                // 移動前のVMを取得
                var vms = e.OldItems.Cast<object>()
                    .Select(item => Properties.FirstOrDefault(p => p.Model.Equals(item)))
                    .Where(vm => vm != null)
                    .ToList();

                foreach (var vm in vms)
                {
                    Properties.Remove(vm!);
                }
                int insertIndex = e.NewStartingIndex;
                foreach (var vm in vms)
                {
                    Properties.Insert(insertIndex++, vm!);
                }
            }
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
                // CollectionChangedイベントでPropertiesに追加される
            }
        }

        private bool CanAddItem()
        {
            var listType = Model.GetType();
            return listType.GetMethod("Add") != null;
        }

        public ObservableCollection<PropertyViewModelBase> Properties { get; set; } = new();
    }

    public class EnumPropertyViewModel : PropertyViewModelBase
    {
        public ObservableCollection<string> Properties { get; set; } = new();

        public EnumPropertyViewModel(string name, object value, object? parent = null)
            : base(name, value, parent)
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

