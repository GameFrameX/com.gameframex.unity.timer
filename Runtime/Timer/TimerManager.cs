using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using UnityEngine;

namespace GameFrameX.Timer.Runtime
{
    /// <summary>
    /// 定时器管理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class TimerManager : IGameFrameworkModule, ITimerManager, IUpdatableModule, IShutdownableModule
    {
        class TimerItem
        {
            public int Id;
            public float Interval;
            public int Repeat;
            public Action<object> Callback;
            public object Param;

            public float Elapsed;
            public bool Deleted;
            public bool IsPaused;
            public float PausedElapsed;
            public TimerTimeScale TimeScaleMode;
            public string Tag;
            public Action OnComplete;

            public void Set(float interval, int repeat, Action<object> callback, object param, string tag, TimerTimeScale timeScale, Action onComplete)
            {
                this.Interval = interval;
                this.Repeat = repeat;
                this.Callback = callback;
                this.Param = param;
                this.Tag = tag;
                this.TimeScaleMode = timeScale;
                this.OnComplete = onComplete;
            }
        }

        private readonly Dictionary<Action<object>, TimerItem> _items = new Dictionary<Action<object>, TimerItem>();
        private readonly Dictionary<Action<object>, TimerItem> _toAdd = new Dictionary<Action<object>, TimerItem>();
        private readonly List<TimerItem> _toRemove = new List<TimerItem>();
        private readonly Stack<TimerItem> _pool = new Stack<TimerItem>(100);
        private readonly List<(Action<object> Callback, object Param)> _pendingCallbacks = new List<(Action<object>, object)>();
        private readonly Dictionary<int, Action<object>> _ids = new Dictionary<int, Action<object>>();
        private readonly Dictionary<string, List<int>> _tags = new Dictionary<string, List<int>>();
        private int _nextId = 1;

        private static readonly object Locker = new object();

        /// <summary>
        /// 是否触发回调异常
        /// </summary>
        public static bool CatchCallbackExceptions = false;

        /// <summary>
        /// 初始化定时器管理器并消费 <see cref="TimerOptions"/> 启动配置（C9 配置终态：模块首次实例化时一次性消费）。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public TimerManager()
        {
            if (GameFrameXOptions.TryGet<TimerOptions>(out var timerOptions))
            {
                CatchCallbackExceptions = timerOptions.CatchCallbackExceptions;
            }
        }

        [UnityEngine.Scripting.Preserve]
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (Locker)
            {
                // 先合并本帧新增的定时器，使其从加入后的第一次 Update 即开始累计（与包内单测契约一致）；
                // 回调在锁外执行，回调内再 Add 走 _toAdd 延迟到下一帧，无重入风险。
                if (_toAdd.Count > 0)
                {
                    var addEnumerator = _toAdd.GetEnumerator();
                    while (addEnumerator.MoveNext())
                    {
                        _items.Add(addEnumerator.Current.Key, addEnumerator.Current.Value);
                    }

                    addEnumerator.Dispose();
                    _toAdd.Clear();
                }

                if (_items.Count > 0)
                {
                    var enumerator = _items.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var timerItem = enumerator.Current.Value;
                        if (timerItem.Deleted)
                        {
                            _toRemove.Add(timerItem);
                            continue;
                        }

                        // Paused timer: skip elapsed accumulation but remain in _items
                        if (timerItem.IsPaused)
                        {
                            continue;
                        }

                        // Select time source based on TimeScaleMode
                        float delta = timerItem.TimeScaleMode == TimerTimeScale.Scaled ? elapseSeconds : realElapseSeconds;

                        // Per-frame timer (interval <= 0): always fire, skip elapsed accumulation
                        if (timerItem.Interval <= 0f)
                        {
                            if (timerItem.Repeat > 0)
                            {
                                timerItem.Repeat--;
                                if (timerItem.Repeat == 0)
                                {
                                    timerItem.Deleted = true;
                                    _toRemove.Add(timerItem);
                                }
                            }

                            if (timerItem.Callback != null)
                            {
                                _pendingCallbacks.Add((timerItem.Callback, timerItem.Param));
                            }

                            continue;
                        }

                        timerItem.Elapsed += delta;
                        if (timerItem.Elapsed < timerItem.Interval)
                        {
                            continue;
                        }

                        timerItem.Elapsed -= timerItem.Interval;
                        if (timerItem.Elapsed < 0 || timerItem.Elapsed > 0.03f)
                            timerItem.Elapsed = 0;

                        if (timerItem.Repeat > 0)
                        {
                            timerItem.Repeat--;
                            if (timerItem.Repeat == 0)
                            {
                                timerItem.Deleted = true;
                                _toRemove.Add(timerItem);
                            }
                        }

                        if (timerItem.Callback != null)
                        {
                            _pendingCallbacks.Add((timerItem.Callback, timerItem.Param));
                        }
                    }

                    enumerator.Dispose();
                }

                int len = _toRemove.Count;
                if (len > 0)
                {
                    for (int k = 0; k < len; k++)
                    {
                        TimerItem i = _toRemove[k];
                        if (i.Deleted && i.Callback != null)
                        {
                            i.OnComplete?.Invoke();
                            _items.Remove(i.Callback);
                            _ids.Remove(i.Id);
                            RemoveFromTagIndex(i);
                            ReturnToPool(i);
                        }
                    }

                    _toRemove.Clear();
                }

            }

            // Execute callbacks outside lock to avoid blocking other threads
            if (_pendingCallbacks.Count > 0)
            {
                for (int i = 0; i < _pendingCallbacks.Count; i++)
                {
                    var (callback, param) = _pendingCallbacks[i];
                    if (CatchCallbackExceptions)
                    {
                        try
                        {
                            callback(param);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Timer callback error > " + e.Message);
                        }
                    }
                    else
                    {
                        callback(param);
                    }
                }

                _pendingCallbacks.Clear();
            }
        }

        [UnityEngine.Scripting.Preserve]
        public void Shutdown()
        {
            lock (Locker)
            {
                this._toRemove.Clear();
                this._toAdd.Clear();
                this._items.Clear();
                this._ids.Clear();
                this._tags.Clear();
                this._pendingCallbacks.Clear();
            }
        }

        /// <summary>
        /// 添加一个定时调用的任务
        /// </summary>
        /// <param name="interval">间隔时间（以秒为单位）</param>
        /// <param name="repeat">重复次数（0 表示无限重复）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        /// <param name="tag">标签（用于分组管理）</param>
        /// <param name="timeScale">时间缩放模式</param>
        /// <param name="onComplete">定时器完成/移除时触发的回调</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        [UnityEngine.Scripting.Preserve]
        public int Add(float interval, int repeat, Action<object> callback, object callbackParam = null, string tag = null, TimerTimeScale timeScale = TimerTimeScale.Unscaled, Action onComplete = null)
        {
            lock (Locker)
            {
                if (callback == null)
                {
                    Debug.LogWarning("timer callback is null, " + interval + "," + repeat);
                    return 0;
                }

                // Re-add existing callback: update in-place
                if (_items.TryGetValue(callback, out var t))
                {
                    UpdateTagIndex(t, tag);
                    t.Set(interval, repeat, callback, callbackParam, tag, timeScale, onComplete);
                    t.Elapsed = 0;
                    t.Deleted = false;
                    t.IsPaused = false;
                    return t.Id;
                }

                if (_toAdd.TryGetValue(callback, out t))
                {
                    UpdateTagIndex(t, tag);
                    t.Set(interval, repeat, callback, callbackParam, tag, timeScale, onComplete);
                    return t.Id;
                }

                int id = _nextId++;
                t = GetFromPool();
                t.Id = id;
                t.Interval = interval;
                t.Repeat = repeat;
                t.Callback = callback;
                t.Param = callbackParam;
                t.Tag = tag;
                t.TimeScaleMode = timeScale;
                t.OnComplete = onComplete;
                _toAdd[callback] = t;
                _ids[id] = callback;
                AddToTagIndex(t);
                return id;
            }
        }

        /// <summary>
        /// 添加一个只执行一次的任务
        /// </summary>
        /// <param name="interval">间隔时间（以秒为单位）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        [UnityEngine.Scripting.Preserve]
        public int AddOnce(float interval, Action<object> callback, object callbackParam = null)
        {
            return Add(interval, 1, callback, callbackParam);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        [UnityEngine.Scripting.Preserve]
        public int AddUpdate(Action<object> callback)
        {
            return Add(0, 0, callback);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数</param>
        /// <returns>定时器的唯一标识符 ID</returns>
        [UnityEngine.Scripting.Preserve]
        public int AddUpdate(Action<object> callback, object callbackParam)
        {
            return Add(0, 0, callback, callbackParam);
        }

        /// <summary>
        /// 检查指定的任务是否存在
        /// </summary>
        /// <param name="callback">要检查的回调函数</param>
        /// <returns>存在返回 true，不存在返回 false</returns>
        [UnityEngine.Scripting.Preserve]
        public bool Exists(Action<object> callback)
        {
            lock (Locker)
            {
                if (_toAdd.ContainsKey(callback))
                {
                    return true;
                }

                if (_items.TryGetValue(callback, out var at))
                {
                    return !at.Deleted;
                }

                return false;
            }
        }

        /// <summary>
        /// 移除指定的任务
        /// </summary>
        /// <param name="callback">要移除的回调函数</param>
        [UnityEngine.Scripting.Preserve]
        public void Remove(Action<object> callback)
        {
            lock (Locker)
            {
                if (_toAdd.TryGetValue(callback, out var t))
                {
                    _toAdd.Remove(callback);
                    _ids.Remove(t.Id);
                    RemoveFromTagIndex(t);
                    t.OnComplete?.Invoke();
                    ReturnToPool(t);
                    return;
                }

                if (_items.TryGetValue(callback, out t))
                {
                    t.Deleted = true;
                }
            }
        }

        // ──────────────── ID-based operations ────────────────

        /// <summary>
        /// 检查指定 ID 的任务是否存在且未被删除。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>存在且活跃返回 true，否则 false</returns>
        [UnityEngine.Scripting.Preserve]
        public bool Exists(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return false;
                }

                // Check _toAdd first (pending adds are always active)
                if (_toAdd.ContainsKey(callback))
                {
                    return true;
                }

                // Check _items (must not be Deleted)
                return _items.TryGetValue(callback, out var t) && !t.Deleted;
            }
        }

        /// <summary>
        /// 按 ID 移除指定的任务。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        [UnityEngine.Scripting.Preserve]
        public void Remove(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return;
                }

                Remove(callback);
            }
        }

        // ──────────────── Pause / Resume ────────────────

        /// <summary>
        /// 暂停指定 ID 的定时器（暂停期间不累积时间）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        [UnityEngine.Scripting.Preserve]
        public void Pause(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return;
                }

                if (TryGetLiveTimer(callback, out var t))
                {
                    t.IsPaused = true;
                }
            }
        }

        /// <summary>
        /// 恢复指定 ID 的定时器（从暂停处继续累积时间）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        [UnityEngine.Scripting.Preserve]
        public void Resume(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return;
                }

                if (TryGetLiveTimer(callback, out var t))
                {
                    t.IsPaused = false;
                }
            }
        }

        /// <summary>
        /// 检查指定 ID 的定时器是否处于暂停状态。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>暂停中返回 true，否则 false</returns>
        [UnityEngine.Scripting.Preserve]
        public bool IsPaused(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return false;
                }

                if (TryGetLiveTimer(callback, out var t))
                {
                    return t.IsPaused;
                }

                return false;
            }
        }

        // ──────────────── Tag-based operations ────────────────

        /// <summary>
        /// 暂停指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        [UnityEngine.Scripting.Preserve]
        public void PauseByTag(string tag)
        {
            lock (Locker)
            {
                if (_tags.TryGetValue(tag, out var ids))
                {
                    foreach (var id in ids)
                    {
                        if (_ids.TryGetValue(id, out var callback) &&
                            _items.TryGetValue(callback, out var t) && !t.Deleted)
                        {
                            t.IsPaused = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 恢复指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        [UnityEngine.Scripting.Preserve]
        public void ResumeByTag(string tag)
        {
            lock (Locker)
            {
                if (_tags.TryGetValue(tag, out var ids))
                {
                    foreach (var id in ids)
                    {
                        if (_ids.TryGetValue(id, out var callback) &&
                            _items.TryGetValue(callback, out var t))
                        {
                            t.IsPaused = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 移除指定标签的所有定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        [UnityEngine.Scripting.Preserve]
        public void RemoveByTag(string tag)
        {
            lock (Locker)
            {
                if (_tags.TryGetValue(tag, out var ids))
                {
                    // Copy to avoid modification during iteration
                    var idsCopy = new List<int>(ids);
                    foreach (var id in idsCopy)
                    {
                        Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// 检查指定标签是否有活跃的定时器。
        /// </summary>
        /// <param name="tag">标签名称</param>
        /// <returns>存在至少一个定时器返回 true，否则 false</returns>
        [UnityEngine.Scripting.Preserve]
        public bool HasTag(string tag)
        {
            lock (Locker)
            {
                return _tags.TryGetValue(tag, out var ids) && ids.Count > 0;
            }
        }

        // ──────────────── Query ────────────────

        /// <summary>
        /// 获取指定 ID 定时器的剩余时间（秒）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>剩余秒数；定时器不存在返回 -1</returns>
        [UnityEngine.Scripting.Preserve]
        public float GetRemaining(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return -1f;
                }

                if (TryGetLiveTimer(callback, out var t))
                {
                    if (t.Interval <= 0f || t.Repeat == 0)
                    {
                        return 0f;
                    }

                    float remaining = t.Interval - t.Elapsed;
                    return remaining > 0 ? remaining : 0f;
                }

                // Timer is in _toAdd (not yet flushed)
                return -1f;
            }
        }

        /// <summary>
        /// 获取指定 ID 定时器已累积的经过时间（秒）。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>已累积的秒数；定时器不存在返回 -1</returns>
        [UnityEngine.Scripting.Preserve]
        public float GetElapsed(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return -1f;
                }

                if (TryGetLiveTimer(callback, out var t))
                {
                    return t.Elapsed;
                }

                return -1f;
            }
        }

        /// <summary>
        /// 获取指定 ID 定时器的剩余重复次数。
        /// </summary>
        /// <param name="id">定时器 ID</param>
        /// <returns>剩余次数（0 表示无限循环）；定时器不存在返回 -1</returns>
        [UnityEngine.Scripting.Preserve]
        public int GetRepeatLeft(int id)
        {
            lock (Locker)
            {
                if (!_ids.TryGetValue(id, out var callback))
                {
                    return -1;
                }

                if (TryGetLiveTimer(callback, out var t))
                {
                    return t.Repeat;
                }

                return -1;
            }
        }

        private TimerItem GetFromPool()
        {
            TimerItem t;
            if (_pool.Count > 0)
            {
                t = _pool.Pop();
                t.Deleted = false;
                t.Elapsed = 0;
                t.IsPaused = false;
                t.PausedElapsed = 0;
                t.TimeScaleMode = TimerTimeScale.Unscaled;
                t.Tag = null;
                t.OnComplete = null;
            }
            else
            {
                t = new TimerItem();
            }

            return t;
        }

        private void ReturnToPool(TimerItem t)
        {
            t.Callback = null;
            t.Param = null;
            t.Tag = null;
            t.OnComplete = null;
            _pool.Push(t);
        }

        // ──────────────── Tag index helpers ────────────────

        private void AddToTagIndex(TimerItem t)
        {
            if (string.IsNullOrEmpty(t.Tag)) return;
            if (!_tags.TryGetValue(t.Tag, out var ids))
            {
                ids = new List<int>();
                _tags[t.Tag] = ids;
            }
            ids.Add(t.Id);
        }

        private void RemoveFromTagIndex(TimerItem t)
        {
            if (string.IsNullOrEmpty(t.Tag)) return;
            if (_tags.TryGetValue(t.Tag, out var ids))
            {
                ids.Remove(t.Id);
                if (ids.Count == 0)
                {
                    _tags.Remove(t.Tag);
                }
            }
        }

        private void UpdateTagIndex(TimerItem t, string newTag)
        {
            if (string.Equals(t.Tag, newTag, StringComparison.Ordinal)) return;
            // Remove from old tag
            RemoveFromTagIndex(t);
            // Update the item's tag so AddToTagIndex uses the new one
            t.Tag = newTag;
            // Add to new tag
            AddToTagIndex(t);
        }

        /// <summary>
        /// 按 回调 查找存活定时器：优先已合并的 _items，回退待合并的 _toAdd（Add 后、首次 Update 前同样可查询/暂停）。
        /// </summary>
        private bool TryGetLiveTimer(Action<object> callback, out TimerItem t)
        {
            if (_items.TryGetValue(callback, out t) && !t.Deleted)
            {
                return true;
            }

            return _toAdd.TryGetValue(callback, out t);
        }
    }
}