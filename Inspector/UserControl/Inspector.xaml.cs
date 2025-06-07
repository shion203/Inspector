using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace MyApp
{
    /// <summary>
    /// 任意のオブジェクトのプロパティ一覧を表示するWPF用ユーザーコントロール。
    /// 
    /// TargetObjectプロパティに任意のオブジェクトをセットすると、
    /// そのオブジェクトの読み書き可能なプロパティを自動的に列挙し、
    /// PropertiesコレクションにPropertyViewModelとして格納する。
    /// 
    /// Propertiesコレクションはバインディング等でUIに表示できる。
    /// </summary>
    public partial class MyInspector : UserControl
    {
        /// <summary>
        /// 表示対象オブジェクトのプロパティ情報を格納するコレクション。
        /// 各要素はPropertyViewModelとしてプロパティ名と値を保持する。
        /// </summary>
        public ObservableCollection<PropertyViewModelBase> Properties { get; } = new ObservableCollection<PropertyViewModelBase>();

        /// <summary>
        /// プロパティ一覧を表示する対象のオブジェクト。
        /// 設定時に自動的にプロパティ情報を読み込み、Propertiesコレクションを更新する。
        /// </summary>
        public object TargetObject
        {
            get 
            {
                return (object)GetValue(TargetObjectProperty); 
            }
            set 
            {
                SetValue(TargetObjectProperty, value); 
                LoadProperties(value); 
            }
        }

        /// <summary>
        /// TargetObject依存関係プロパティの定義。
        /// オブジェクトが変更された際にプロパティ一覧を再読み込みする。
        /// </summary>
        public static readonly DependencyProperty TargetObjectProperty =
            DependencyProperty.Register(nameof(TargetObject), typeof(object), typeof(MyInspector),
                new PropertyMetadata(null, OnTargetObjectChanged));

        /// <summary>
        /// コンストラクタ。コントロールの初期化を行う。
        /// </summary>
        public MyInspector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// TargetObjectプロパティが変更されたときに呼ばれるコールバック。
        /// 新しいオブジェクトのプロパティ一覧を読み込む。
        /// </summary>
        /// <param name="d">依存関係オブジェクト</param>
        /// <param name="e">変更イベント引数</param>
        private static void OnTargetObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MyInspector control && e.NewValue != null)
            {
                control.LoadProperties(e.NewValue);
            }
        }

        /// <summary>
        /// 指定したオブジェクトの読み書き可能なプロパティを列挙し、Propertiesコレクションに追加する。
        /// </summary>
        /// <param name="obj">プロパティ一覧を取得する対象オブジェクト</param>
        private void LoadProperties(object obj)
        {
            Properties.Clear();

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(obj);

                    // コレクション型かどうか判定（stringは除外）
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                    {
                        Properties.Add(new CollectionPropertyViewModel(prop.Name, value));
                    }
                    else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    {
                        Properties.Add(new ClassPropertyViewModel(prop.Name, value));
                    }
                    else
                    {
                        Properties.Add(new PropertyViewModelBase(prop.Name, value));
                    }
                }
            }
        }
    }

    public class ClassPropertyViewModel : PropertyViewModelBase
    {
        public ClassPropertyViewModel(string name, object value)
            :base(name, value)    
        {

        }
    }

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
    public class CollectionPropertyViewModel : PropertyViewModelBase
    {
        public CollectionPropertyViewModel(string name, object value)
            : base(name, value)
        {

        }
    }
}
