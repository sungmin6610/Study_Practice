using System;
using System.Linq;
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
    /// <summary>
    /// 포토공정 자동 시퀀스 — 수정 2: 파이프라이닝.
    ///  · 챔버는 자체 상태(ChamberProcState)로 공정을 스스로 진행(TickProcess).
    ///  · 로봇은 "From→To" 범용 이송 잡 하나로 통합된 상태머신.
    ///  · 스케줄러가 유휴 로봇에 다운스트림 우선으로 잡을 배정(데드락 방지).
    /// 안전(EMG/압력/인터록/타임아웃/원점순서/서보유지)은 전부 유지.
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

        // 현재 잡 / 챔버 재실 슬롯 추적
        private TransferJob job;
        private readonly int[] cSlot = new int[3];   // 챔버 A/B/C 에 든 웨이퍼 슬롯번호(0=없음)

        // ── HMI 호환용 공개 속성(시그니처 유지) ──
        public int CurrentSlot => job?.Slot ?? 0;
        public int CurrentChamberIndex => 0;
        public string CurrentJobText => job != null ? job.ToString() : (AutoRun ? "대기(스케줄)" : "-");
        public int CurrentProcessElapsedMs
        {
            get
            {
                foreach (var c in chamberMgr.Chambers)
                    if (c.ProcState == ChamberProcState.Processing) return c.ElapsedMs;
                return 0;
            }
        }

        // ── 타임아웃 관리 ──
        private int waitTicks;
        private int TimeoutMotion => 15000 / Constants.TimerInterval;
        private int TimeoutCylinder => 3000 / Constants.TimerInterval;
        private int TimeoutVacuum => 2000 / Constants.TimerInterval;

        public Action OnAutoStart;
        public Action OnAutoComplete;
        public Action<string> OnAutoAbort;

        public AutoSequence(IOManager ioManager, MotionManager motionManager, RobotManager robotManager,
                            ChamberManager chamberManager, EquipmentStateManager equipmentManager,
                            WaferManager waferManager, AlarmManager alarmManager)
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

            // 새 Lot: 챔버/웨이퍼/잡 초기화
            foreach (var c in chamberMgr.Chambers) c.ResetProc();
            for (int i = 0; i < 3; i++) cSlot[i] = 0;
            for (int s = 1; s <= 5; s++)
            {
                var wf = waferMgr.GetWafer(s);
                if (wf != null) { wf.Location = WaferLocation.FOUPA; wf.State = WaferState.Ready; wf.ProcessCompleted = false; }
            }
            job = null;
            waitTicks = 0;
            Step = AutoStep.Idle;
            AutoRun = true;
            OnAutoStart?.Invoke();
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
            job = null;
            alarm.Clear();
            equipment.ChangeState(EquipmentState.Idle);
        }

        // ────────────────────────────────────────────────
        public void Run()
        {
            UpdateTowerLamp();
            if (!AutoRun) return;

            // 안전 인터록 (매 스캔 최우선)
            if (!io.Input(IOMap.Input.EMG))
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

            // 챔버 독립 공정 진행 (로봇과 무관, 매 스캔 1회)
            foreach (var c in chamberMgr.Chambers) c.TickProcess(Constants.TimerInterval);

            // 로봇 상태머신 (한 틱 여러 스텝 연속)
            int guard = 30;
            AutoStep before;
            do
            {
                before = Step;
                ExecuteStep();
            }
            while (AutoRun && Step != before && --guard > 0);
        }

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
                    motion.ServoON();
                    Next(AutoStep.HomeUD);
                    break;

                case AutoStep.HomeUD:
                    if (!robot.IsBackward()) { Abort("이송 실린더가 후진 상태가 아님 - 원점복귀 불가"); break; }
                    equipment.ChangeState(EquipmentState.Homing);
                    motion.HomeUD();                 // 규칙 3: 상/하 먼저
                    Next(AutoStep.WaitUDHome);
                    break;

                case AutoStep.WaitUDHome:
                    if (Wait(motion.UDHomeDone, TimeoutMotion, "상/하축 원점복귀 타임아웃"))
                        Next(AutoStep.HomeLR);
                    break;

                case AutoStep.HomeLR:
                    motion.HomeLR();                 // 상/하 원점 후에만
                    Next(AutoStep.WaitLRHome);
                    break;

                case AutoStep.WaitLRHome:
                    if (Wait(motion.LRHomeDone, TimeoutMotion, "좌/우축 원점복귀 타임아웃"))
                        Next(AutoStep.Ready);
                    break;

                case AutoStep.Ready:
                    equipment.ChangeState(EquipmentState.Running);
                    Next(AutoStep.Schedule);
                    break;

                // ═══════════ 스케줄러 ═══════════
                case AutoStep.Schedule:
                    equipment.ChangeState(EquipmentState.Running);
                    if (AllAtFoupB()) { Next(AutoStep.Complete); break; }
                    job = PickNextJob();
                    if (job == null) break;          // 유휴: Step 그대로 → 다음 틱 재시도 (챔버는 계속 공정)
                    LogManager.Instance.Info("JOB 시작 - " + job);
                    Next(AutoStep.P_MoveLR);
                    break;

                // ═══════════ 픽업 (job.From) ═══════════
                case AutoStep.P_MoveLR:
                    if (!robot.IsBackward()) { Abort("포크 전진 상태에서 좌/우 이동 시도(픽업)"); break; }
                    motion.MoveLR(StationLR(job.From));
                    Next(AutoStep.P_WaitLR);
                    break;

                case AutoStep.P_WaitLR:
                    if (Wait(motion.LRMoveDone, TimeoutMotion, "픽업 좌/우 이동 타임아웃"))
                        Next(AutoStep.P_DoorOpen);
                    break;

                case AutoStep.P_DoorOpen:
                    if (ChamberOf(job.From) is EtherCAT_Test.Chamber.Chamber pch)
                    {
                        if (!robot.IsBackward()) { Abort("도어 열기 전 후진 미확인(픽업)"); break; }
                        pch.OpenDoor();
                        Next(AutoStep.P_WaitDoorOpen);
                    }
                    else Next(AutoStep.P_MoveDown);   // FOUP: 도어 없음
                    break;

                case AutoStep.P_WaitDoorOpen:
                    if (Wait(ChamberOf(job.From).IsDoorOpen, TimeoutCylinder, "픽업 도어 열림 타임아웃"))
                        Next(AutoStep.P_MoveDown);
                    break;

                case AutoStep.P_MoveDown:
                    motion.MoveUD(UD_Down(job.From, job.Slot));   // 안착높이
                    Next(AutoStep.P_WaitDown);
                    break;

                case AutoStep.P_WaitDown:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "픽업 안착높이 이동 타임아웃"))
                        Next(AutoStep.P_Forward);
                    break;

                case AutoStep.P_Forward:
                    {
                        var ch = ChamberOf(job.From);
                        if (ch != null && !ch.IsDoorOpen) { Abort("픽업 도어 열림 미확인 - 전진 금지"); break; }
                        robot.Forward();
                        Next(AutoStep.P_WaitForward);
                    }
                    break;

                case AutoStep.P_WaitForward:
                    if (Wait(robot.IsForward(), TimeoutCylinder, "픽업 전진 타임아웃"))
                        Next(AutoStep.P_VacOn);
                    break;

                case AutoStep.P_VacOn:
                    robot.VacuumOn();
                    Next(AutoStep.P_WaitVac);
                    break;

                case AutoStep.P_WaitVac:
                    if (Wait(robot.IsVacuum(), TimeoutVacuum, "픽업 진공 흡착 실패"))
                        Next(AutoStep.P_MoveUp);
                    break;

                case AutoStep.P_MoveUp:
                    motion.MoveUD(UD_Up(job.From, job.Slot));   // 상승높이(들어올림)
                    Next(AutoStep.P_WaitUp);
                    break;

                case AutoStep.P_WaitUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "픽업 상승 타임아웃"))
                        Next(AutoStep.P_Backward);
                    break;

                case AutoStep.P_Backward:
                    robot.Backward();
                    Next(AutoStep.P_WaitBackward);
                    break;

                case AutoStep.P_WaitBackward:
                    if (Wait(robot.IsBackward(), TimeoutCylinder, "픽업 후진 타임아웃"))
                    {
                        var wf = waferMgr.GetWafer(job.Slot);
                        if (wf != null) { wf.Location = WaferLocation.Robot; wf.State = WaferState.Processing; }
                        var ch = ChamberOf(job.From);
                        if (ch != null) { ch.ClearAfterPick(); cSlot[ChIdx(job.From)] = 0; }
                        Next(AutoStep.P_DoorClose);
                    }
                    break;

                case AutoStep.P_DoorClose:
                    if (ChamberOf(job.From) is EtherCAT_Test.Chamber.Chamber cch)
                    {
                        cch.CloseDoor();
                        Next(AutoStep.P_WaitDoorClose);
                    }
                    else Next(AutoStep.Q_MoveLR);
                    break;

                case AutoStep.P_WaitDoorClose:
                    if (Wait(ChamberOf(job.From).IsDoorClosed, TimeoutCylinder, "픽업 도어 닫힘 타임아웃"))
                        Next(AutoStep.Q_MoveLR);
                    break;

                // ═══════════ 안착 (job.To) ═══════════
                case AutoStep.Q_MoveLR:
                    if (!robot.IsBackward()) { Abort("포크 전진 상태에서 좌/우 이동 시도(안착)"); break; }
                    if (!robot.IsVacuum()) { Abort("이송 중 진공 상실"); break; }
                    motion.MoveLR(StationLR(job.To));
                    Next(AutoStep.Q_WaitLR);
                    break;

                case AutoStep.Q_WaitLR:
                    if (Wait(motion.LRMoveDone, TimeoutMotion, "안착 좌/우 이동 타임아웃"))
                        Next(AutoStep.Q_DoorOpen);
                    break;

                case AutoStep.Q_DoorOpen:
                    if (ChamberOf(job.To) is EtherCAT_Test.Chamber.Chamber qch)
                    {
                        if (!robot.IsVacuum()) { Abort("이송 중 진공 상실"); break; }
                        qch.OpenDoor();
                        Next(AutoStep.Q_WaitDoorOpen);
                    }
                    else Next(AutoStep.Q_MoveUp);
                    break;

                case AutoStep.Q_WaitDoorOpen:
                    if (Wait(ChamberOf(job.To).IsDoorOpen, TimeoutCylinder, "안착 도어 열림 타임아웃"))
                        Next(AutoStep.Q_MoveUp);
                    break;

                case AutoStep.Q_MoveUp:
                    motion.MoveUD(UD_Up(job.To, job.Slot));   // 상승높이
                    Next(AutoStep.Q_WaitUp);
                    break;

                case AutoStep.Q_WaitUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "안착 상승높이 이동 타임아웃"))
                        Next(AutoStep.Q_Forward);
                    break;

                case AutoStep.Q_Forward:
                    {
                        var ch = ChamberOf(job.To);
                        if (ch != null && !ch.IsDoorOpen) { Abort("안착 도어 열림 미확인 - 전진 금지"); break; }
                        robot.Forward();
                        Next(AutoStep.Q_WaitForward);
                    }
                    break;

                case AutoStep.Q_WaitForward:
                    if (Wait(robot.IsForward(), TimeoutCylinder, "안착 전진 타임아웃"))
                        Next(AutoStep.Q_MoveDown);
                    break;

                case AutoStep.Q_MoveDown:
                    motion.MoveUD(UD_Down(job.To, job.Slot));   // 안착높이(내려놓음)
                    Next(AutoStep.Q_WaitDown);
                    break;

                case AutoStep.Q_WaitDown:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "안착 하강 타임아웃"))
                        Next(AutoStep.Q_VacOff);
                    break;

                case AutoStep.Q_VacOff:
                    robot.VacuumOff();
                    robot.BlowOn();               // 진공OFF + 배기ON 동시 (릴리즈)
                    Next(AutoStep.Q_Blow);
                    break;

                case AutoStep.Q_Blow:
                    if (++waitTicks >= Constants.BlowTimeMs / Constants.TimerInterval)
                        Next(AutoStep.Q_BlowOff);
                    break;

                case AutoStep.Q_BlowOff:
                    robot.BlowOff();
                    Next(AutoStep.Q_Backward);
                    break;

                case AutoStep.Q_Backward:
                    robot.Backward();
                    Next(AutoStep.Q_WaitBackward);
                    break;

                case AutoStep.Q_WaitBackward:
                    if (Wait(robot.IsBackward(), TimeoutCylinder, "안착 후진 타임아웃"))
                    {
                        var wf = waferMgr.GetWafer(job.Slot);
                        var ch = ChamberOf(job.To);
                        if (ch != null)
                        {
                            ch.HasWafer = true;
                            ch.SetLoaded();
                            cSlot[ChIdx(job.To)] = job.Slot;
                            if (wf != null) { wf.Location = WaferLocOf(job.To); wf.CurrentProcess = ch.Process; }
                            Next(AutoStep.Q_DoorClose);
                        }
                        else   // FOUP B 안착 완료
                        {
                            if (wf != null) { wf.Location = WaferLocation.FOUPB; wf.State = WaferState.Completed; wf.ProcessCompleted = true; }
                            LogManager.Instance.WaferUnloaded(job.Slot);
                            Next(AutoStep.Q_SafeUp);
                        }
                    }
                    break;

                case AutoStep.Q_DoorClose:
                    ChamberOf(job.To).CloseDoor();
                    Next(AutoStep.Q_WaitDoorClose);
                    break;

                case AutoStep.Q_WaitDoorClose:
                    if (Wait(ChamberOf(job.To).IsDoorClosed, TimeoutCylinder, "안착 도어 닫힘 타임아웃"))
                    {
                        ChamberOf(job.To).StartProcess(job.Slot);   // 챔버 독립 공정 시작
                        Next(AutoStep.JobDone);
                    }
                    break;

                case AutoStep.Q_SafeUp:
                    motion.MoveUD(UD_Up(job.To, job.Slot));   // FOUP B 복귀 상승
                    Next(AutoStep.Q_WaitSafeUp);
                    break;

                case AutoStep.Q_WaitSafeUp:
                    if (Wait(motion.UDMoveDone, TimeoutMotion, "FOUP B 복귀 상승 타임아웃"))
                        Next(AutoStep.JobDone);
                    break;

                case AutoStep.JobDone:
                    LogManager.Instance.Info("JOB 완료 - " + job);
                    job = null;
                    Next(AutoStep.Schedule);
                    break;

                // ═══════════ 종료 ═══════════
                case AutoStep.Complete:
                    equipment.ChangeState(EquipmentState.Complete);
                    AutoRun = false;
                    OnAutoComplete?.Invoke();
                    break;

                case AutoStep.AlarmStop:
                    break;
            }
        }

        // ═══════════ 스케줄러 (다운스트림 우선 = 데드락 방지) ═══════════
        private TransferJob PickNextJob()
        {
            var A = chamberMgr.ChamberA;
            var B = chamberMgr.ChamberB;
            var C = chamberMgr.ChamberC;

            // 1) C 완료 → FOUP B 언로드 (원래 슬롯)
            if (C.ProcState == ChamberProcState.Done && cSlot[2] > 0)
                return new TransferJob(Station.ChamberC, Station.FOUPB, cSlot[2]);

            // 2) B 완료 + C 비어있음 → B→C
            if (B.ProcState == ChamberProcState.Done && C.ProcState == ChamberProcState.Empty && cSlot[1] > 0)
                return new TransferJob(Station.ChamberB, Station.ChamberC, cSlot[1]);

            // 3) A 완료 + B 비어있음 → A→B
            if (A.ProcState == ChamberProcState.Done && B.ProcState == ChamberProcState.Empty && cSlot[0] > 0)
                return new TransferJob(Station.ChamberA, Station.ChamberB, cSlot[0]);

            // 4) FOUP A 미처리 웨이퍼 + A 비어있음 → FOUP A 투입
            int s = NextFoupAWaferSlot();
            if (s > 0 && A.ProcState == ChamberProcState.Empty)
                return new TransferJob(Station.FOUPA, Station.ChamberA, s);

            return null;   // 유휴
        }

        private int NextFoupAWaferSlot()
        {
            for (int s = 1; s <= 5; s++)
            {
                var wf = waferMgr.GetWafer(s);
                if (wf != null && wf.Location == WaferLocation.FOUPA && wf.State == WaferState.Ready)
                    return s;
            }
            return 0;
        }

        private bool AllAtFoupB()
        {
            for (int s = 1; s <= 5; s++)
            {
                var wf = waferMgr.GetWafer(s);
                if (wf == null || wf.Location != WaferLocation.FOUPB) return false;
            }
            return true;
        }

        // ═══════════ 스테이션 테이블 ═══════════
        private long StationLR(Station s)
        {
            switch (s)
            {
                case Station.FOUPA: return Position.FOUPA_X;
                case Station.ChamberA: return Position.CHAMBER_A_X;
                case Station.ChamberB: return Position.CHAMBER_B_X;
                case Station.ChamberC: return Position.CHAMBER_C_X;
                default: return Position.FOUPB_X;
            }
        }

        private long UD_Up(Station s, int slot)   // 상승높이
        {
            switch (s)
            {
                case Station.FOUPA: return Position.FOUPA_UP[slot];
                case Station.FOUPB: return Position.FOUPB_UP[slot];
                default: return Position.CHAMBER_UP;
            }
        }

        private long UD_Down(Station s, int slot) // 안착높이
        {
            switch (s)
            {
                case Station.FOUPA: return Position.FOUPA_DOWN[slot];
                case Station.FOUPB: return Position.FOUPB_DOWN[slot];
                default: return Position.CHAMBER_DOWN;
            }
        }

        private EtherCAT_Test.Chamber.Chamber ChamberOf(Station s)
        {
            switch (s)
            {
                case Station.ChamberA: return chamberMgr.ChamberA;
                case Station.ChamberB: return chamberMgr.ChamberB;
                case Station.ChamberC: return chamberMgr.ChamberC;
                default: return null;   // FOUP: 도어 없음
            }
        }

        private int ChIdx(Station s)
            => s == Station.ChamberA ? 0 : s == Station.ChamberB ? 1 : 2;

        private WaferLocation WaferLocOf(Station s)
        {
            switch (s)
            {
                case Station.FOUPA: return WaferLocation.FOUPA;
                case Station.ChamberA: return WaferLocation.ChamberA;
                case Station.ChamberB: return WaferLocation.ChamberB;
                case Station.ChamberC: return WaferLocation.ChamberC;
                default: return WaferLocation.FOUPB;
            }
        }

        // ────────────────────────────────────────────────
        private bool Wait(bool condition, int timeoutTicks, string timeoutMessage)
        {
            if (condition) { waitTicks = 0; return true; }
            if (++waitTicks > timeoutTicks) Abort(timeoutMessage);
            return false;
        }

        private void Next(AutoStep next)
        {
            waitTicks = 0;
            Step = next;
        }

        private void Abort(string message)
        {
            robot.BlowOff();
            // 진행 중이던 잡(로봇 적재 웨이퍼 포함) 기록
            string ctx = job != null ? $" | 진행잡: {job}" : "";
            alarm.SetAlarm(message + ctx);
            equipment.ChangeState(EquipmentState.Alarm);
            AutoRun = false;
            Step = AutoStep.AlarmStop;
            OnAutoAbort?.Invoke(message + ctx);
        }

        private void UpdateTowerLamp()
        {
            if (equipment.IsAlarm || equipment.IsEmergency)
            {
                io.Output(IOMap.Output.TowerRed, true);
                io.Output(IOMap.Output.TowerYellow, false);
                io.Output(IOMap.Output.TowerGreen, false);
                return;
            }
            if (AutoRun)
            {
                io.Output(IOMap.Output.TowerRed, false);
                io.Output(IOMap.Output.TowerYellow, false);
                io.Output(IOMap.Output.TowerGreen, true);
                return;
            }
        }
    }
}
