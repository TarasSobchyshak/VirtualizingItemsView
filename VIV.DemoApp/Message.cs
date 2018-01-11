using Virtualization;
using VIV.DemoApp.MVVM;

namespace VIV.DemoApp
{
    public class Message : BindableObject, ISelectable, IVirtualizing
    {
        public string Id
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool IsSelected
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public int Index
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }


        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
    }
}
