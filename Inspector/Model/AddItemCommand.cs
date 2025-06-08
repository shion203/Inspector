using Inspector.Model;

public class AddItemCommand<T> : IUndoableCommand
{
    private readonly IList<T> _list;
    private readonly T _item;

    public AddItemCommand(IList<T> list, T item)
    {
        _list = list;
        _item = item;
    }

    public void Execute() => _list.Add(_item);
    public void Undo() => _list.Remove(_item);
}