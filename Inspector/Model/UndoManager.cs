using System;
using System.Collections.Generic;



namespace Inspector.Model
{
    // Commandインターフェース
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
    }

    // Undo/Redoを管理するUndoManager
    public class UndoManager
    {
        public bool IsEnabled { get; set; } = true;

        private readonly Stack<IUndoableCommand> _undoStack = new();
        private readonly Stack<IUndoableCommand> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;



        // コマンドを実行し、Undoスタックに追加
        public void Execute(IUndoableCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        // Undo操作
        public void Undo()
        {
            if (!CanUndo) return;
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }

        // Redo操作
        public void Redo()
        {
            if (!CanRedo) return;
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }

        // スタックをクリア
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
        // コマンドを実行せずにUndoスタックに追加
        public void RegisterCommand(IUndoableCommand command)
        {
            if(!IsEnabled) return;
            _undoStack.Push(command);
            _redoStack.Clear();
        }
    }
}
