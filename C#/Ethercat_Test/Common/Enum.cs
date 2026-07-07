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

    // 수정 2(파이프라이닝): 이송 잡의 출발/도착 스테이션
    public enum Station { FOUPA, ChamberA, ChamberB, ChamberC, FOUPB }

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
    //  수정 2(파이프라이닝): 범용 이송(픽업 P_* → 안착 Q_*) + 스케줄러(Schedule) 스텝.
    //  기존 Pick_*/Cham_*/Unload_* 를 스테이션 파라미터 기반 공용 스텝으로 통합.
    public enum AutoStep
    {
        Idle = 0,

        // 초기화 (규칙 3: 상/하 원점 → 좌/우 원점 순서)
        ServoOn,
        HomeUD,
        WaitUDHome,
        HomeLR,
        WaitLRHome,
        Ready,

        // 스케줄러: 유휴 시 다음 잡 선정 (다운스트림 우선)
        Schedule,

        // ── 픽업 (job.From) : 안착높이 → 전진 → 진공ON → 상승높이 → 후진 ──
        P_MoveLR, P_WaitLR,
        P_DoorOpen, P_WaitDoorOpen,      // 챔버일 때만
        P_MoveDown, P_WaitDown,          // 안착높이
        P_Forward, P_WaitForward,
        P_VacOn, P_WaitVac,
        P_MoveUp, P_WaitUp,              // 상승높이(들어올림)
        P_Backward, P_WaitBackward,
        P_DoorClose, P_WaitDoorClose,    // 챔버일 때만

        // ── 안착 (job.To) : 상승높이 → 전진 → 안착높이 → 진공OFF/Blow → 후진 ──
        Q_MoveLR, Q_WaitLR,
        Q_DoorOpen, Q_WaitDoorOpen,      // 챔버일 때만
        Q_MoveUp, Q_WaitUp,              // 상승높이
        Q_Forward, Q_WaitForward,
        Q_MoveDown, Q_WaitDown,          // 안착높이(내려놓음)
        Q_VacOff, Q_Blow, Q_BlowOff,
        Q_Backward, Q_WaitBackward,
        Q_DoorClose, Q_WaitDoorClose,    // 챔버일 때만 → StartProcess
        Q_SafeUp, Q_WaitSafeUp,          // FOUP 일 때만 (복귀 상승)

        JobDone,          // 잡 종료 → Schedule 복귀

        Complete,
        AlarmStop         // 타임아웃/EMG 등 이상 시 정지
    }
}
