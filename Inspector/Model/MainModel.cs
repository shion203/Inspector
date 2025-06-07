using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Model
{
    public class HogeClass2
    {
        public int HogeInt { get; set; }
        public float HogeFloat { get; set; }
        public string HogeString { get; set; } = "HogeString";

        public ObservableCollection<int> HogeIntCollection { get; set; } = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

    }

    /// <summary>
    /// ルートにぶら下がっているクラスプロパティ
    /// </summary>
    public class HogeClass
    {
        public int HogeInt { get; set; }
        public float HogeFloat { get; set; }
        public string HogeString { get; set; } = "HogeString";

        public ObservableCollection<int> HogeIntCollection { get; set; } = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

        public HogeClass2 Hoge2 { get; set; } = new HogeClass2
        {
            HogeInt = 100,
            HogeFloat = 1.5f,
            HogeString = "Hoge2String",
            HogeIntCollection = new ObservableCollection<int> { 1, 2, 3 }
        };

        /// <summary>
        /// @todo:クラスの入れ子は無限ループになっちゃうので対処必要そう
        /// </summary>
        /*
        public HogeClass Hoge2 { get; set; } = new HogeClass
        {
            HogeInt = 100,
            HogeFloat = 1.5f,
            HogeString = "Hoge2String",
            HogeIntCollection = new ObservableCollection<int> { 1, 2, 3 }
        };
        */
    }

    /// <summary>
    /// Inspectorで表示する対象
    /// </summary>
    public class MainModel
    {
        public enum HogeEnum
        {
            One,
            Two,
            Three,
        }

        // 様々な型
        public int HogeInt { get; set; } = 99;

        public float HogeFloat { get; set; }

        public string HogeString { get; set; } = "HogeString";

        public HogeEnum HogeEnumValue { get; set; } = HogeEnum.Two;

        // Collectionのケース
        public ObservableCollection<int> HogeIntCollection { get; set; } = new ObservableCollection<int> { 1, 2, 3, 4, 5 };

        /// <summary>
        /// クラスのケース
        /// </summary>
        public HogeClass HogeClass { get; set; } = new HogeClass { HogeInt = 100, HogeFloat = 1.5f, HogeString = "Hoge2String", HogeIntCollection = new ObservableCollection<int> { 1, 2, 3 } };
    }

}
