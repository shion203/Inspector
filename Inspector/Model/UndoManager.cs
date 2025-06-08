using System;
using System.Collections.Generic;



namespace Inspector.Model
{
    // Command�C���^�[�t�F�[�X
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
    }

    // Undo/Redo���Ǘ�����UndoManager
    public class UndoManager
    {
        public bool IsEnabled { get; set; } = true;

        private readonly Stack<IUndoableCommand> _undoStack = new();
        private readonly Stack<IUndoableCommand> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;



        // �R�}���h�����s���AUndo�X�^�b�N�ɒǉ�
        public void Execute(IUndoableCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        // Undo����
        public void Undo()
        {
            if (!CanUndo) return;
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }

        // Redo����
        public void Redo()
        {
            if (!CanRedo) return;
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }

        // �X�^�b�N���N���A
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
        // �R�}���h�����s������Undo�X�^�b�N�ɒǉ�
        public void RegisterCommand(IUndoableCommand command)
        {
            if(!IsEnabled) return;
            _undoStack.Push(command);
            _redoStack.Clear();
        }
    }
}
