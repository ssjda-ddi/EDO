using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EDO.Core.Util
{
    public class UndoTransaction :IDisposable
    {
        private UndoManager undoManager;

        public UndoTransaction(UndoManager undoManager) :this(undoManager, false)
        {
        }

        public UndoTransaction(UndoManager undoManager, bool newTransaction)
        {
            if (newTransaction)
            {
                undoManager.Transactions.Clear();
            }
            undoManager.Transactions.Add(this);

            this.undoManager = undoManager;
            Debug.Assert(undoManager != null);
            this.undoManager.IsEnabled = false;
        }


        public void Commit()
        {
            //it won't be memorized until the number of transactions get to 0, so delete first
            undoManager.Transactions.Remove(this);
            undoManager.IsEnabled = true;
            undoManager.Memorize();
        }


        public void ReplaceCommit()
        {
            //it won't be memorized until the number of transactions get to 0, so delete first
            undoManager.Transactions.Remove(this);
            undoManager.IsEnabled = true;
            undoManager.ReplaceMemorize();
        }

        public void Dispose()
        {
            // Delete needed here in preperation for the case when Commit() is not done.
            undoManager.Transactions.Remove(this);
            this.undoManager.IsEnabled = true;
        }
    }
}
