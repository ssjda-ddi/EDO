using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EDO.Core.ViewModel
{
    //Menu used when importing DDI
    public class CheckMenuItemVM :BaseVM
    {
        public static CheckMenuItemVM FindByMenuElem(List<CheckMenuItemVM> menuItems, MenuElem menuElem)
        {
            foreach (CheckMenuItemVM menuItem in menuItems)
            {
                if (menuItem.MenuElem == menuElem)
                {
                    return menuItem;
                }
            }
            return null;
        }

        public static List<CheckMenuItemVM> FindByMenuElems(List<CheckMenuItemVM> menuItems, List<MenuElem> menuElems)
        {
            List<CheckMenuItemVM> results = new List<CheckMenuItemVM>();
            foreach (MenuElem menuElem in menuElems)
            {
                CheckMenuItemVM menuItem = FindByMenuElem(menuItems, menuElem);
                if (menuItem == null) {
                    throw new ApplicationException();
                }
                results.Add(menuItem);
            }
            return results;
        }

        public static int CountCheck(IEnumerable<CheckMenuItemVM> menuItems)
        {
            int checkCount = 0;
            foreach (CheckMenuItemVM childMenuItem in menuItems)
            {
                if ((bool)childMenuItem.IsChecked)
                {
                    checkCount++;
                }
            }
            return checkCount;
        }

        public CheckMenuItemVM(MenuElem elem)
        {
            this.elem = elem;
            this.menuItems = new ObservableCollection<CheckMenuItemVM>();
            this.relatedMenuItems = new List<CheckMenuItemVM>();
            isChecked = true;
        }

        private MenuElem elem;
        public MenuElem MenuElem { get { return elem; } }

        private List<CheckMenuItemVM> relatedMenuItems;
        public List<CheckMenuItemVM> RelatedMenuItems { get { return relatedMenuItems; } }

        public string Title { get { return elem.Title; } }

        private CheckMenuItemVM parentMenuItem;
        public CheckMenuItemVM ParentMenuItem {
            get 
            {
                return parentMenuItem; 
            }
            set
            {
                if (parentMenuItem != value) {
                    //Don't have to notify because it is set only the beginning
                    parentMenuItem = value;
                }
            }
        }

        private ObservableCollection<CheckMenuItemVM> menuItems;
        public ObservableCollection<CheckMenuItemVM> MenuItems { get { return menuItems;  } }

        public void Add(CheckMenuItemVM menuItem)
        {
            menuItem.ParentMenuItem = this;
            menuItems.Add(menuItem);
        }


        private void Log(string msg)
        {
            Debug.WriteLine(msg + " [" + Title + "] IsChecked=" + IsChecked);
        }

        private void Log(string msg, CheckMenuItemVM menuItem)
        {
            Debug.WriteLine(msg + " [" + menuItem.Title + "] IsChecked=" + menuItem.IsChecked);
        }

        private bool isChanging = false;

        private bool isChecked;
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (isChecked != value)
                {
                    //change the self checked-state
                    UpdateSelfCheckedStatus(value);
                    try
                    {
                        //Guard variable is needed to prevent the state change when the relevant menu of children of its own part, the state of their own from becoming back to the original.
                        //Without this guard, category check of Question Scheme, for example,  cannot be off.
                        isChanging = true; 
                        if (elem.IsCategory)
                        {
                            //In the case of category change the child checked-state
                            UpdateChildrenCheckedStatus();
                        }
                        else
                        {
                            UpdateParentCheckedStatus();
                        }
                        UpdateRelatedCheckStatus();
                    }
                    finally
                    {
                        isChanging = false;
                    }
                }
            }
        }


        public void UpdateSelfCheckedStatus(bool isChecked)
        {
            if (isChanging)
            {
                return;
            }
            this.isChecked = isChecked;
            NotifyPropertyChanged("IsChecked");

        }

        private void UpdateRelatedCheckStatus()
        {
            if (isChecked)
            {
                //For example: when Question is clicked, its child "Question Item" is checked.
                //In order to check menues as "Category" and "Code" associated with "Question Item"
                // Call IsChecked=true in this menu(not UpdateSelfCheckedStatus)
                foreach (CheckMenuItemVM relatedMenuItem in relatedMenuItems)
                {
                    relatedMenuItem.IsChecked = isChecked;
                }
            }
        }

        private void UpdateChildrenCheckedStatus()
        {
            //change the child checked-state
            foreach (CheckMenuItemVM childMenuItem in MenuItems)
            {
                childMenuItem.IsChecked = isChecked;
            }
        }

        private void UpdateParentCheckedStatus()
        {
            //change the parent checked-state
            int checkCount = CountCheck(parentMenuItem.MenuItems);
            bool check = checkCount == 0 ? false : true;
            parentMenuItem.UpdateSelfCheckedStatus(check);
        }

    }
}
