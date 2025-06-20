using System;

namespace Inspector.Model
{
    // doActionとundoActionを受け取るCommandクラス
    public class UndoableCommand : IUndoableCommand
    {
        private readonly Action _doAction;
        private readonly Action _undoAction;

        public UndoableCommand(Action doAction, Action undoAction)
        {
            _doAction = doAction ?? throw new ArgumentNullException(nameof(doAction));
            _undoAction = undoAction ?? throw new ArgumentNullException(nameof(undoAction));
        }

        public void Execute()
        {
            _doAction();
        }

        public void Undo()
        {
            _undoAction();
        }
    }
}
