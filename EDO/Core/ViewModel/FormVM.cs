using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Util;
using EDO.Core.Model;
using EDO.Main;
using EDO.Core.View;

namespace EDO.Core.ViewModel
{
    /// <summary>
    /// Base class of ViewModel corresponding to the input form in the main screen.
    /// assuming StudyUnitVM or GroupVM, inherited EDOUnitVM as a parent.
    /// メインスクリーンの入力フォームに対応するViewModelの基底クラスです。
    /// EDOUnitVMを継承したStudyUnitVMもしくはGroupVMをparentとして想定しています。
    /// </summary>
    public abstract class FormVM :BaseVM, IStatefullVM
    {
        protected VMState state;

        public FormVM(EDOUnitVM edoUnit)
            : base(edoUnit)
        {
            state = null;
        }

        public IValidatable View { get; set; }

        /// <summary>
        /// Viewの切り替え時に実行されます。
        /// </summary>
        public void Reload()
        {
            //It is called when DataContext is changed.
            //Restore the state here
            Reload(state);
            state = null;
        }

        /// <summary>
        /// Viewの切り替え時に実行されます。StateにもとづいてViewの状態を復元します。
        /// </summary>
        /// <param name="state">前回のViewの状態</param>
        protected virtual void Reload(VMState state)
        {
        }

        /// <summary>
        /// Undo、Redoの際に<see cref="UndoInfo"/>に保存されたStateを受け取り、Viewの状態を復元します。
        /// </summary>
        /// <param name="state"></param>
        public void LoadState(VMState state)
        {
            //In the case of FormVM use Reload to return the state, so save the data here.
            this.state = state; 
        }


        protected virtual Action GetCompleteAction(VMState state)
        {
            return null;
        }

        public virtual void Complete(VMState state)
        {
            Action action = GetCompleteAction(state);
            if (action == null)
            {
                return;
            }
            using (UndoTransaction tx = new UndoTransaction(UndoManager, true))
            {
                action();
            }
        }

        public virtual VMState SaveState()
        {
            return null;
        }

        public virtual bool Validate()
        {
            if (this.View == null)
            {
                return true;
            }
            if (!this.View.Validate())
            {
                return false;
            }
            Complete(null); //Execute after GetCompleteAction
            return true;
        }

        public override StudyUnitVM StudyUnit { get { return (StudyUnitVM)Parent; } }
        public GroupVM Group { get { return (GroupVM)Parent; } }
        public virtual void InitRow(object newItem) { }

    }
}
