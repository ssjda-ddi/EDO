using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.Model
{
    //Interface implemented in VM for saving ID when returning the state of Form
    //used in EDOUtils.Find in Reload
    public interface IStringIDProvider
    {
        string Id { get; }
    }
}
