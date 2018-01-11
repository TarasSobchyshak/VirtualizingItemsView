using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VIV.DemoApp
{
    public interface IItemsProvider<T>
    {
        IList<T> GetLoadedItems();

        Task<int> FetchCountAsync();
        Task<IList<T>> FetchRangeAsync(int startIndex, int length);
    }


    public class MessagesProvider : IItemsProvider<Message>
    {
        #region Fields

        private readonly string _includeProperties;
        private IList<Message> _defaultMessages;
        private IList<Message> _loadedMessages;
        private IList<Message> _prevMessages;
        private int _startIndex = 0;
        private int _length = 100;
        private int _count = -1;

        #endregion


        #region Constructors

        public MessagesProvider()
        {
            _loadedMessages = new List<Message>();
            _defaultMessages = new List<Message>();
            _prevMessages = new List<Message>();
        }

        #endregion


        #region IItemsProvider

        public IList<Message> GetLoadedItems()
        {
            if (_loadedMessages?.Any() != true) throw new InvalidOperationException($"'{nameof(FetchRangeAsync)}' method must be invoked first.");

            return _loadedMessages;
        }

        public async Task<int> FetchCountAsync()
        {
            await Task.Delay(1000);
            _count = 10000;
            return _count;
        }

        public async Task<IList<Message>> FetchRangeAsync(int startIndex, int length)
        {
            _startIndex = startIndex;
            _length = length;

            var messages = new List<Message>(length);
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                messages.Add(new Message() { Subject = $"Subject {i}", Id = i.ToString(), Body = "body_asdfasdfkjhdasjkfhkdas", IsSelected = false, Sender = "sender_adjasaskdlasxz" });
            }

            //var messages = await _messageManager.GetAsync(
            //    skip: startIndex,
            //    take: length,
            //    filter: _filter,
            //    includeProperties: _includeProperties,
            //    orderBy: _orderBy
            //    );

            //var mapped = _mapper.Map<IEnumerable<MessageDto>, IEnumerable<Message>>(messages);

            _prevMessages = _loadedMessages;
            _loadedMessages = messages.ToList(); // TODO: don't remove ToList()

            return _loadedMessages;
        }

        #endregion
    }
}
