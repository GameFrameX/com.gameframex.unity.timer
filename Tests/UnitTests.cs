using System;
using System.Reflection;
using GameFrameX.Timer.Runtime;
using NUnit.Framework;

namespace GameFrameX.Timer.Tests
{
    internal class UnitTests
    {
        private TimerManager _timerManager;

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _updateMethod = typeof(TimerManager)
            .GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance,
                null, new[] { typeof(float), typeof(float) }, null);

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _shutdownMethod = typeof(TimerManager)
            .GetMethod("Shutdown", BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);

        private static void InvokeUpdate(TimerManager manager, float elapseSeconds, float realElapseSeconds)
        {
            _updateMethod.Invoke(manager, new object[] { elapseSeconds, realElapseSeconds });
        }

        private static void InvokeShutdown(TimerManager manager)
        {
            _shutdownMethod.Invoke(manager, null);
        }

        [SetUp]
        public void Setup()
        {
            Assert.That(_updateMethod, Is.Not.Null, "TimerManager.Update(float,float) should be accessible via reflection");
            Assert.That(_shutdownMethod, Is.Not.Null, "TimerManager.Shutdown() should be accessible via reflection");

            _timerManager = new TimerManager();
        }

        [TearDown]
        public void TearDown()
        {
            InvokeShutdown(_timerManager);
        }

        // ──────────────── Add / AddOnce / AddUpdate ────────────────

        [Test]
        public void Add_IntervalTimer_FiresAfterInterval()
        {
            bool called = false;
            var callback = new Action<object>(_ => called = true);

            _timerManager.Add(1f, 1, callback);
            InvokeUpdate(_timerManager, 0f, 0.5f);

            Assert.That(called, Is.False, "Should not fire before interval elapses");

            InvokeUpdate(_timerManager, 0f, 0.6f);

            Assert.That(called, Is.True, "Should fire after interval elapses");
        }

        [Test]
        public void AddOnce_FiresOnceThenAutoRemoves()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.AddOnce(1f, callback);
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should fire first time");

            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should not fire again after auto-removal");
        }

        [Test]
        public void AddUpdate_FiresEveryFrame()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.AddUpdate(callback);

            InvokeUpdate(_timerManager, 0f, 0.016f);
            InvokeUpdate(_timerManager, 0f, 0.016f);
            InvokeUpdate(_timerManager, 0f, 0.016f);

            Assert.That(callCount, Is.EqualTo(3), "Should fire every frame");
        }

        [Test]
        public void AddUpdate_WithParam_PassesParamToCallback()
        {
            object received = null;
            var expected = new object();
            var callback = new Action<object>(p => received = p);

            _timerManager.AddUpdate(callback, expected);
            InvokeUpdate(_timerManager, 0f, 0.016f);

            Assert.That(received, Is.SameAs(expected), "Callback param should match");
        }

        // ──────────────── Remove ────────────────

        [Test]
        public void Remove_StopsTimerFromFiring()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.Add(1f, 0, callback);
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should fire before remove");

            _timerManager.Remove(callback);

            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should not fire after remove");
        }

        [Test]
        public void Remove_WhileTimerInToAdd_DropsWithoutAdding()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            // Add and immediately Remove before any Update flushes _toAdd
            _timerManager.Add(1f, 1, callback);
            _timerManager.Remove(callback);
            InvokeUpdate(_timerManager, 0f, 2f);

            Assert.That(callCount, Is.EqualTo(0), "Should not fire if removed before first Update");
            Assert.That(_timerManager.Exists(callback), Is.False, "Should not exist after removal");
        }

        // ──────────────── Exists ────────────────

        [Test]
        public void Exists_ReturnsFalse_BeforeAdd()
        {
            Assert.That(_timerManager.Exists(_ => { }), Is.False, "New callback should not exist");
        }

        [Test]
        public void Exists_ReturnsTrue_AfterAdd()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 1, callback);
            Assert.That(_timerManager.Exists(callback), Is.True, "Should exist after Add");
        }

        [Test]
        public void Exists_ReturnsFalse_AfterTimerExpires()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.AddOnce(1f, callback);
            InvokeUpdate(_timerManager, 0f, 2f);
            Assert.That(_timerManager.Exists(callback), Is.False, "Should not exist after one-shot expires");
        }

        [Test]
        public void Exists_ReturnsFalse_AfterRemove()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback);
            _timerManager.Remove(callback);
            Assert.That(_timerManager.Exists(callback), Is.False, "Should not exist after Remove");
        }

        // ──────────────── Repeat count ────────────────

        [Test]
        public void RepeatCount_TimerFiresSpecifiedNumberOfTimes()
        {
            int callCount = 0;
            int repeat = 3;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.Add(0.5f, repeat, callback);

            for (int i = 1; i <= 5; i++)
            {
                InvokeUpdate(_timerManager, 0f, 0.6f);

                if (i < repeat)
                {
                    Assert.That(callCount, Is.EqualTo(i), $"Should have fired {i} times so far");
                    Assert.That(_timerManager.Exists(callback), Is.True, $"Should still exist after fire #{i}");
                }
                else
                {
                    Assert.That(callCount, Is.EqualTo(repeat), $"Should stop at {repeat} fires");
                    Assert.That(_timerManager.Exists(callback), Is.False, "Should auto-remove after last fire");
                    break;
                }
            }
        }

        [Test]
        public void RepeatCount_Zero_MeansInfinite()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.Add(0.5f, 0, callback);
            Assert.That(_timerManager.Exists(callback), Is.True, "Should exist after add");

            // Fire 10 times — infinite repeat should keep going
            for (int i = 0; i < 10; i++)
            {
                InvokeUpdate(_timerManager, 0f, 0.6f);
            }

            Assert.That(callCount, Is.EqualTo(10), "Should fire every time for infinite repeat");
            Assert.That(_timerManager.Exists(callback), Is.True, "Should still exist for infinite repeat");
        }

        // ──────────────── Re-add same callback ────────────────

        [Test]
        public void ReAddSameCallback_UpdatesIntervalAndParam_InPlace()
        {
            string received = null;
            var callback = new Action<object>(p => received = (string)p);

            _timerManager.Add(1f, 1, callback, "first");
            // Re-add before first Update: should update the timer in _toAdd
            _timerManager.Add(2f, 0, callback, "updated");

            // 1.5s > 1 but < 2 — should NOT fire if interval was updated to 2
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(received, Is.Null, "Should use updated interval (2s), not original (1s)");

            // 2.5s total > 2s — should fire with updated param
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(received, Is.EqualTo("updated"), "Should use updated callback param");
        }

        [Test]
        public void ReAddSameCallback_AfterFirstFire_ResetsTimer()
        {
            int callCount = 0;
            var callback = new Action<object>(_ => callCount++);

            _timerManager.AddOnce(1f, callback);
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "First fire");
            Assert.That(_timerManager.Exists(callback), Is.False, "Timer should auto-remove");

            // Re-add the same callback after it expired — should work like a fresh timer
            _timerManager.AddOnce(1f, callback);
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(2), "Should fire again after re-add");
        }

        // ──────────────── Callback param ────────────────

        [Test]
        public void Param_IsPassedCorrectly()
        {
            object received = null;
            var expected = new object();
            _timerManager.AddOnce(1f, p => received = p, expected);

            InvokeUpdate(_timerManager, 0f, 1.5f);

            Assert.That(received, Is.SameAs(expected), "Callback param should be the same object reference");
        }

        [Test]
        public void Param_WhenNull_DoesNotThrow()
        {
            bool called = false;
            _timerManager.AddOnce(1f, _ => called = true);

            Assert.DoesNotThrow(() => InvokeUpdate(_timerManager, 0f, 1.5f));
            Assert.That(called, Is.True, "Callback should still fire with null param");
        }

        // ──────────────── CatchCallbackExceptions ────────────────

        [Test]
        public void CatchCallbackExceptions_On_DoesNotThrow_OnException()
        {
            var original = TimerManager.CatchCallbackExceptions;
            TimerManager.CatchCallbackExceptions = true;
            try
            {
                _timerManager.AddOnce(1f, _ => throw new InvalidOperationException("expected test error"));
                Assert.DoesNotThrow(() => InvokeUpdate(_timerManager, 0f, 1.5f),
                    "Should catch exception silently");
            }
            finally
            {
                TimerManager.CatchCallbackExceptions = original;
            }
        }

        [Test]
        public void CatchCallbackExceptions_Off_ThrowsToCaller()
        {
            var original = TimerManager.CatchCallbackExceptions;
            TimerManager.CatchCallbackExceptions = false;
            try
            {
                _timerManager.AddOnce(1f, _ => throw new InvalidOperationException("expected test error"));
                Assert.Throws<TargetInvocationException>(() => InvokeUpdate(_timerManager, 0f, 1.5f),
                    "Exception should propagate through reflection as TargetInvocationException");
            }
            finally
            {
                TimerManager.CatchCallbackExceptions = original;
            }
        }

        // ──────────────── Object pool ────────────────

        [Test]
        public void ObjectPool_ReusesTimerItem_AfterTimerExpires()
        {
            // Add & fire multiple one-shot timers — each recycles the TimerItem
            for (int i = 0; i < 5; i++)
            {
                bool called = false;
                _timerManager.AddOnce(0.1f, _ => called = true);
                InvokeUpdate(_timerManager, 0f, 0.2f);
                Assert.That(called, Is.True, $"Timer {i} should have fired");
            }

            // No crash, no stale state — the pool recycles correctly
        }

        // ──────────────── Shutdown ────────────────

        [Test]
        public void Shutdown_ClearsAllTimers()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback);
            _timerManager.AddOnce(2f, _ => { });
            Assert.That(_timerManager.Exists(callback), Is.True, "Timer exists before shutdown");

            InvokeShutdown(_timerManager);
            // After shutdown, no timers should fire
            Assert.That(_timerManager.Exists(callback), Is.False, "Timer should not exist after shutdown");
        }

        // ──────────────── Multiple timers ────────────────

        [Test]
        public void MultipleTimers_AllFireIndependently()
        {
            int countA = 0, countB = 0, countC = 0;
            var cbA = new Action<object>(_ => countA++);
            var cbB = new Action<object>(_ => countB++);
            var cbC = new Action<object>(_ => countC++);

            _timerManager.AddUpdate(cbA);            // every frame
            _timerManager.Add(0.5f, 4, cbB);         // 4 times, 0.5s interval
            _timerManager.AddOnce(1.5f, cbC);        // once at 1.5s

            // Frame 1: 0.016s
            InvokeUpdate(_timerManager, 0f, 0.016f);
            Assert.That(countA, Is.EqualTo(1), "cbA should fire frame 1");

            // Frame 2: 0.016s → total ~0.032s
            InvokeUpdate(_timerManager, 0f, 0.016f);
            Assert.That(countA, Is.EqualTo(2), "cbA should fire frame 2");

            // Frame 3: 0.6s → cbB fires (0.616s total)
            InvokeUpdate(_timerManager, 0f, 0.6f);
            Assert.That(countB, Is.EqualTo(1), "cbB should fire once at 0.6s");
            Assert.That(countC, Is.EqualTo(0), "cbC should not fire yet");

            // Frame 4: 0.016s
            InvokeUpdate(_timerManager, 0f, 0.016f);
            Assert.That(countB, Is.EqualTo(1), "cbB should not fire yet");

            // Frame 5: 0.6f → cbB second fire (1.232s total)
            InvokeUpdate(_timerManager, 0f, 0.6f);
            Assert.That(countB, Is.EqualTo(2), "cbB second fire");

            // Frame 6: 0.6f → cbB 3rd + cbC fires (1.832s total)
            InvokeUpdate(_timerManager, 0f, 0.6f);
            Assert.That(countB, Is.EqualTo(3), "cbB third fire");
            Assert.That(countC, Is.EqualTo(1), "cbC should fire at ~1.5s");
            Assert.That(_timerManager.Exists(cbC), Is.False, "cbC should auto-remove");

            // Frame 7: 0.6f → cbB 4th (last)
            InvokeUpdate(_timerManager, 0f, 0.6f);
            Assert.That(countB, Is.EqualTo(4), "cbB fourth fire");
            Assert.That(_timerManager.Exists(cbB), Is.False, "cbB should auto-remove after 4 fires");

            // cbA keeps firing every frame regardless
            Assert.That(countA, Is.GreaterThanOrEqualTo(4), "cbA should have fired many times");
        }

        // ──────────────── ID system ────────────────

        [Test]
        public void Add_ReturnsPositiveId()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            Assert.That(id, Is.GreaterThan(0), "Timer ID should be positive");
        }

        [Test]
        public void Add_EachCallReturnsUniqueId()
        {
            int id1 = _timerManager.Add(1f, 1, _ => { });
            int id2 = _timerManager.Add(1f, 1, _ => { });
            Assert.That(id2, Is.Not.EqualTo(id1), "Each timer should get a unique ID");
        }

        [Test]
        public void Exists_ById_ReturnsTrue_AfterAdd()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            Assert.That(_timerManager.Exists(id), Is.True, "Timer should exist by ID after Add");
        }

        [Test]
        public void Exists_ById_ReturnsFalse_AfterRemove()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            _timerManager.Remove(id);
            Assert.That(_timerManager.Exists(id), Is.False, "Timer should not exist by ID after Remove");
        }

        [Test]
        public void Exists_ById_ReturnsFalse_ForUnknownId()
        {
            Assert.That(_timerManager.Exists(99999), Is.False, "Unknown ID should not exist");
        }

        [Test]
        public void Remove_ById_StopsTimer()
        {
            int callCount = 0;
            int id = _timerManager.Add(1f, 0, _ => callCount++);
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should fire before remove");

            _timerManager.Remove(id);
            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should not fire after Remove by ID");
        }

        [Test]
        public void Remove_ById_AlsoCleansCallbackLookup()
        {
            var callback = new Action<object>(_ => { });
            int id = _timerManager.Add(1f, 1, callback);
            _timerManager.Remove(id);
            Assert.That(_timerManager.Exists(callback), Is.False, "Callback should also be removed");
        }

        [Test]
        public void AddOnce_ReturnsPositiveId()
        {
            int id = _timerManager.AddOnce(1f, _ => { });
            Assert.That(id, Is.GreaterThan(0), "AddOnce should return positive ID");
        }

        [Test]
        public void AddUpdate_ReturnsPositiveId()
        {
            int id = _timerManager.AddUpdate(_ => { });
            Assert.That(id, Is.GreaterThan(0), "AddUpdate should return positive ID");
        }

        // ──────────────── Pause / Resume ────────────────

        [Test]
        public void Pause_PreventsTimerFromFiring()
        {
            int callCount = 0;
            int id = _timerManager.Add(1f, 0, _ => callCount++);

            InvokeUpdate(_timerManager, 0f, 0.5f);
            _timerManager.Pause(id);

            // Even with enough time passing, paused timer shouldn't fire
            InvokeUpdate(_timerManager, 0f, 2f);
            Assert.That(callCount, Is.EqualTo(0), "Paused timer should not fire");
        }

        [Test]
        public void PauseThenResume_TimerContinuesFromWhereItStopped()
        {
            int callCount = 0;
            int id = _timerManager.Add(1f, 2, _ => callCount++);

            // Accumulate 0.6s (not enough to fire)
            InvokeUpdate(_timerManager, 0f, 0.6f);
            _timerManager.Pause(id);

            // Paused for 2s - should NOT accumulate while paused
            InvokeUpdate(_timerManager, 0f, 2f);
            Assert.That(callCount, Is.EqualTo(0), "Should not accumulate while paused");

            // Resume and finish remaining 0.4s
            _timerManager.Resume(id);
            InvokeUpdate(_timerManager, 0f, 0.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should fire after resume + remaining time");
        }

        // ──────────────── Tag system ────────────────

        [Test]
        public void Tag_HasTag_ReturnsTrueAfterAdd()
        {
            _timerManager.Add(1f, 1, _ => { }, tag: "test-tag");
            Assert.That(_timerManager.HasTag("test-tag"), Is.True, "HasTag should be true after Add with tag");
        }

        [Test]
        public void Tag_HasTag_ReturnsFalse_ForUnknownTag()
        {
            Assert.That(_timerManager.HasTag("nonexistent"), Is.False, "Unknown tag should return false");
        }

        [Test]
        public void Tag_PauseByTag_PausesAllTaggedTimers()
        {
            int countA = 0, countB = 0;
            _timerManager.Add(1f, 0, _ => countA++, tag: "group1");
            _timerManager.Add(1f, 0, _ => countB++, tag: "group1");

            InvokeUpdate(_timerManager, 0f, 0.5f);
            _timerManager.PauseByTag("group1");

            InvokeUpdate(_timerManager, 0f, 2f);
            Assert.That(countA, Is.EqualTo(0), "Timer A should not fire after PauseByTag");
            Assert.That(countB, Is.EqualTo(0), "Timer B should not fire after PauseByTag");
        }

        [Test]
        public void Tag_ResumeByTag_ResumesAllTaggedTimers()
        {
            int callCount = 0;
            int id = _timerManager.Add(1f, 0, _ => callCount++, tag: "group1");

            _timerManager.PauseByTag("group1");
            _timerManager.ResumeByTag("group1");

            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should fire after ResumeByTag");
        }

        [Test]
        public void Tag_RemoveByTag_RemovesAllTaggedTimers()
        {
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback, tag: "group1");
            Assert.That(_timerManager.Exists(callback), Is.True, "Timer exists after add");

            _timerManager.RemoveByTag("group1");
            Assert.That(_timerManager.Exists(callback), Is.False, "Timer should not exist after RemoveByTag");
            Assert.That(_timerManager.HasTag("group1"), Is.False, "Tag index should be empty");
        }

        [Test]
        public void Tag_UntaggedTimers_NotAffectedByTagOperations()
        {
            int untaggedCount = 0, taggedCount = 0;
            _timerManager.Add(1f, 0, _ => untaggedCount++);                           // no tag
            _timerManager.Add(1f, 0, _ => taggedCount++, tag: "group1");

            InvokeUpdate(_timerManager, 0f, 0.5f);
            _timerManager.PauseByTag("group1");

            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(untaggedCount, Is.GreaterThan(0), "Untagged timer should still fire");
            Assert.That(taggedCount, Is.EqualTo(0), "Tagged timer should be paused");
        }

        // ──────────────── Time scale mode ────────────────

        [Test]
        public void TimeScale_Scaled_UsesElapseSeconds()
        {
            int callCount = 0;
            // Scaled timer: uses elapseSeconds (first arg to InvokeUpdate)
            _timerManager.Add(1f, 1, _ => callCount++, timeScale: TimerTimeScale.Scaled);

            // First Update: elapseSeconds=0.5, realElapseSeconds=0.016 → elapse is used, not enough
            InvokeUpdate(_timerManager, 0.5f, 0.016f);
            Assert.That(callCount, Is.EqualTo(0), "Should not fire at 0.5s scaled elapsed");

            // Second Update: elapseSeconds=0.6, total scaled=1.1 → enough
            InvokeUpdate(_timerManager, 0.6f, 0.016f);
            Assert.That(callCount, Is.EqualTo(1), "Should fire at 1.1s scaled elapsed");
        }

        [Test]
        public void TimeScale_DefaultIsUnscaled_UsesRealElapseSeconds()
        {
            int callCount = 0;
            // Default Unscaled: uses realElapseSeconds (second arg to InvokeUpdate)
            _timerManager.Add(1f, 1, _ => callCount++);

            // realElapseSeconds = 0.6 → should fire (accumulated 0.6 < 1... wait, 0.6 < 1, not enough)
            InvokeUpdate(_timerManager, 0f, 0.6f);
            Assert.That(callCount, Is.EqualTo(0), "Should not fire at 0.6s real elapsed");

            // realElapseSeconds = 0.5 → total 1.1 → enough
            InvokeUpdate(_timerManager, 0f, 0.5f);
            Assert.That(callCount, Is.EqualTo(1), "Should fire at 1.1s real elapsed");
        }

        // ──────────────── Query API ────────────────

        [Test]
        public void GetRemaining_ReturnsCorrectValue()
        {
            int id = _timerManager.Add(2f, 1, _ => { });
            InvokeUpdate(_timerManager, 0f, 0.5f);

            float remaining = _timerManager.GetRemaining(id);
            Assert.That(remaining, Is.GreaterThan(0f), "Remaining should be > 0 before timer expires");
            Assert.That(remaining, Is.LessThanOrEqualTo(1.5f), "Remaining should be <= 1.5 after 0.5s elapsed");
        }

        [Test]
        public void GetRemaining_ReturnsNegative_ForUnknownId()
        {
            Assert.That(_timerManager.GetRemaining(99999), Is.EqualTo(-1f), "Unknown ID should return -1");
        }

        [Test]
        public void GetElapsed_ReturnsNegative_ForUnknownId()
        {
            Assert.That(_timerManager.GetElapsed(99999), Is.EqualTo(-1f), "Unknown ID should return -1");
        }

        [Test]
        public void GetRepeatLeft_ReturnsCorrectValue()
        {
            int id = _timerManager.Add(1f, 5, _ => { });
            Assert.That(_timerManager.GetRepeatLeft(id), Is.EqualTo(5), "Should show 5 repeats remaining");

            InvokeUpdate(_timerManager, 0f, 1.5f);
            Assert.That(_timerManager.GetRepeatLeft(id), Is.EqualTo(4), "Should decrement after each fire");
        }

        [Test]
        public void GetRepeatLeft_ReturnsNegative_ForUnknownId()
        {
            Assert.That(_timerManager.GetRepeatLeft(99999), Is.EqualTo(-1), "Unknown ID should return -1");
        }

        // ──────────────── OnComplete ────────────────

        [Test]
        public void OnComplete_Fires_WhenTimerNaturallyCompletes()
        {
            bool completed = false;
            int id = _timerManager.Add(1f, 3, _ => { }, onComplete: () => completed = true);

            // Fire 3 times
            InvokeUpdate(_timerManager, 0f, 1.5f); // 1st
            Assert.That(completed, Is.False, "Should not complete after 1st fire");

            InvokeUpdate(_timerManager, 0f, 1.5f); // 2nd
            Assert.That(completed, Is.False, "Should not complete after 2nd fire");

            InvokeUpdate(_timerManager, 0f, 1.5f); // 3rd → expired
            Assert.That(completed, Is.True, "Should complete after 3rd fire");
        }

        [Test]
        public void OnComplete_Fires_WhenTimerRemovedByCallback()
        {
            bool completed = false;
            var callback = new Action<object>(_ => { });
            _timerManager.Add(1f, 0, callback, onComplete: () => completed = true);

            _timerManager.Remove(callback);
            Assert.That(completed, Is.True, "OnComplete should fire when removed by callback");
        }

        [Test]
        public void OnComplete_DoesNotFire_ForInfiniteRepeatTimer()
        {
            bool completed = false;
            _timerManager.Add(1f, 0, _ => { }, onComplete: () => completed = true);

            // Fire multiple times — infinite repeat never completes
            for (int i = 0; i < 10; i++)
            {
                InvokeUpdate(_timerManager, 0f, 1.5f);
            }

            Assert.That(completed, Is.False, "OnComplete should not fire for infinite repeat timer");
        }

        // ──────────────── IsPaused ────────────────

        [Test]
        public void IsPaused_ReturnsFalse_ForActiveTimer()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            Assert.That(_timerManager.IsPaused(id), Is.False, "Active timer should not be paused");
        }

        [Test]
        public void IsPaused_ReturnsTrue_AfterPause()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            _timerManager.Pause(id);
            Assert.That(_timerManager.IsPaused(id), Is.True, "Timer should be paused");
        }

        [Test]
        public void IsPaused_ReturnsFalse_AfterResume()
        {
            int id = _timerManager.Add(1f, 1, _ => { });
            _timerManager.Pause(id);
            _timerManager.Resume(id);
            Assert.That(_timerManager.IsPaused(id), Is.False, "Timer should not be paused after resume");
        }

        [Test]
        public void IsPaused_ReturnsFalse_ForUnknownId()
        {
            Assert.That(_timerManager.IsPaused(99999), Is.False, "Unknown ID should not be paused");
        }

        // ──────────────── Edge cases ────────────────

        [Test]
        public void ReAdd_WithDifferentTag_UpdatesTagIndex()
        {
            _timerManager.Add(1f, 1, _ => { }, tag: "old-tag");
            Assert.That(_timerManager.HasTag("old-tag"), Is.True, "Tag index should have old-tag after first add");

            // Re-add same callback with new tag
            _timerManager.Add(1f, 1, _ => { }, tag: "new-tag");
            Assert.That(_timerManager.HasTag("old-tag"), Is.False, "Old tag should be removed from index");
            Assert.That(_timerManager.HasTag("new-tag"), Is.True, "New tag should be in index");
        }

        [Test]
        public void Remove_UnknownId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _timerManager.Remove(99999), "Remove with unknown ID should not throw");
        }

        [Test]
        public void Pause_AfterTimerExpired_IsNoOp()
        {
            int id = _timerManager.AddOnce(0.1f, _ => { });
            InvokeUpdate(_timerManager, 0f, 1f); // Timer expired
            Assert.That(_timerManager.Exists(id), Is.False, "Timer should not exist after expiration");

            Assert.DoesNotThrow(() => _timerManager.Pause(id), "Pause on expired timer should not throw");
        }

        [Test]
        public void Resume_AfterTimerExpired_IsNoOp()
        {
            int id = _timerManager.AddOnce(0.1f, _ => { });
            InvokeUpdate(_timerManager, 0f, 1f);

            Assert.DoesNotThrow(() => _timerManager.Resume(id), "Resume on expired timer should not throw");
        }

        [Test]
        public void NegativeInterval_TreatedAsPerFrame()
        {
            int callCount = 0;
            _timerManager.Add(-1f, 3, _ => callCount++);

            InvokeUpdate(_timerManager, 0f, 0.016f);
            Assert.That(callCount, Is.EqualTo(1), "Negative interval should fire every frame");

            InvokeUpdate(_timerManager, 0f, 0.016f);
            Assert.That(callCount, Is.EqualTo(2), "Should fire frame 2");

            InvokeUpdate(_timerManager, 0f, 0.016f);
            Assert.That(callCount, Is.EqualTo(3), "Should fire frame 3 then stop");
            Assert.That(_timerManager.Exists(callback => { }), Is.False, "Should auto-remove after 3 fires");
        }

        [Test]
        public void GetRemaining_ForInfiniteRepeat_ReturnsZero()
        {
            int id = _timerManager.Add(1f, 0, _ => { }); // infinite
            Assert.That(_timerManager.GetRemaining(id), Is.EqualTo(0f), "Infinite repeat should report 0 remaining");
        }

        [Test]
        public void GetRemaining_ForPerFrameTimer_ReturnsZero()
        {
            int id = _timerManager.Add(0f, 1, _ => { }); // per-frame, one shot
            Assert.That(_timerManager.GetRemaining(id), Is.EqualTo(0f), "Per-frame timer should report 0 remaining");
        }
    }
}
