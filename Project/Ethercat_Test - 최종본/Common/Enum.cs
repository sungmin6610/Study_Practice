using System;

namespace EtherCAT_Test.Common
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  enum(열거형) = "정해진 이름표들 중 하나만 고르는 타입".
    //  예를 들어 신호등을 int 로 0,1,2 라고 쓰면 헷갈리지만,
    //  enum 으로 빨강/노랑/초록 이라고 이름을 붙이면 코드가 훨씬 읽기 쉽다.
    //  여기서는 장비 상태, 챔버 종류, 공정 종류, 웨이퍼 상태/위치,
    //  그리고 자동운전 진행 단계(AutoStep) 를 이름표로 정의한다.
    // ─────────────────────────────────────────────────────────────

    // 장비 전체의 현재 상태 (전원꺼짐 → 초기화 → 원점복귀 → 대기 → 자동/운전 → 완료/알람 등)
    public enum EquipmentState
    {
        PowerOff,
        Initializing,
        Homing,
        Idle,
        Ready,
        Auto,
        Running,
        Pause,
        Complete,
        Alarm,
        Emergency
    }

    // 챔버 종류 3가지 (A, B, C)
    public enum ChamberType { A, B, C }

    // 각 챔버에서 하는 공정 종류 (포토 리소그래피의 3단계)
    public enum ProcessType
    {
        PRCoating,   // A 챔버 : PR 도포 (감광액 바르기)
        Exposure,    // B 챔버 : 노광 (빛으로 회로 모양 찍기)
        Develop      // C 챔버 : 현상 (필요없는 부분 씻어내기)
    }

    // 웨이퍼(반도체 원판) 1장의 진행 상태
    public enum WaferState { Ready, Processing, Completed, Scrap }  // 대기 / 처리중 / 완료 / 폐기

    // 웨이퍼가 지금 어디에 있는지 (출발 선반 → 로봇 → 챔버 A/B/C → 도착 선반)
    public enum WaferLocation { FOUPA, Robot, ChamberA, ChamberB, ChamberC, FOUPB }

    // ─────────────────────────────────────────────────────
    //  자동 시퀀스 스텝
    //  챔버 A/B/C 는 chamberIndex(0~2) 로 파라미터화하여
    //  동일 스텝을 3회 반복 사용한다.
    //
    //  [읽는 법] 자동운전은 "지금 몇 단계까지 왔는지"를 이 이름표로 기억한다.
    //  보통 두 개가 짝: 명령을 내리는 스텝(예: Pick_MoveDown = 내려가라) 다음에
    //  그 결과를 기다리는 스텝(예: Pick_WaitDown = 다 내려갈 때까지 기다림) 이 온다.
    //  아래로 갈수록 나중 단계이며, 맨 끝은 Complete(완료) 또는 AlarmStop(이상 정지).
    // ─────────────────────────────────────────────────────
    public enum AutoStep
    {
        Idle = 0,

        // 초기화
        ServoOn,
        HomeUD,
        WaitUDHome,
        HomeLR,          // 중요: 상/하 원점 완료 후에만 진입
        WaitLRHome,
        Ready,

        // ── FOUP A 픽업 ──
        Pick_MoveLR,          // 좌/우 → FOUPA_X (로봇 후진 인터록)
        Pick_WaitLR,
        Pick_MoveDown,        // 상/하 → 슬롯 안착 위치
        Pick_WaitDown,
        Pick_Forward,         // 포크 전진
        Pick_WaitForward,
        Pick_VacuumOn,        // 진공 흡착
        Pick_WaitVacuum,
        Pick_MoveUp,          // 웨이퍼를 들어올린 후에
        Pick_WaitUp,
        Pick_Backward,        // 후진
        Pick_WaitBackward,

        // ── 챔버 안착 (chamberIndex 공용) ──
        Cham_DoorOpen,
        Cham_WaitDoorOpen,
        Cham_MoveLR,
        Cham_WaitLR,
        Cham_MoveUp,          // 상승(이동) 높이로
        Cham_WaitUp,
        Cham_Forward,
        Cham_WaitForward,
        Cham_MoveDown,        // 안착 높이로 내려 웨이퍼를 내려놓음
        Cham_WaitDown,
        Cham_VacuumOff,
        Cham_Blow,            // 파기(Blow)로 확실히 분리
        Cham_BlowOff,
        Cham_Backward,
        Cham_WaitBackward,
        Cham_DoorClose,
        Cham_WaitDoorClose,

        // ── 챔버 공정 (램프 ON = 공정 진행) ──
        Cham_ProcessStart,
        Cham_ProcessWait,

        // ── 챔버 픽업 ──
        Cham_DoorReopen,
        Cham_WaitDoorReopen,
        Cham_PickForward,     // 안착 높이 그대로 포크 삽입
        Cham_WaitPickForward,
        Cham_PickVacuum,
        Cham_WaitPickVacuum,
        Cham_PickUp,          // 상승 높이로 들어올림
        Cham_WaitPickUp,
        Cham_PickBackward,
        Cham_WaitPickBackward,
        Cham_DoorClose2,
        Cham_WaitDoorClose2,

        NextChamberOrUnload,  // 다음 챔버로 or FOUP B 언로드로 분기

        // ── FOUP B 언로드 ──
        Unload_MoveLR,
        Unload_WaitLR,
        Unload_MoveUp,        // 슬롯 상승 위치
        Unload_WaitUp,
        Unload_Forward,
        Unload_WaitForward,
        Unload_MoveDown,      // 안착 위치로 내려놓음
        Unload_WaitDown,
        Unload_VacuumOff,
        Unload_Blow,
        Unload_BlowOff,
        Unload_Backward,
        Unload_WaitBackward,
        Unload_MoveSafeUp,    // 슬롯 상승 위치로 복귀
        Unload_WaitSafeUp,

        NextSlotOrComplete,   // 다음 슬롯 반복 or 완료

        Complete,
        AlarmStop             // 타임아웃/EMG 등 이상 시 정지
    }
}
