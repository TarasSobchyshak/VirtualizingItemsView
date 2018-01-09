using Virtualization;
using VIV.DemoApp.MVVM;

namespace VIV.DemoApp
{
    public class Message : BindableObject, ISelectable
    {
        public string Id { get; set; }
        public bool IsSelected
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
    }
}
