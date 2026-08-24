using System;
using System.Collections.Generic;
using EtherCAT_Test.Common;

namespace EtherCAT_Test.Equipment
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  SEMI E10 은 반도체 장비의 상태를 6가지로 분류하는 국제 표준.
    //  이 클래스는 장비가 각 상태에 '몇 초씩 머물렀는지'를 더해가며,
    //  마지막에 가동률(Availability, 얼마나 생산에 쓰였는지 %)을 계산한다.
    //
    //  [싱글턴 패턴] 이 클래스는 프로그램 전체에 딱 1개만 존재한다.
    //   - 생성자를 private 으로 막아 밖에서 new 하지 못하게 하고,
    //   - 대신 E10StateTracker.Instance 라는 '유일한 하나'를 공유해서 쓴다.
    //  이유: 누적 시간은 온 프로그램이 하나로 모아야 의미가 있기 때문.
    //  lock(_lock) : 여러 작업이 동시에 값을 건드려 꼬이지 않게 잠깐 문을 잠그는 장치.
    // ─────────────────────────────────────────────────────────────

    /// <summary>SEMI E10 6-state.</summary>
    public enum E10State
    {
        Productive,        // 생산 (Running / Auto)
        Standby,           // 대기 (Idle / Ready / Complete / Pause)
        Engineering,       // 엔지니어링 (수동 조작 — 현재 미매핑)
        ScheduledDown,     // 계획 정지 (미사용, enum 만 정의)
        UnscheduledDown,   // 비계획 정지 (Alarm / Emergency)
        NonScheduled       // 비가동 (PowerOff 등, 가동률 분모에서 제외)
    }

    /// <summary>
    /// EquipmentState → E10 6-state 매핑 및 상태별 누적 시간(초) 집계.
    /// 가동률 = Productive / (전체 - NonScheduled).
    /// EquipmentStateManager.ChangeState 훅(OnStateChanged)에서 전이 시각 기록으로 집계.
    /// </summary>
    public class E10StateTracker
    {
        private static readonly E10StateTracker _instance = new E10StateTracker();
        public static E10StateTracker Instance => _instance;

        private readonly object _lock = new object();
        private readonly Dictionary<E10State, double> _sec = new Dictionary<E10State, double>();
        private E10State _current;
        private DateTime _since;

        private E10StateTracker()
        {
            foreach (E10State s in Enum.GetValues(typeof(E10State))) _sec[s] = 0.0;
            _current = E10State.NonScheduled;   // 초기(연결 전) = 비가동
            _since = DateTime.Now;
        }

        public static E10State Map(EquipmentState s)
        {
            switch (s)
            {
                case EquipmentState.Running:
                case EquipmentState.Auto:
                    return E10State.Productive;

                case EquipmentState.Idle:
                case EquipmentState.Ready:
                case EquipmentState.Complete:
                case EquipmentState.Pause:
                case EquipmentState.Initializing:
                case EquipmentState.Homing:
                    return E10State.Standby;

                case EquipmentState.Alarm:
                case EquipmentState.Emergency:
                    return E10State.UnscheduledDown;

                case EquipmentState.PowerOff:
                    return E10State.NonScheduled;

                default:
                    return E10State.Standby;
            }
        }

        /// <summary>상태 전이 시점에 직전 상태의 체류시간을 누적.</summary>
        public void OnStateChange(EquipmentState prev, EquipmentState next)
        {
            Accumulate();
            lock (_lock) _current = Map(next);
        }

        private void Accumulate()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                _sec[_current] += (now - _since).TotalSeconds;
                _since = now;
            }
        }

        /// <summary>현재 진행분까지 반영한 상태별 누적시간(초) 스냅샷.</summary>
        public Dictionary<E10State, double> Snapshot()
        {
            Accumulate();
            lock (_lock) return new Dictionary<E10State, double>(_sec);
        }

        /// <summary>가동률(%) = Productive / (전체 - NonScheduled).</summary>
        // 가동률 = (실제 생산한 시간) ÷ (전원꺼짐 등 제외한 '가동 가능했던 시간) × 100.
        // 분모가 0 이하이면(아직 데이터 없음) 0% 로 처리해 0으로 나누는 오류를 막는다.
        public double AvailabilityPercent()
        {
            var snap = Snapshot();                        // 현재까지의 상태별 누적시간
            double total = 0.0;
            foreach (var kv in snap) total += kv.Value;   // 모든 상태 시간 합계
            double denom = total - snap[E10State.NonScheduled];   // 분모: 비가동(전원꺼짐 등) 제외
            return denom <= 0 ? 0.0 : snap[E10State.Productive] / denom * 100.0;
        }
    }
}
