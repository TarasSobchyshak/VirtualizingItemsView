using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Virtualization;
using VIV.DemoApp.MVVM;

namespace VIV.DemoApp
{
    public class MainPageViewModel : BindableObject
    {
        public MainPageViewModel()
        {
            Messages = new ObservableCollection<Message>();
        }

        public ObservableCollection<Message> Messages
        {
            get => GetProperty<ObservableCollection<Message>>();
            set => SetProperty(value);
        }

        public ICommand LoadMessagesCommand => new RelayCommand(LoadMessages);

        private void LoadMessages()
        {
            var count = 1000;
            var items = new List<Message>(count);
            var rand = new Random();

            for (int i = 0; i < count; ++i)
            {
                items.Add(new Message
                {
                    Body = "asfasdf",
                    Id = rand.Next(0, 1_000_000).ToString(),
                    Sender = "asdfdasfdasfsafds",
                    Subject = "gkasjdgksssdgadasdgssagsdgasgs",
                    IsSelected = (i & 1) == 1
                });
            }

            Messages = new ObservableCollection<Message>(items);
        }
    }
}
