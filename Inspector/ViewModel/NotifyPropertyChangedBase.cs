using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MyApp.Model
{
    /// <summary>
    /// INotifyPropertyChangedを実装し、SetPropertyメソッドを持つ基底クラス
    /// </summary>
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName!);
            return true;
        }
    }
}