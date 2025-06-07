using MyApp.ViewModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        public MyInspector()
        {
            InitializeComponent();
            this.PreviewMouseWheel += MyInspector_PreviewMouseWheel;
        }

        private void MyInspector_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 親のScrollViewerを探してスクロールさせる
            var parent = this.Parent;
            while (parent != null && !(parent is ScrollViewer))
            {
                if (parent is FrameworkElement fe)
                    parent = fe.Parent;
                else
                    break;
            }

            if (parent is ScrollViewer scrollViewer)
            {
                if (e.Delta < 0)
                    scrollViewer.LineDown();
                else
                    scrollViewer.LineUp();

                e.Handled = true;
            }
        }

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
            var propertyViewModels = CreatePropertyViewModels(obj);
            foreach (var vm in propertyViewModels)
            {
                Properties.Add(vm);
            }
        }

        /// <summary>
        /// 指定オブジェクトのプロパティを適切なViewModelに変換してリストで返す（static版）。
        /// </summary>
        /// <param name="obj">プロパティ一覧を取得する対象オブジェクト</param>
        /// <returns>PropertyViewModelBaseのリスト</returns>
        public static List<PropertyViewModelBase> CreatePropertyViewModels(object obj)
        {
            var list = new List<PropertyViewModelBase>();
            if (obj == null) return list;

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(obj);

                    if (value == null)
                    {
                        continue;
                    }

                    list.Add(PropertyViewModelBase.CreateViewModel(prop.Name, value));
                }
            }
            return list;
        }

    }

}
