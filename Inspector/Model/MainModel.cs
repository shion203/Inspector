using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Model
{

    public class HogeClass
    {
        public int HogeInt;
        public float HogeFloat;
        public string HogeString;
        public ObservableCollection<int> HogeIntCollection = new ObservableCollection<int>();
    }

    public class MainModel
    {
        public enum HogeEnum
        {
            One,
            Two,
            Three,
        }

        public int HogeInt { get; set; } = 99;
        public float HogeFloat { get; set; }
        public string HogeString { get; set; }
        public ObservableCollection<int> HogeIntCollection { get; set; } = new ObservableCollection<int>();
        public HogeClass HogeClass { get; set; }
    }

}
