using System;
using System.Collections.Generic;
using Game.Core.Managers;
using Gameplay.GAS;

namespace Game.Messages
{
    public sealed class GameplayMessageManager : BaseManager<GameplayMessageManager>
    {
        private readonly Dictionary<GameplayTag, List<IMessageHandler>> _handlers = new();

        protected override void OnInit()
        {
        }

        protected override void OnDispose()
        {
            _handlers.Clear();
        }

        public IDisposable Subscribe<T>(GameplayTag tag, Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (!_handlers.TryGetValue(tag, out List<IMessageHandler> handlers))
            {
                handlers = new List<IMessageHandler>();
                _handlers.Add(tag, handlers);
            }

            MessageHandler<T> messageHandler = new(handler);
            handlers.Add(messageHandler);
            return new Subscription(this, tag, messageHandler);
        }

        public void Publish<T>(GameplayTag tag, T message)
        {
            if (!_handlers.TryGetValue(tag, out List<IMessageHandler> handlers))
                return;

            IMessageHandler[] snapshot = handlers.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Invoke(message);
            }
        }

        private void Unsubscribe(GameplayTag tag, IMessageHandler handler)
        {
            if (!_handlers.TryGetValue(tag, out List<IMessageHandler> handlers))
                return;

            handlers.Remove(handler);
            if (handlers.Count == 0)
                _handlers.Remove(tag);
        }

        private interface IMessageHandler
        {
            void Invoke<T>(T message);
        }

        private sealed class MessageHandler<T> : IMessageHandler
        {
            private readonly Action<T> _handler;

            public MessageHandler(Action<T> handler)
            {
                _handler = handler;
            }

            public void Invoke<TMessage>(TMessage message)
            {
                if (message is T typedMessage)
                    _handler.Invoke(typedMessage);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly GameplayMessageManager _owner;
            private readonly GameplayTag _tag;
            private readonly IMessageHandler _handler;
            private bool _disposed;

            public Subscription(
                GameplayMessageManager owner,
                GameplayTag tag,
                IMessageHandler handler)
            {
                _owner = owner;
                _tag = tag;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _owner.Unsubscribe(_tag, _handler);
            }
        }
    }
}
