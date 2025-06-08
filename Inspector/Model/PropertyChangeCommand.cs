using Inspector.Model;

public class PropertyChangeCommand<T> : IUndoableCommand
{
    private readonly Action<T> _setter;
    private readonly T _oldValue;
    private readonly T _newValue;

    public PropertyChangeCommand(Action<T> setter, T oldValue, T newValue)
    {
        _setter = setter;
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public void Execute() => _setter(_newValue);
    public void Undo() => _setter(_oldValue);
}