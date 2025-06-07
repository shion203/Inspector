using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyApp.Model;

namespace MyApp.ViewModel
{

    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);

        public void Execute(object parameter) => execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }


    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Model = new MainModel();

            SetModelCommand = new RelayCommand(SetModel, (_) => true);
        }

        private void SetModel(object obj)
        {
            Model = new MainModel();
        }

        public ICommand SetModelCommand { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public MainModel Model
        {
            get { return _mainModel; }
            set { SetProperty(ref _mainModel, value); }
        }
        private MainModel _mainModel;


        public string Hoge
        {
            get { return "Hoge"; }
        }

    }

}
