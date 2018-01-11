using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using VIV.DemoApp.MVVM;

namespace VIV.DemoApp
{
    public class MainPageViewModel : BindableObject
    {
        public MainPageViewModel()
        {
        }

        public VirtualizingCollection<Message> Messages
        {
            get => GetProperty<VirtualizingCollection<Message>>();
            set => SetProperty(value);
        }

        public ICommand LoadMessagesCommand => new RelayCommand(async () => await LoadMessages());

        private async Task LoadMessages()
        {
            var provider = new MessagesProvider();

            var count = await provider.FetchCountAsync();

            var virtualizedCollection = new VirtualizingCollection<Message>(
                pageSize: 100,
                itemsProvider: provider,
                count: count
                );

            Messages = virtualizedCollection;
        }
    }
}
