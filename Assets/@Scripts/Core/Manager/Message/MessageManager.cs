using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Message
{
    public sealed class MessageManager
    {
        private sealed class TypedListenerRecord
        {
            public int HandleId;
            public Delegate OriginalHandler;
            public Action<object> Invoke;
        }

        private sealed class TypedAsyncListenerRecord
        {
            public int HandleId;
            public Delegate OriginalHandler;
            public Func<object, Awaitable> Invoke;
            public EAwaitMode Mode;
        }

        private static MessageManager _instance;

        private readonly Dictionary<Type, List<TypedListenerRecord>> _typedListeners =
            new Dictionary<Type, List<TypedListenerRecord>>();

        private readonly Dictionary<Type, List<TypedAsyncListenerRecord>> _typedAsyncListeners =
            new Dictionary<Type, List<TypedAsyncListenerRecord>>();

        private readonly Dictionary<int, Action> _typedUnsubscribeActions =
            new Dictionary<int, Action>();

        private int _nextHandleId = 1;

        public static MessageManager Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        private MessageManager()
        {
            _instance = this;
        }

        public void Publish<TMessage>(TMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "[GameMessageSystem] null message는 publish할 수 없습니다.");
            }

            Type messageType = typeof(TMessage);
            if (!_typedListeners.TryGetValue(messageType, out List<TypedListenerRecord> records) || records.Count == 0)
            {
                return;
            }

            TypedListenerRecord[] snapshot = records.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Invoke(message);
            }
        }

        public async Awaitable PublishAsync<TMessage>(TMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "[GameMessageSystem] null message는 publish할 수 없습니다.");
            }

            Publish(message);

            Type messageType = typeof(TMessage);
            if (!_typedAsyncListeners.TryGetValue(messageType, out List<TypedAsyncListenerRecord> records) || records.Count == 0)
            {
                return;
            }

            TypedAsyncListenerRecord[] snapshot = records.ToArray();
            List<Awaitable> blockingTasks = new List<Awaitable>();
            for (int i = 0; i < snapshot.Length; i++)
            {
                TypedAsyncListenerRecord record = snapshot[i];
                if (record.Mode == EAwaitMode.Blocking)
                {
                    blockingTasks.Add(record.Invoke(message));
                }
                else
                {
                    _ = RunNonBlocking(record.Invoke, message);
                }
            }

            for (int i = 0; i < blockingTasks.Count; i++)
            {
                await blockingTasks[i];
            }
        }

        public SMessageSubscriptionHandle Subscribe<TMessage>(Action<TMessage> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type messageType = typeof(TMessage);
            List<TypedListenerRecord> records = GetOrCreateTypedBucket(messageType);
            for (int i = 0; i < records.Count; i++)
            {
                if (Equals(records[i].OriginalHandler, handler))
                {
                    throw new InvalidOperationException(
                        $"[GameMessageSystem] 중복 구독은 허용되지 않습니다. messageType={messageType.Name}, handler={handler.Method.Name}");
                }
            }

            int handleId = _nextHandleId++;
            TypedListenerRecord record = new TypedListenerRecord
            {
                HandleId = handleId,
                OriginalHandler = handler,
                Invoke = boxed => handler((TMessage)boxed)
            };

            records.Add(record);
            _typedUnsubscribeActions[handleId] = () =>
            {
                if (_typedListeners.TryGetValue(messageType, out List<TypedListenerRecord> typedRecords))
                {
                    typedRecords.RemoveAll(r => r.HandleId == handleId);
                    if (typedRecords.Count == 0)
                    {
                        _typedListeners.Remove(messageType);
                    }
                }
            };

            return new SMessageSubscriptionHandle(handleId);
        }

        public SMessageSubscriptionHandle SubscribeAsync<TMessage>(
            Func<TMessage, Awaitable> handler,
            EAwaitMode mode = EAwaitMode.Blocking)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Type messageType = typeof(TMessage);
            List<TypedAsyncListenerRecord> records = GetOrCreateTypedAsyncBucket(messageType);
            for (int i = 0; i < records.Count; i++)
            {
                if (Equals(records[i].OriginalHandler, handler))
                {
                    throw new InvalidOperationException(
                        $"[GameMessageSystem] 중복 비동기 구독은 허용되지 않습니다. messageType={messageType.Name}, handler={handler.Method.Name}");
                }
            }

            int handleId = _nextHandleId++;
            TypedAsyncListenerRecord record = new TypedAsyncListenerRecord
            {
                HandleId = handleId,
                OriginalHandler = handler,
                Mode = mode,
                Invoke = boxed => handler((TMessage)boxed)
            };

            records.Add(record);
            _typedUnsubscribeActions[handleId] = () =>
            {
                if (_typedAsyncListeners.TryGetValue(messageType, out List<TypedAsyncListenerRecord> typedRecords))
                {
                    typedRecords.RemoveAll(r => r.HandleId == handleId);
                    if (typedRecords.Count == 0)
                    {
                        _typedAsyncListeners.Remove(messageType);
                    }
                }
            };

            return new SMessageSubscriptionHandle(handleId);
        }

        public bool HasSubscribers<TMessage>()
        {
            Type messageType = typeof(TMessage);
            bool hasSync = _typedListeners.TryGetValue(messageType, out List<TypedListenerRecord> syncRecords) &&
                syncRecords != null && syncRecords.Count > 0;
            bool hasAsync = _typedAsyncListeners.TryGetValue(messageType, out List<TypedAsyncListenerRecord> asyncRecords) &&
                asyncRecords != null && asyncRecords.Count > 0;
            return hasSync || hasAsync;
        }

        public void Unsubscribe(SMessageSubscriptionHandle handle)
        {
            if (!handle.IsValid)
            {
                Debug.LogWarning("[GameMessageSystem] 유효하지 않은 handle unsubscribe 요청이 들어왔습니다.");
                return;
            }

            if (_typedUnsubscribeActions.TryGetValue(handle.Value, out Action typedUnsubscribe))
            {
                typedUnsubscribe();
                _typedUnsubscribeActions.Remove(handle.Value);
                return;
            }

            Debug.LogWarning($"[GameMessageSystem] 존재하지 않는 handle unsubscribe 요청이 들어왔습니다. handle={handle.Value}");
        }

        private List<TypedListenerRecord> GetOrCreateTypedBucket(Type messageType)
        {
            if (!_typedListeners.TryGetValue(messageType, out List<TypedListenerRecord> records))
            {
                records = new List<TypedListenerRecord>();
                _typedListeners[messageType] = records;
            }

            return records;
        }

        private List<TypedAsyncListenerRecord> GetOrCreateTypedAsyncBucket(Type messageType)
        {
            if (!_typedAsyncListeners.TryGetValue(messageType, out List<TypedAsyncListenerRecord> records))
            {
                records = new List<TypedAsyncListenerRecord>();
                _typedAsyncListeners[messageType] = records;
            }

            return records;
        }

        private async Awaitable RunNonBlocking(Func<object, Awaitable> handler, object message)
        {
            try
            {
                await handler(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameMessageSystem] NonBlocking handler failed: {e}");
            }
        }
    }
}