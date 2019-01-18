using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model;
using EDO.Core.IO;
using System.Diagnostics;
using EDO.Core.ViewModel;

namespace EDO.Core.Util
{
    public class UndoManager
    {
        private List<UndoInfo> infos;
        private int curIndex;
        private EDOModel curModel;
        private IStatefullVM stateProvider;
        public event EventHandler ModelChangeHandler;
        public UndoManager()
        {
            infos = new List<UndoInfo>();
            curIndex = -1;
            curModel = null;
            stateProvider = null;
            IsEnabled = true;
            transactions = new List<UndoTransaction>();
        }

        public bool IsEnabled { get; set; }

        public void Init(EDOModel edoModel, IStatefullVM stateProvider)
        {
            this.curModel = edoModel;
            this.stateProvider = stateProvider;
            curIndex = -1;
            IsEnabled = true;
            infos.Clear();
            transactions.Clear();
            Memorize();
        }

        public int UndoBufferCount { get; set; }

        private bool ShouldMemorize
        {
            get
            {
                if (!IsEnabled)
                {
                    //Won't memorize if disabled
                    return false;
                }
                if (Transactions.Count > 0)
                {
                    return false;
                }
                if (curModel == null)
                {
                    //Can't memorize if current model is null
                    return false;
                }
                if (infos.Count == 0)
                {
                    //Should memory always when the number of memorized model
                    return true;
                }
                ////Otherwise memory only when the current model and the previous model are different
                //if you enter edit mode in the data grid and change the line without changing the value,
                //EndEdit and Memorize would be executed, so it should be avoided
                bool result = EDOSerializer.IsNotSame(infos[curIndex].Model, curModel);
                if (result)
                {
//                    EDOSerializer.SaveDebug("d:/temp/edo1.xml", infos.Last().Model);
//                    EDOSerializer.SaveDebug("d:/temp/edo2.xml", curModel);
                }
                return result;
            }
        }

        private void OnModelChanged()
        {
            if (ModelChangeHandler != null)
            {
                ModelChangeHandler(this, new EventArgs());
            }
        }

        public bool Memorize()
        {
            if (!ShouldMemorize)
            {
                return false;
            }
            //Delete index: current to end
            //If there are 3 and now on index 2, index 3 to 0 -> don't delete
            //If there are 3 and now on index 1, index 2 to 1 -> delete
            int startIndex = curIndex + 1;
            int removeCount = infos.Count - 1 - curIndex;
//            Debug.WriteLine("RemoveRange({0}, {1})", startIndex, removeCount);
            infos.RemoveRange(startIndex, removeCount);
            if (UndoBufferCount > 0 && infos.Count > UndoBufferCount)
            {
                removeCount = infos.Count - UndoBufferCount;
                infos.RemoveRange(0, removeCount);
            }
            //Replication of the model after changing its properties are stored.

            EDOModel newModel = EDOSerializer.Clone(curModel);
            infos.Add(new UndoInfo(newModel, stateProvider.SaveState()));
            curIndex = infos.Count - 1;
//            Debug.WriteLine("model's count=" + models.Count + " curIndex=" + curIndex);
//            Debug.Assert(curIndex == models.Count - 1); // points at the bottom end
            OnModelChanged();
            return true;
        }

        public bool ReplaceMemorize()
        {
            if (!ShouldMemorize)
            {
                return false;
            }
            //replace the model of the last model of the current.
            EDOModel newModel = EDOSerializer.Clone(curModel);
            UndoInfo info = new UndoInfo(newModel, stateProvider.SaveState());
            infos[curIndex] = info;
            OnModelChanged();
            return true;
        }

        public bool CanUndo
        {
            get
            {
                //Valid when there are more than two
                return curIndex > 0;
            }
        }

        public UndoInfo Undo()
        {
            if (!CanUndo)
            {
                return null;
            }
            Debug.Assert(curIndex > 0);
            curIndex--;

            UndoInfo info = infos[curIndex].Copy();
            curModel = info.Model;
            return info;
        }

        public bool CanRedo
        {
            get
            {
                return curIndex <  infos.Count - 1;
            }
        }

        public UndoInfo Redo()
        {
            if (!CanRedo)
            {
                return null;
            }
            Debug.Assert(curIndex < infos.Count - 1);
            curIndex++;
            UndoInfo info = infos[curIndex].Copy();
            curModel = info.Model;
            return info;
        }

        private List<UndoTransaction> transactions;
        public List<UndoTransaction> Transactions 
        {
            get
            {
                return transactions;
            }
        }
    }
}
