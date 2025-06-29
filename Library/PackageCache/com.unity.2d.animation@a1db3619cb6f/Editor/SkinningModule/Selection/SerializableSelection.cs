using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.U2D.Animation
{
    [Serializable]
    internal abstract class SerializableSelection<T> : ISelection<T>, ISerializationCallbackReceiver
    {
        internal static readonly int kInvalidID = -1;

        [SerializeField]
        private T[] m_Keys = new T[0];

        private HashSet<T> m_Selection = new HashSet<T>();
        private HashSet<T> m_TemporalSelection = new HashSet<T>();
        private bool m_SelectionInProgress = false;

        public int Count => m_Selection.Count + m_TemporalSelection.Count;

        public T activeElement
        {
            get { return First(); }
            set
            {
                Clear();
                Select(value, true);
            }
        }

        public T[] elements
        {
            get
            {
                HashSet<T> set = m_Selection;

                if (m_SelectionInProgress)
                {
                    HashSet<T> union = new HashSet<T>(m_Selection);
                    union.UnionWith(m_TemporalSelection);
                    set = union;
                }

                return new List<T>(set).ToArray();
            }
            set
            {
                Clear();
                foreach (T element in value)
                    Select(element, true);
            }
        }

        protected abstract T GetInvalidElement();

        public void Clear()
        {
            GetSelection().Clear();
        }

        public void BeginSelection()
        {
            m_SelectionInProgress = true;
            Clear();
        }

        public void EndSelection(bool select)
        {
            m_SelectionInProgress = false;

            if (select)
                m_Selection.UnionWith(m_TemporalSelection);
            else
                m_Selection.ExceptWith(m_TemporalSelection);

            m_TemporalSelection.Clear();
        }

        public void Select(T element, bool select)
        {
            if (EqualityComparer<T>.Default.Equals(element, GetInvalidElement()))
                return;

            if (select)
                GetSelection().Add(element);
            else if (Contains(element))
                GetSelection().Remove(element);
        }

        public bool Contains(T element)
        {
            return m_Selection.Contains(element) || m_TemporalSelection.Contains(element);
        }

        private HashSet<T> GetSelection()
        {
            if (m_SelectionInProgress)
                return m_TemporalSelection;

            return m_Selection;
        }

        private T First()
        {
            T element = First(m_Selection);

            if (EqualityComparer<T>.Default.Equals(element, GetInvalidElement()))
                element = First(m_TemporalSelection);

            return element;
        }

        private T First(HashSet<T> set)
        {
            if (set.Count == 0)
                return GetInvalidElement();

            using (HashSet<T>.Enumerator enumerator = set.GetEnumerator())
            {
                Debug.Assert(enumerator.MoveNext());
                return enumerator.Current;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_Keys = new List<T>(m_Selection).ToArray();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            elements = m_Keys;
        }
    }
}
