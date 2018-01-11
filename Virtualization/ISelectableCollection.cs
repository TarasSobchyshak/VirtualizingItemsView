using System.Collections.Generic;

namespace Virtualization
{
    public interface ISelectableCollection
    {
        IList<string> SelectedItemsIds { get; set; }
        bool AllItemsSelected { get; set; }
    }
}
