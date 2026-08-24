using System;
using EtherCAT_Test.Alarm;
using EtherCAT_Test.Chamber;
using EtherCAT_Test.Common;
using EtherCAT_Test.Equipment;
using EtherCAT_Test.IO;
using EtherCAT_Test.Motion;
using EtherCAT_Test.Process;
using EtherCAT_Test.Robot;

namespace EtherCAT_Test.Sequence
{
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

        public void Start()
        {
            if (AutoRun) return;
            alarm.Clear();
            CurrentSlot = 1;
            CurrentChamberIndex = 0;
            waitTicks = 0;
            Step = AutoStep.Idle;
            AutoRun = true;
        }

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

        private EtherCAT_Test.Chamber.Chamber Cham
            => chamberMgr.Chambers[CurrentChamberIndex];
        private Wafer CurWafer => waferMgr.GetWafer(CurrentSlot);

        // ────────────────────────────────────────────────
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

                case AutoStep.Cham_WaitForward:
                    if (Wait(robot.IsForward(), TimeoutCylinder, "챔버 진입(전진) 타임아웃"))
                        Next(AutoStep.Cham_MoveDown);
                    break;

                case AutoStep.Cham_MoveDown:
                    motion.MoveUD(Position.CHAMBER_DOWN); // 안착 높이로 내려놓기
                    Next(AutoStep.Cham_WaitDown);
                    break;

                case AutoStep.Cham_WaitDown:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "챔버 안착 이동 타임아웃"))
                        Next(AutoStep.Cham_VacuumOff);
                    break;

                case AutoStep.Cham_VacuumOff:
                    robot.VacuumOff();
                    Next(AutoStep.Cham_Blow);
                    break;

                case AutoStep.Cham_Blow:
                    robot.BlowOn();               // 파기: 웨이퍼 분리 보조
                    if (++waitTicks >= 500 / Constants.TimerInterval)  // 0.5초
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
                    processTicks = 0;
                    Next(AutoStep.Cham_ProcessWait);
                    break;

                case AutoStep.Cham_ProcessWait:
                    if (++processTicks >= Cham.ProcessTimeMs / Constants.TimerInterval)
                    {
                        Cham.LampOff();
                        Cham.Busy = false;
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
                    if (++waitTicks >= 500 / Constants.TimerInterval)
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
                    // 규칙 1: 서보는 OFF 하지 않고 유지
                    break;

                case AutoStep.AlarmStop:
                    // 알람 상태 유지. Reset() 호출 전까지 정지.
                    break;
            }
        }

        // ────────────────────────────────────────────────
        // 조건 대기 + 타임아웃. 조건 성립 시 true 반환.
        private bool Wait(bool condition, int timeoutTicks, string timeoutMessage)
        {
            if (condition)
            {
                waitTicks = 0;
                return true;
            }
            if (++waitTicks > timeoutTicks)
            {
                Abort(timeoutMessage);
            }
            return false;
        }

        private void Next(AutoStep next)
        {
            waitTicks = 0;
            Step = next;
        }

        private void Abort(string message)
        {
            // 모든 출력 안전 정지 (서보는 OFF 하지 않음 - 규칙 1)
            robot.BlowOff();
            alarm.SetAlarm(message);
            equipment.ChangeState(EquipmentState.Alarm);
            AutoRun = false;
            Step = AutoStep.AlarmStop;
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
