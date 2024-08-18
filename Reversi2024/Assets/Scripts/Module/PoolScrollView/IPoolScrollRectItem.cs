using System;
using UnityEngine;

namespace KBS.Module.PoolScrollView
{
    public interface IPoolScrollViewItem
    {
        public RectTransform RectTransform { get; }
        
        public GameObject GameObject { get; }

        public void UpdateDisplay(int index);
    }
}

