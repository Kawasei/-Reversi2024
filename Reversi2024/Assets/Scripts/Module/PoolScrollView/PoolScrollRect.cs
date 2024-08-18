using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KBS.Module.PoolScrollView
{
    public class PoolScrollRect : ScrollRect
    {
        [SerializeField] IPoolScrollViewItem itemBase;
        
        [SerializeField] protected RectOffset padding = new RectOffset();
        [SerializeField] private float offset;
        
        private bool isDirty = false;
        private Vector2 itemSize;
        private int itemCount;
        private Func<IPoolScrollViewItem> createItem;
        private List<(int,IPoolScrollViewItem)> showingItems = new List<(int,IPoolScrollViewItem)>();
        private Queue<IPoolScrollViewItem> itemQueue = new Queue<IPoolScrollViewItem>();

        public void Setup(int count, Func<IPoolScrollViewItem> createItem, Vector2 itemSize)
        {
            this.itemSize = itemSize;
            this.createItem = createItem;
            UpdateItemCount(count);
        }

        public void UpdateItemCount(int count)
        {
            itemCount = count;
            UpdateContentsRectSize();
            UpdateShowItems(true);
        }

        protected override void LateUpdate()
        {
            if (Vector2.Distance(Vector2.zero, velocity) >= 0.05f)
            {
                isDirty = true;
            }
            if (isDirty)
            {
                UpdateShowItems();
                isDirty = false;
            }
            base.LateUpdate();
        }

        private void UpdateContentsRectSize()
        {
            Vector2 size = new Vector2(padding.left + padding.right, padding.top + padding.bottom);
            if (vertical)
            {
                size.x += itemSize.x;
                size.y += itemSize.y * itemCount + offset * (itemCount - 1);
            }
            else
            {
                size.x += itemSize.x * itemCount + offset * (itemCount - 1);
                size.y += itemSize.y;
            }

            content.sizeDelta = size;

            isDirty = true;
        }

        private void UpdateShowItems(bool isForce = false)
        {
            var showingIndexes = CalcShowItemIndexes();

            if (isForce)
            {
                showingItems.ForEach(x => PoolItem(x.Item2));
                showingItems.Clear();
            }
            
            for (int i = showingItems.Count - 1; i >= 0; i--)
            {
                var index = showingItems[i].Item1;
                if (showingIndexes.Count(x => x == index) == 0)
                {
                    PoolItem(showingItems[i].Item2);
                    showingItems.RemoveAt(i);
                }
            }

            foreach (var showingIndex in showingIndexes)
            {
                if (showingItems.Count(x => x.Item1 == showingIndex) > 0)
                {
                    continue;
                }
                var pos = CalcItemPosition(showingIndex);
                var item = CreateOrDequeItem();
                item.UpdateDisplay(showingIndex);
                item.RectTransform.localPosition = pos;
                showingItems.Add((showingIndex,item));
            }
        }

        private List<int> CalcShowItemIndexes()
        {
            var res = new List<int>();
            var viewportSize = viewport.rect.size;
            var contentSize = content.sizeDelta;

            var viewingRect = new Rect();
            viewingRect.size = viewportSize;
            viewingRect.x = Mathf.Max(0.0f,(contentSize.x - viewportSize.x)) * (1.0f - normalizedPosition.x);
            viewingRect.y = -1.0f * Mathf.Max(0.0f,(contentSize.y - viewportSize.y)) * (1.0f - normalizedPosition.y); // 下方向にすすむのでy座標は-1倍

            bool OverlapRect(Rect source, Rect target)
            {
                var sourceLeftUp = new Vector2(source.x, source.y);
                var sourceRightDown = new Vector2(source.x + source.size.x, source.y - source.size.y);

                List<Vector2> targetCorners = new List<Vector2>();
                targetCorners.Add(new Vector2(target.x,target.y));
                targetCorners.Add(new Vector2(target.x + target.size.x,target.y));
                targetCorners.Add(new Vector2(target.x + target.size.x,target.y - target.size.y));
                targetCorners.Add(new Vector2(target.x,target.y - target.size.y));
                foreach (var corner in targetCorners)
                {
                    if (sourceLeftUp.x <= corner.x && sourceRightDown.x >= corner.x &&
                        sourceLeftUp.y >= corner.y && sourceRightDown.y <= corner.y)
                    {
                        return true;
                    }  
                }

                return false;
            }
            
            for (int i = 0; i < itemCount; i++)
            {
                var itemRect = new Rect();
                var pos = CalcItemPosition(i);
                itemRect.x = pos.x;
                itemRect.y = pos.y;
                itemRect.size = itemSize;

                if (OverlapRect(viewingRect,itemRect))
                {
                    res.Add(i);
                }
            }

            return res;
        }

        private Vector2 CalcItemPosition(int index)
        {
            Vector2 pos = Vector2.zero;
            if (vertical)
            {
                pos.x = padding.left;
                pos.y = -1.0f * (padding.top + index * (itemSize.y + offset));
            }
            else
            {
                pos.x = padding.left + index * (itemSize.x + offset);
                pos.y = -1.0f * padding.top;
            }

            return pos;
        }

        private IPoolScrollViewItem CreateOrDequeItem()
        {
            IPoolScrollViewItem res;
            if (itemQueue.Count == 0)
            {
                res = this.createItem.Invoke();
                var anchorPivot = new Vector2(0, 1); // AnchorとPivotを固定して座標計算しやすく
                res.RectTransform.anchorMin = anchorPivot;
                res.RectTransform.anchorMax = anchorPivot;
                res.RectTransform.pivot = anchorPivot;
            }
            else
            {
                res = itemQueue.Dequeue();
            }
            res.RectTransform.SetParent(content);
            res.GameObject.SetActive(true);
            return res;
        }

        private void PoolItem(IPoolScrollViewItem item)
        {
            item.GameObject.SetActive(false);
            itemQueue.Enqueue(item);
        }
    }
}