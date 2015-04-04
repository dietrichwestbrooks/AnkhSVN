﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ankh
{
    public delegate TWrapped WrapItem<TInner, TWrapped>(TInner item);

    public class WrapCollectionWithNotify<TInner, TWrapped> : ReadOnlyCollectionWithNotify<TWrapped>, IDisposable
        where TInner : class
        where TWrapped : class
    {
        readonly IAnkhServiceProvider _context;
        public WrapCollectionWithNotify(IAnkhServiceProvider context, CollectionWithNotify<TInner> collection, WrapItem<TInner, TWrapped> wrapper)
            : base(new WrapInnerCollection(collection, wrapper))
        {
            _context = context;

            ((WrapInnerCollection)this.Items).ResetCollection();
        }

        public WrapCollectionWithNotify(IAnkhServiceProvider context, ReadOnlyCollectionWithNotify<TInner> collection, WrapItem<TInner, TWrapped> wrapper)
            : base(new WrapInnerCollection(collection, wrapper))
        {
            _context = context;

            ((WrapInnerCollection)this.Items).ResetCollection();
        }

        protected IAnkhServiceProvider Context
        {
            get { return _context; }
        }

        protected void Dispose(bool disposing)
        {
            ((WrapInnerCollection)this.Items).Dispose(disposing);
        }

        class WrapInnerCollection : CollectionWithNotify<TWrapped>
        {
            readonly ISupportsCollectionChanged<TInner> _sourceCollection;
            readonly WrapItem<TInner, TWrapped> _wrapper;

            public WrapInnerCollection(ISupportsCollectionChanged<TInner> collection, WrapItem<TInner, TWrapped> wrapper)
            {
                if (collection == null)
                    throw new ArgumentNullException("collection");
                if (wrapper == null)
                    throw new ArgumentNullException("wrapper");

                _sourceCollection = collection;
                _wrapper = wrapper;

                _sourceCollection.CollectionChanged += OnSourceCollectionChanged;
                _sourceCollection.PropertyChanged += OnSourcePropertyChanged;
            }

            internal void Dispose(bool disposing)
            {
                _sourceCollection.CollectionChanged -= OnSourceCollectionChanged;
                _sourceCollection.PropertyChanged -= OnSourcePropertyChanged;
            }

            private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e);
            }

            private void OnSourceCollectionChanged(object sender, CollectionChangedEventArgs<TInner> e)
            {
                int n;
                switch (e.Action)
                {
                    case CollectionChange.Reset:
                        ResetCollection();
                        break;
                    case CollectionChange.Add:
                        n = e.NewStartingIndex;
                        foreach (TInner key in e.NewItems)
                        {
                            Insert(n++, _wrapper(key));
                        }
                        break;
                    case CollectionChange.Remove:
                        n = e.OldStartingIndex;
                        foreach (TInner key in e.OldItems)
                        {
                            RemoveAt(n);
                        }
                        break;
                    case CollectionChange.Replace:
                        this[e.NewStartingIndex] = _wrapper(e.NewItems[0]);
                        break;
                    case CollectionChange.Move:
                        Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            protected internal virtual void ResetCollection()
            {
                using (BatchUpdate())
                {
                    Clear();
                    foreach (TInner key in _sourceCollection)
                    {
                        Add(_wrapper(key));
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
