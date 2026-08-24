using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherCAT_Test.Common;
using EtherCAT_Test.Logging;

namespace EtherCAT_Test.Equipment
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  장비가 지금 어떤 상태인지(전원꺼짐/대기/운전/알람 등)를 한 곳에서 관리.
    //  상태가 '바뀔 때'마다 자동으로 두 가지를 한다:
    //   ① 로그(기록) 남기기,  ② 구독자에게 "상태 바뀌었다!"고 알림(OnStateChanged).
    //  이렇게 알림을 쏘면, E10 가동률 집계 같은 다른 기능이 그 순간에 맞춰 동작할 수 있다.
    // ─────────────────────────────────────────────────────────────
    public class EquipmentStateManager
    {
        // 현재 상태. 밖에서는 읽기만 가능(private set)하고, 바꾸려면 ChangeState 를 거쳐야 함.
        public EquipmentState CurrentState { get; private set; }

        // 생성자: 처음엔 전원 꺼짐 상태로 시작.
        public EquipmentStateManager()
        {
            CurrentState = EquipmentState.PowerOff;
        }

        // 상태를 바꾸는 유일한 통로. 바뀐 경우에만 로그+알림을 발생시킨다.
        public void ChangeState(EquipmentState state)
        {
            var prev = CurrentState;   // 바뀌기 전 상태 기억
            CurrentState = state;      // 새 상태로 교체
            if (prev != state)         // 진짜로 달라졌을 때만
            {
                LogManager.Instance.LogState(prev.ToString(), state.ToString());  // 상태 전이 로그
                OnStateChanged?.Invoke(prev, state);                              // E10 집계 훅(Phase 3)
            }
        }

        /// <summary>상태 전이 훅(이전, 이후). 미구독 시 아무 동작 없음.</summary>
        public Action<EquipmentState, EquipmentState> OnStateChanged;

        // 아래는 "지금 이 상태 맞아?"를 짧게 물어보는 편의용 속성들 (읽기 전용).
        // 예: if (equipment.IsAlarm) 처럼 쓰면 코드가 읽기 쉬워진다.
        public bool IsAuto => CurrentState == EquipmentState.Auto;

        public bool IsRunning => CurrentState == EquipmentState.Running;

        public bool IsAlarm => CurrentState == EquipmentState.Alarm;

        public bool IsEmergency => CurrentState == EquipmentState.Emergency;
    }
}
