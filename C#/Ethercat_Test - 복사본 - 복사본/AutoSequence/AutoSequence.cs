using System;
using EtherCAT_Test.Alarm;
using EtherCAT_Test.Chamber;
using EtherCAT_Test.Common;
using EtherCAT_Test.Equipment;
using EtherCAT_Test.IO;
using EtherCAT_Test.Logging;
using EtherCAT_Test.Motion;
using EtherCAT_Test.Process;
using EtherCAT_Test.Robot;

namespace EtherCAT_Test.Sequence
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일 = 자동운전의 '두뇌']
    //  이 프로그램에서 가장 중요한 부분. 웨이퍼 5장을 사람 손 없이 자동으로
    //  옮기고 공정을 돌리는 순서를 지휘한다.
    //  전체 경로: FOUP A(슬롯 1~5) → 챔버 A(PR도포) → B(노광) → C(현상) → FOUP B
    //
    //  [작동 방식 = 상태 머신]
    //  100ms(0.1초)마다 Run() 이 한 번씩 불린다. Run() 은 '지금 스텝(Step)'을 보고
    //  그 스텝의 일만 조금 하고 바로 끝난다. 다음 0.1초에 또 불려 다음 스텝을 한다.
    //  즉 한 번에 다 하지 않고, 잘게 나눠 조금씩 진행한다(그래야 화면이 안 멈춘다).
    //  보통 "명령 스텝(움직여라)" → "대기 스텝(다 될 때까지 기다림)" 이 짝을 이룬다.
    //
    //  [안전장치]
    //   - 인터록: 비상정지(EMG)·공압 저하 등 위험 신호는 매번 최우선으로 확인
    //   - 타임아웃: 정해진 시간 안에 동작이 안 끝나면 알람을 내고 멈춘다
    //   - 타워램프: 녹=운전 / 적=알람·비상 자동 표시
    //
    //  [의존성 주입] 필요한 매니저들(io, motion, robot ...)을 직접 만들지 않고
    //  생성자로 '받아서' 쓴다. 그래서 이 클래스는 지휘만 하고 실제 하드웨어 조작은
    //  각 매니저에게 시킨다.
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// 포토공정 자동 시퀀스 (100ms 타이머에서 Run() 호출)
    ///   FOUP A 슬롯 1~5 → 챔버 A(PR도포) → B(노광) → C(현상) → FOUP B
    /// - EMG / 메인압력 인터록
    /// - 스텝별 타임아웃 → AlarmManager
    /// - 타워램프 자동 제어 (녹:운전 / 황:대기 / 적:알람)
    /// - Wafer 상태 추적
    /// </summary>
    public class AutoSequence
    {
        private readonly IOManager io;
        private readonly MotionManager motion;
        private readonly RobotManager robot;
        private readonly ChamberManager chamberMgr;
        private readonly EquipmentStateManager equipment;
        private readonly WaferManager waferMgr;
        private readonly AlarmManager alarm;

        public AutoStep Step { get; private set; }
        public bool AutoRun { get; private set; }

        public int CurrentSlot { get; private set; }        // 1~5
        public int CurrentChamberIndex { get; private set; } // 0=A, 1=B, 2=C

        // 수정 3: 챔버 공정 진행률 표시용 (읽기전용). 다른 로직 변경 없음.
        public int CurrentProcessElapsedMs => processTicks * Constants.TimerInterval;

        // ── 타임아웃 관리 (Run() 1회 = TimerInterval ms) ──
        private int waitTicks;
        private int TimeoutMotion => 15000 / Constants.TimerInterval; // 서보 이동 15s
        private int TimeoutCylinder => 3000 / Constants.TimerInterval; // 실린더 3s
        private int TimeoutVacuum => 2000 / Constants.TimerInterval; // 진공 2s
        private int processTicks;                                       // 챔버 공정 시간용

        public AutoSequence(IOManager ioManager,
                            MotionManager motionManager,
                            RobotManager robotManager,
                            ChamberManager chamberManager,
                            EquipmentStateManager equipmentManager,
                            WaferManager waferManager,
                            AlarmManager alarmManager)
        {
            io = ioManager;
            motion = motionManager;
            robot = robotManager;
            chamberMgr = chamberManager;
            equipment = equipmentManager;
            waferMgr = waferManager;
            alarm = alarmManager;
            Step = AutoStep.Idle;
        }

        // 자동운전 시작 버튼이 부르는 함수.
        // 이미 돌고 있으면 그냥 나가고(중복 시작 방지), 아니면 처음 값으로 초기화한 뒤 시작.
        public void Start()
        {
            if (AutoRun) return;          // 이미 자동운전 중이면 아무것도 안 함
            alarm.Clear();                // 이전 알람 지우기
            CurrentSlot = 1;              // 1번 슬롯부터
            CurrentChamberIndex = 0;      // 챔버 A(0) 부터
            waitTicks = 0;
            Step = AutoStep.Idle;         // 첫 스텝으로 되돌림
            AutoRun = true;               // '자동운전 중' 켜기
            OnAutoStart?.Invoke();   // 로깅/ Lot 시작 훅
        }

        // 로깅/Lot 연동용 훅 (구독은 Form1; 미구독 시 아무 동작 없음 → 시퀀스 로직 불변)
        public Action OnAutoStart;
        public Action OnAutoComplete;
        public Action<string> OnAutoAbort;

        public void Stop()
        {
            AutoRun = false;
            equipment.ChangeState(EquipmentState.Pause);
        }

        public void Reset()
        {
            AutoRun = false;
            Step = AutoStep.Idle;
            alarm.Clear();
            equipment.ChangeState(EquipmentState.Idle);
        }

        // 지금 다루는 챔버 한 개를 짧게 가리키는 이름(현재 챔버 인덱스로 골라줌).
        private EtherCAT_Test.Chamber.Chamber Cham
            => chamberMgr.Chambers[CurrentChamberIndex];
        // 지금 다루는 웨이퍼 한 장을 짧게 가리키는 이름(현재 슬롯 번호로 골라줌).
        private Wafer CurWafer => waferMgr.GetWafer(CurrentSlot);

        // ────────────────────────────────────────────────
        //  100ms 타이머가 매번 부르는 함수. 자동운전의 심장 박동.
        public void Run()
        {
            UpdateTowerLamp();

            //    자동운전 중이 아니면 인터록/시퀀스를 실행하지 않는다.
            if (!AutoRun) return;

            // ── 안전 인터록 (매 스캔 최우선 확인) ──
            // EMG 는 N.C. 접점: 신호 ON = 정상, 신호 OFF = 비상정지 눌림(또는 단선)
            if (!io.Input(IOMap.Input.EMG))          //  ! 추가 (조건 반전)
            {
                Abort("비상정지(EMG) 스위치 감지");
                equipment.ChangeState(EquipmentState.Emergency);
                return;
            }
            if (!io.Input(IOMap.Input.MainPressure))
            {
                Abort("메인 공압 저하");
                return;
            }

            //    한 틱에 여러 스텝 연속 실행:
            //    지령/조건성립 스텝은 Step 이 바뀌므로 같은 틱에 이어서 처리하고,
            //    조건 미성립 Wait 스텝(Step 불변)에서는 루프가 멈춰 다음 틱을 기다린다.
            //    Abort() 시 AutoRun=false 가 되므로 while 조건에서 즉시 종료된다.
            int guard = 20;   // 한 틱 최대 20스텝, 무한루프 방지
            AutoStep before;
            do
            {
                before = Step;      // 실행 전 스텝 기억
                ExecuteStep();      // 현재 스텝 1개 실행
            }
            // 스텝이 바뀌었으면(=일이 진행됐으면) 같은 틱에 이어서 다음 스텝도 처리.
            // 스텝이 그대로면(=무언가 기다리는 중) 멈추고 다음 0.1초를 기다린다.
            // guard 로 한 틱에 최대 20번까지만 돌려 혹시 모를 무한반복을 막는다.
            while (AutoRun && Step != before && --guard > 0);
        }

        //    switch(Step) 상태머신 본체 (내용 무변경).
        //  switch = "지금 Step 값이 무엇이냐에 따라 갈라서 실행"하는 문법.
        //  각 case 는 하나의 단계다. 대부분 "동작을 시키고 → Next(다음스텝) 로 넘어가거나,
        //  Wait(...) 가 true 가 되면 다음으로 넘어간다". break 는 그 case 끝을 뜻한다.
        private void ExecuteStep()
        {
            switch (Step)
            {
                // ═══════════ 초기화 ═══════════
                case AutoStep.Idle:
                    equipment.ChangeState(EquipmentState.Initializing);
                    Next(AutoStep.ServoOn);
                    break;

                case AutoStep.ServoOn:
                    motion.ServoON();          // 이후 자동운전 종료까지 OFF 금지 (규칙 1)
                    Next(AutoStep.HomeUD);
                    break;

                case AutoStep.HomeUD:
                    // 포크가 전진 상태면 원점복귀 금지 (충돌 방지)
                    if (!robot.IsBackward())
                    {
                        Abort("이송 실린더가 후진 상태가 아님 - 원점복귀 불가");
                        break;
                    }
                    equipment.ChangeState(EquipmentState.Homing);
                    motion.HomeUD();           // 규칙 3: 반드시 상/하 먼저
                    Next(AutoStep.WaitUDHome);
                    break;

                case AutoStep.WaitUDHome:
                    if (Wait(motion.UDHomeDone, TimeoutMotion, "상/하축 원점복귀 타임아웃"))
                        Next(AutoStep.HomeLR);
                    break;

                case AutoStep.HomeLR:
                    motion.HomeLR();           // 상/하 원점 완료 후에만 실행
                    Next(AutoStep.WaitLRHome);
                    break;

                case AutoStep.WaitLRHome:
                    if (Wait(motion.LRHomeDone, TimeoutMotion, "좌/우축 원점복귀 타임아웃"))
                        Next(AutoStep.Ready);
                    break;

                case AutoStep.Ready:
                    equipment.ChangeState(EquipmentState.Running);
                    Next(AutoStep.Pick_MoveLR);
                    break;

                // ═══════════ FOUP A 픽업 ═══════════
                case AutoStep.Pick_MoveLR:
                    if (!robot.IsBackward())   // 좌/우 이동 전 포크 후진 인터록
                    {
                        Abort("포크 전진 상태에서 좌/우 이동 시도");
                        break;
                    }
                    motion.MoveLR(Position.FOUPA_X);
                    Next(AutoStep.Pick_WaitLR);
                    break;

                case AutoStep.Pick_WaitLR:
                    if (Wait(motion.LRMoveDone, TimeoutMotion, "FOUP A 좌/우 이동 타임아웃"))
                        Next(AutoStep.Pick_MoveDown);
                    break;

                case AutoStep.Pick_MoveDown:
                    motion.MoveUD(Position.FOUPA_DOWN[CurrentSlot]);   // 안착 위치
                    Next(AutoStep.Pick_WaitDown);
                    break;

                case AutoStep.Pick_WaitDown:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "FOUP A 안착위치 이동 타임아웃"))
                        Next(AutoStep.Pick_Forward);
                    break;

                case AutoStep.Pick_Forward:
                    robot.Forward();
                    Next(AutoStep.Pick_WaitForward);
                    break;

                case AutoStep.Pick_WaitForward:
                    if (Wait(robot.IsForward(), TimeoutCylinder, "이송 실린더 전진 타임아웃"))
                        Next(AutoStep.Pick_VacuumOn);
                    break;

                case AutoStep.Pick_VacuumOn:
                    robot.VacuumOn();
                    Next(AutoStep.Pick_WaitVacuum);
                    break;

                case AutoStep.Pick_WaitVacuum:
                    if (Wait(robot.IsVacuum(), TimeoutVacuum, "진공 흡착 실패"))
                        Next(AutoStep.Pick_MoveUp);
                    break;

                // 순서 중요: 들어올린 다음 후진해야 웨이퍼가 선반에 긁히지 않음
                case AutoStep.Pick_MoveUp:
                    motion.MoveUD(Position.FOUPA_UP[CurrentSlot]);     // 상승 위치
                    Next(AutoStep.Pick_WaitUp);
                    break;

                case AutoStep.Pick_WaitUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "FOUP A 상승 타임아웃"))
                        Next(AutoStep.Pick_Backward);
                    break;

                case AutoStep.Pick_Backward:
                    robot.Backward();
                    Next(AutoStep.Pick_WaitBackward);
                    break;

                case AutoStep.Pick_WaitBackward:
                    if (Wait(robot.IsBackward(), TimeoutCylinder, "이송 실린더 후진 타임아웃"))
                    {
                        CurWafer.Location = WaferLocation.Robot;
                        CurWafer.State = WaferState.Processing;
                        CurrentChamberIndex = 0;      // 챔버 A 부터
                        Next(AutoStep.Cham_DoorOpen);
                    }
                    break;

                // ═══════════ 챔버 안착 (A/B/C 공용) ═══════════
                case AutoStep.Cham_DoorOpen:
                    // 이송 중 진공 상실 감시
                    if (!robot.IsVacuum()) { Abort("이송 중 진공 상실"); break; }
                    Cham.OpenDoor();
                    Next(AutoStep.Cham_WaitDoorOpen);
                    break;

                case AutoStep.Cham_WaitDoorOpen:
                    if (Wait(Cham.IsDoorOpen, TimeoutCylinder,
                             $"챔버 {Cham.Type} 도어 열림 타임아웃"))
                        Next(AutoStep.Cham_MoveLR);
                    break;

                case AutoStep.Cham_MoveLR:
                    if (!robot.IsBackward()) { Abort("포크 전진 상태에서 좌/우 이동 시도"); break; }
                    motion.MoveLR(Cham.PositionX);
                    Next(AutoStep.Cham_WaitLR);
                    break;

                case AutoStep.Cham_WaitLR:
                    if (Wait(motion.LRMoveDone, TimeoutMotion,
                             $"챔버 {Cham.Type} 좌/우 이동 타임아웃"))
                        Next(AutoStep.Cham_MoveUp);
                    break;

                case AutoStep.Cham_MoveUp:
                    motion.MoveUD(Position.CHAMBER_UP);   // 상승(진입) 높이
                    Next(AutoStep.Cham_WaitUp);
                    break;

                case AutoStep.Cham_WaitUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "챔버 진입높이 이동 타임아웃"))
                        Next(AutoStep.Cham_Forward);
                    break;

                case AutoStep.Cham_Forward:
                    // 전진 직전 도어 열림 재확인 (충돌 방지 인터록)
                    if (!Cham.IsDoorOpen)
                    {
                        Abort($"챔버 {Cham.Type} 도어 열림 미확인 - 블레이드 전진 금지");
                        break;
                    }
                    robot.Forward();
                    Next(AutoStep.Cham_WaitForward);
                    break;

                //    진공을 유지한 채 안착 높이까지 하강하고,
                //    안착면 도달 직후 진공OFF + 배기ON 을 동시에 수행해 릴리즈한다.
                case AutoStep.Cham_WaitForward:
                    if (Wait(robot.IsForward(), TimeoutCylinder, "챔버 진입(전진) 타임아웃"))
                        Next(AutoStep.Cham_MoveDown);
                    break;

                case AutoStep.Cham_MoveDown:
                    motion.MoveUD(Position.CHAMBER_DOWN); // 진공 유지 상태로 안착 높이까지 하강
                    Next(AutoStep.Cham_WaitDown);
                    break;

                case AutoStep.Cham_WaitDown:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "챔버 안착 이동 타임아웃"))
                        Next(AutoStep.Cham_VacuumOff);   // 안착면 도달
                    break;

                case AutoStep.Cham_VacuumOff:
                    robot.VacuumOff();            // 안착 직후 진공 해제
                    robot.BlowOn();               // 동시에 배기 ON 으로 릴리즈
                    Next(AutoStep.Cham_Blow);
                    break;

                case AutoStep.Cham_Blow:
                    // 배기 유지시간 확보 (배기는 Cham_VacuumOff 에서 이미 ON)
                    if (++waitTicks >= Constants.BlowTimeMs / Constants.TimerInterval)
                        Next(AutoStep.Cham_BlowOff);
                    break;

                case AutoStep.Cham_BlowOff:
                    robot.BlowOff();
                    Next(AutoStep.Cham_Backward);
                    break;

                case AutoStep.Cham_Backward:
                    robot.Backward();
                    Next(AutoStep.Cham_WaitBackward);
                    break;

                case AutoStep.Cham_WaitBackward:
                    if (Wait(robot.IsBackward(), TimeoutCylinder, "챔버 이탈(후진) 타임아웃"))
                    {
                        Cham.HasWafer = true;
                        CurWafer.Location = (WaferLocation)((int)WaferLocation.ChamberA + CurrentChamberIndex);
                        CurWafer.CurrentProcess = Cham.Process;
                        Next(AutoStep.Cham_DoorClose);
                    }
                    break;

                case AutoStep.Cham_DoorClose:
                    Cham.CloseDoor();
                    Next(AutoStep.Cham_WaitDoorClose);
                    break;

                case AutoStep.Cham_WaitDoorClose:
                    if (Wait(Cham.IsDoorClosed, TimeoutCylinder,
                             $"챔버 {Cham.Type} 도어 닫힘 타임아웃"))
                        Next(AutoStep.Cham_ProcessStart);
                    break;

                // ═══════════ 챔버 공정 ═══════════
                case AutoStep.Cham_ProcessStart:
                    Cham.Busy = true;
                    Cham.LampOn();                 // 램프 ON = 공정 진행 표시
                    LogManager.Instance.WaferProcessStart(CurrentSlot, Cham.Type.ToString(), Cham.Process.ToString());  // 로깅 훅
                    processTicks = 0;
                    Next(AutoStep.Cham_ProcessWait);
                    break;

                case AutoStep.Cham_ProcessWait:
                    if (++processTicks >= Cham.ProcessTimeMs / Constants.TimerInterval)
                    {
                        Cham.LampOff();
                        Cham.Busy = false;
                        LogManager.Instance.WaferProcessEnd(CurrentSlot, Cham.Type.ToString(), Cham.Process.ToString());  // 로깅 훅
                        Next(AutoStep.Cham_DoorReopen);
                    }
                    break;

                // ═══════════ 챔버 픽업 ═══════════
                case AutoStep.Cham_DoorReopen:
                    Cham.OpenDoor();
                    Next(AutoStep.Cham_WaitDoorReopen);
                    break;

                case AutoStep.Cham_WaitDoorReopen:
                    if (Wait(Cham.IsDoorOpen, TimeoutCylinder,
                             $"챔버 {Cham.Type} 도어 재열림 타임아웃"))
                        Next(AutoStep.Cham_PickForward);
                    break;

                // 현재 UD 는 안착 높이(CHAMBER_DOWN) 그대로 → 바로 포크 삽입
                case AutoStep.Cham_PickForward:
                    // 픽업 전진 직전에도 도어 열림 재확인
                    if (!Cham.IsDoorOpen)
                    {
                        Abort($"챔버 {Cham.Type} 도어 열림 미확인 - 블레이드 전진 금지");
                        break;
                    }
                    robot.Forward();
                    Next(AutoStep.Cham_WaitPickForward);
                    break;

                case AutoStep.Cham_WaitPickForward:
                    if (Wait(robot.IsForward(), TimeoutCylinder, "챔버 픽업 전진 타임아웃"))
                        Next(AutoStep.Cham_PickVacuum);
                    break;

                case AutoStep.Cham_PickVacuum:
                    robot.VacuumOn();
                    Next(AutoStep.Cham_WaitPickVacuum);
                    break;

                case AutoStep.Cham_WaitPickVacuum:
                    if (Wait(robot.IsVacuum(), TimeoutVacuum, "챔버 픽업 진공 실패"))
                        Next(AutoStep.Cham_PickUp);
                    break;

                case AutoStep.Cham_PickUp:
                    motion.MoveUD(Position.CHAMBER_UP);   // 들어올리기
                    Next(AutoStep.Cham_WaitPickUp);
                    break;

                case AutoStep.Cham_WaitPickUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "챔버 픽업 상승 타임아웃"))
                        Next(AutoStep.Cham_PickBackward);
                    break;

                case AutoStep.Cham_PickBackward:
                    robot.Backward();
                    Next(AutoStep.Cham_WaitPickBackward);
                    break;

                case AutoStep.Cham_WaitPickBackward:
                    if (Wait(robot.IsBackward(), TimeoutCylinder, "챔버 픽업 후진 타임아웃"))
                    {
                        Cham.HasWafer = false;
                        CurWafer.Location = WaferLocation.Robot;
                        Next(AutoStep.Cham_DoorClose2);
                    }
                    break;

                case AutoStep.Cham_DoorClose2:
                    Cham.CloseDoor();
                    Next(AutoStep.Cham_WaitDoorClose2);
                    break;

                case AutoStep.Cham_WaitDoorClose2:
                    if (Wait(Cham.IsDoorClosed, TimeoutCylinder,
                             $"챔버 {Cham.Type} 도어 닫힘 타임아웃"))
                        Next(AutoStep.NextChamberOrUnload);
                    break;

                case AutoStep.NextChamberOrUnload:
                    if (CurrentChamberIndex < 2)
                    {
                        CurrentChamberIndex++;             // A → B → C
                        Next(AutoStep.Cham_DoorOpen);
                    }
                    else
                    {
                        Next(AutoStep.Unload_MoveLR);      // 현상까지 완료 → FOUP B
                    }
                    break;

                // ═══════════ FOUP B 언로드 ═══════════
                case AutoStep.Unload_MoveLR:
                    if (!robot.IsBackward()) { Abort("포크 전진 상태에서 좌/우 이동 시도"); break; }
                    if (!robot.IsVacuum()) { Abort("이송 중 진공 상실"); break; }
                    motion.MoveLR(Position.FOUPB_X);
                    Next(AutoStep.Unload_WaitLR);
                    break;

                case AutoStep.Unload_WaitLR:
                    if (Wait(motion.LRMoveDone, TimeoutMotion, "FOUP B 좌/우 이동 타임아웃"))
                        Next(AutoStep.Unload_MoveUp);
                    break;

                case AutoStep.Unload_MoveUp:
                    motion.MoveUD(Position.FOUPB_UP[CurrentSlot]);   // 상승 위치로 진입
                    Next(AutoStep.Unload_WaitUp);
                    break;

                case AutoStep.Unload_WaitUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "FOUP B 상승위치 이동 타임아웃"))
                        Next(AutoStep.Unload_Forward);
                    break;

                case AutoStep.Unload_Forward:
                    robot.Forward();
                    Next(AutoStep.Unload_WaitForward);
                    break;

                case AutoStep.Unload_WaitForward:
                    if (Wait(robot.IsForward(), TimeoutCylinder, "FOUP B 전진 타임아웃"))
                        Next(AutoStep.Unload_MoveDown);
                    break;

                case AutoStep.Unload_MoveDown:
                    motion.MoveUD(Position.FOUPB_DOWN[CurrentSlot]); // 안착 위치에 내려놓기
                    Next(AutoStep.Unload_WaitDown);
                    break;

                case AutoStep.Unload_WaitDown:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "FOUP B 안착 이동 타임아웃"))
                        Next(AutoStep.Unload_VacuumOff);
                    break;

                case AutoStep.Unload_VacuumOff:
                    robot.VacuumOff();
                    Next(AutoStep.Unload_Blow);
                    break;

                case AutoStep.Unload_Blow:
                    robot.BlowOn();
                    if (++waitTicks >= Constants.BlowTimeMs / Constants.TimerInterval)  // 2초
                        Next(AutoStep.Unload_BlowOff);
                    break;

                case AutoStep.Unload_BlowOff:
                    robot.BlowOff();
                    Next(AutoStep.Unload_Backward);
                    break;

                case AutoStep.Unload_Backward:
                    robot.Backward();
                    Next(AutoStep.Unload_WaitBackward);
                    break;

                case AutoStep.Unload_WaitBackward:
                    if (Wait(robot.IsBackward(), TimeoutCylinder, "FOUP B 후진 타임아웃"))
                    {
                        CurWafer.Location = WaferLocation.FOUPB;
                        CurWafer.State = WaferState.Completed;
                        CurWafer.ProcessCompleted = true;
                        LogManager.Instance.WaferUnloaded(CurrentSlot);   // 로깅 훅 (FOUP B 안착 완료)
                        Next(AutoStep.Unload_MoveSafeUp);
                    }
                    break;

                case AutoStep.Unload_MoveSafeUp:
                    motion.MoveUD(Position.FOUPB_UP[CurrentSlot]);   // 간섭 없는 높이로 복귀
                    Next(AutoStep.Unload_WaitSafeUp);
                    break;

                case AutoStep.Unload_WaitSafeUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "FOUP B 복귀 상승 타임아웃"))
                        Next(AutoStep.NextSlotOrComplete);
                    break;

                case AutoStep.NextSlotOrComplete:
                    if (CurrentSlot < 5)
                    {
                        CurrentSlot++;                     // 다음 웨이퍼
                        Next(AutoStep.Pick_MoveLR);
                    }
                    else
                    {
                        Next(AutoStep.Complete);
                    }
                    break;

                case AutoStep.Complete:
                    equipment.ChangeState(EquipmentState.Complete);
                    AutoRun = false;
                    OnAutoComplete?.Invoke();   // 로깅/Lot 완료 훅
                    // 규칙 1: 서보는 OFF 하지 않고 유지
                    break;

                case AutoStep.AlarmStop:
                    // 알람 상태 유지. Reset() 호출 전까지 정지.
                    break;
            }
        }

        // ────────────────────────────────────────────────
        // 조건 대기 + 타임아웃. 조건 성립 시 true 반환.
        // 뜻: "condition(원하는 상태)이 될 때까지 기다려라. 단 너무 오래(timeoutTicks) 걸리면
        //      알람을 내고 멈춰라." 각 0.1초마다 waitTicks 를 1씩 늘려 시간을 센다.
        private bool Wait(bool condition, int timeoutTicks, string timeoutMessage)
        {
            if (condition)          // 원하는 상태가 됐으면
            {
                waitTicks = 0;      // 시간 카운터 초기화하고
                return true;        // "완료!" 알림
            }
            if (++waitTicks > timeoutTicks)   // 아직인데 제한시간을 넘었으면
            {
                Abort(timeoutMessage);        // 타임아웃 알람 내고 정지
            }
            return false;           // 아직 완료 아님(다음 틱에 또 확인)
        }

        // 다음 스텝으로 넘어가기. 시간 카운터를 0으로 리셋하고 Step 을 바꾼다.
        private void Next(AutoStep next)
        {
            waitTicks = 0;
            Step = next;
        }

        // 이상 상황에서 안전하게 멈추기(비상정지·타임아웃 등).
        // 출력을 끄고, 알람을 켜고, 상태를 Alarm 으로 바꾸고, 자동운전을 끈다.
        private void Abort(string message)
        {
            // 모든 출력 안전 정지 (서보는 OFF 하지 않음 - 규칙 1)
            robot.BlowOff();
            alarm.SetAlarm(message);
            equipment.ChangeState(EquipmentState.Alarm);
            AutoRun = false;
            Step = AutoStep.AlarmStop;
            OnAutoAbort?.Invoke(message);   // 로깅/Lot 알람정지 훅
        }

        // 타워램프: 적=알람/EMG, 녹=자동운전
        
        private void UpdateTowerLamp()
        {
            // 알람/비상: 수동보다 우선하여 적색만 점등
            if (equipment.IsAlarm || equipment.IsEmergency)
            {
                io.Output(IOMap.Output.TowerRed, true);
                io.Output(IOMap.Output.TowerYellow, false);
                io.Output(IOMap.Output.TowerGreen, false);
                return;
            }

            // 자동운전 중: 녹색 점등
            if (AutoRun)
            {
                io.Output(IOMap.Output.TowerRed, false);
                io.Output(IOMap.Output.TowerYellow, false);
                io.Output(IOMap.Output.TowerGreen, true);
                return;
            }

            // 대기 / 정지 / 완료: 자동 제어하지 않음 → 수동 램프 버튼 유효
        }
    }
}
