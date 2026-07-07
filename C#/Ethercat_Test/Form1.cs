using IEG3268_Dll;
using System;
using System.Drawing;
using System.Windows.Forms;
using EtherCAT_Test.Common;
using EtherCAT_Test.IO;
using EtherCAT_Test.Equipment;
using EtherCAT_Test.Motion;
using EtherCAT_Test.Robot;
using EtherCAT_Test.Chamber;
using EtherCAT_Test.Alarm;
using EtherCAT_Test.Process;
using EtherCAT_Test.Sequence;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일 = 메인 화면(HMI)]
    //  사용자가 보는 메인 창. 버튼/램프/상태표시를 담당하며, 앞에서 만든 모든
    //  매니저(io, motion, robot ...)를 '소유'하고 하나로 엮는 중심이다.
    //  partial class = 하나의 Form1 클래스를 여러 파일(.cs)로 나눠 적은 것.
    //    (Form1.UI.cs, Form1.Mimic.cs, Form1.Auth.cs ... 전부 같은 Form1 이다)
    //  ': Form' = 윈도우 창의 기본 기능을 물려받는다(상속). 이 프로그램의 유일한 '진짜 상속' 중 하나.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1 : Form
    {
        // EtherCAT 하드웨어와 통신하는 외부 DLL 객체. 아래 매니저들이 공유해서 쓴다.
        IEG3268 EtherCAT_M = new IEG3268();

        // 이 화면이 들고 있는 매니저들(각자 맡은 일이 다르다). 위 계층이 아래 계층을 '사용'.
        private IOManager io;                      // 입출력 신호
        private EquipmentStateManager equipment;   // 장비 상태
        private MotionManager motion;              // 서보 2축
        private RobotManager robot;                // 이송 실린더/진공
        private ChamberManager chamber;            // 챔버 3개
        private AlarmManager alarm;                // 알람
        private WaferManager waferManager;         // 웨이퍼 5장
        private AutoSequence autoSequence;         // 자동운전 두뇌
        private RecipeManager recipeManager;       // 레시피

        // 생성자: 창이 만들어질 때 매니저들을 순서대로 생성해 서로 연결한다.
        // 이 '연결(조립)'이 곧 계층구조다: 위 상자가 아래 상자를 재료로 받아 만들어진다.
        public Form1()
        {
            InitializeComponent();   // 디자이너로 배치한 버튼/라벨 등을 실제로 만드는 자동생성 코드

            io = new IOManager(EtherCAT_M);               // 통신 객체를 넣어 IO 창구 생성
            equipment = new EquipmentStateManager();
            motion = new MotionManager(EtherCAT_M);
            robot = new RobotManager(io);                 // 로봇은 io 를 받아서 사용
            recipeManager = new RecipeManager();          // 레시피 로드(없으면 기본 SCA 생성)
            chamber = new ChamberManager(io, recipeManager);  // 챔버는 io+레시피를 받아서 사용
            alarm = new AlarmManager();
            waferManager = new WaferManager();

            //  AutoSequence 생성자에 io 추가 (타워램프/EMG 인터록용)
            //  자동운전 두뇌에 필요한 매니저들을 전부 넘겨 조립한다(의존성 주입).
            autoSequence = new AutoSequence(io, motion, robot, chamber,
                                            equipment, waferManager, alarm);

            //  디자이너에서 timer1.Enabled = true 로 되어 있어도
            //  연결 전에는 절대 돌지 않도록 여기서 확실히 정지
            timer1.Stop();
            timer1.Interval = Constants.TimerInterval;   // 100ms 마다 timer1_Tick 실행되도록 설정
        }

        // ══════════════ 통신 연결 / 해제 ══════════════

        // [연결 ON 버튼] EtherCAT 하드웨어에 접속 시도.
        // 이벤트 함수 형태(object sender, EventArgs e)는 "이 버튼을 눌렀을 때 실행"이라는 뜻.
        private void button1_Click(object sender, EventArgs e)
        {
            if (EtherCAT_M.CIFX_50RE_Connect() == true)   // 접속 성공하면
            {
                label2.Text = "Connect OK";
                EtherCAT_M.ReadData_Send_Start(100);  // Timer Interval Set
                EtherCAT_M.ReadData_Timer_Start();    // Timer Start

                equipment.ChangeState(EquipmentState.Idle);  // 상태를 '대기'로
                isConnected = true;                   // 수정 2: 연결 상태 플래그
                timer1.Start();                       // 연결 성공 후에만 UI/시퀀스 타이머 시작
                UpdateControlEnable();                // 버튼 활성/비활성 갱신
            }
            else
            {
                label2.Text = "NG";                   // 실패 표시
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();                            // 통신 끊기 전 타이머부터 정지
            autoSequence.Stop();
            EtherCAT_M.CIFX_50RE_Disconnect();
            equipment.ChangeState(EquipmentState.PowerOff);
            label2.Text = "Disconnect";
            isConnected = false;                      // 수정 2: 연결 해제
            UpdateControlEnable();                    // 타이머 정지 후 즉시 버튼 상태 갱신
        }

        // ══════════════ 타워 램프 수동 (테스트용) ══════════════
        // 자동운전 중에는 AutoSequence 가 매 스캔 램프를 덮어쓰므로
        // 수동 버튼은 대기(Idle) 상태에서만 의미가 있음.

        private void button3_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerRed, true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerRed, false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerYellow, true);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerYellow, false);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerGreen, true);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerGreen, false);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerRed, true);
            io.Output(IOMap.Output.TowerYellow, true);
            io.Output(IOMap.Output.TowerGreen, true);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            io.Output(IOMap.Output.TowerRed, false);
            io.Output(IOMap.Output.TowerYellow, false);
            io.Output(IOMap.Output.TowerGreen, false);
        }

        // ══════════════ 챔버 수동 (Chamber 객체 사용) ══════════════

        private void ChamA_ON_Click(object sender, EventArgs e)
        {
            chamber.ChamberA.LampOn();
        }

        private void ChamA_OFF_Click(object sender, EventArgs e)
        {
            chamber.ChamberA.LampOff();
        }

        private void ChamA_UP_Click(object sender, EventArgs e)
        {
            chamber.ChamberA.OpenDoor();
        }

        private void ChamA_DOWN_Click(object sender, EventArgs e)
        {
            chamber.ChamberA.CloseDoor();
        }

        private void ChamB_ON_Click(object sender, EventArgs e)
        {
            chamber.ChamberB.LampOn();
        }

        private void ChamB_OFF_Click(object sender, EventArgs e)
        {
            chamber.ChamberB.LampOff();
        }

        private void ChamB_UP_Click(object sender, EventArgs e)
        {
            chamber.ChamberB.OpenDoor();
        }

        private void ChamB_DOWN_Click(object sender, EventArgs e)
        {
            chamber.ChamberB.CloseDoor();
        }

        private void ChamC_ON_Click(object sender, EventArgs e)
        {
            chamber.ChamberC.LampOn();
        }

        private void ChamC_OFF_Click(object sender, EventArgs e)
        {
            chamber.ChamberC.LampOff();
        }

        private void ChamC_UP_Click(object sender, EventArgs e)
        {
            chamber.ChamberC.OpenDoor();
        }

        private void ChamC_DOWN_Click(object sender, EventArgs e)
        {
            chamber.ChamberC.CloseDoor();
        }

        // ══════════════ 웨이퍼 이송 로봇 수동 ══════════════

        private void button11_Click(object sender, EventArgs e)
        {            
            robot.Forward();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            robot.Backward();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            robot.VacuumOn();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            robot.VacuumOff();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            robot.BlowOn();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            robot.BlowOff();
        }

        // ══════════════ 서보 ON / OFF ══════════════

        private void button17_Click(object sender, EventArgs e)
        {
            motion.ServoON();
            MessageBox.Show("Servo ON 완료");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            // 규칙 1: 자동운전(서보 제어) 중에는 서보 OFF 금지
            if (autoSequence.AutoRun)
            {
                MessageBox.Show("자동운전 중에는 서보를 OFF 할 수 없습니다.\n\n" +
                                "자동운전을 정지한 후 다시 시도하세요.");
                return;
            }
            motion.ServoOFF();
            MessageBox.Show("Servo OFF");
        }

        // ══════════════ 원점 복귀 ══════════════

        // 상/하 원점 복귀
        private void button19_Click(object sender, EventArgs e)
        {            
            if (robot.IsBackward())
            {
                MessageBox.Show("상/하 원점복귀 시작");
                motion.HomeUD();
            }
            else
            {
                MessageBox.Show("웨이퍼 이송 실린더가 후진 확인되지 않았습니다." + "\n\n" +
                                "웨이퍼 이송 실린더를 후진해주세요.");
            }
        }

        // 좌/우 원점 복귀
        private void button20_Click(object sender, EventArgs e)
        {
            // 규칙 3: 상/하 원점 완료 전에는 좌/우 원점복귀 불가
            if (!motion.UDHomeDone)
            {
                MessageBox.Show("먼저 상/하축 원점복귀를 완료하세요.");
                return;
            }
            MessageBox.Show("좌/우 원점복귀 시작");
            motion.HomeLR();
        }

        // ══════════════ 위치 이동 / 조그 ══════════════

        // 상/하 절대위치 이동
        private void button22_Click(object sender, EventArgs e)
        {
            long target = Convert.ToInt64(numericUpDown5.Value);
            motion.MoveUD(target);
        }

        // 좌/우 절대위치 이동
        private void button23_Click(object sender, EventArgs e)
        {
            long target = Convert.ToInt64(numericUpDown5.Value);
            motion.MoveLR(target);
        }

        // 좌/우 조그 (-)
        private void button24_Click(object sender, EventArgs e)
        {
            long jog = Convert.ToInt64(numericUpDown6.Value);
            motion.MoveLR(motion.LRPosition - jog);
        }

        // 좌/우 조그 (+)
        private void button25_Click(object sender, EventArgs e)
        {
            long jog = Convert.ToInt64(numericUpDown6.Value);
            motion.MoveLR(motion.LRPosition + jog);
        }

        // 상/하 조그 (+)
        private void button26_Click(object sender, EventArgs e)
        {
            long jog = Convert.ToInt64(numericUpDown6.Value);
            motion.MoveUD(motion.UDPosition + jog);
        }

        // 상/하 조그 (-)
        private void button27_Click(object sender, EventArgs e)
        {
            long jog = Convert.ToInt64(numericUpDown6.Value);
            motion.MoveUD(motion.UDPosition - jog);
        }

        // 서보 파라미터 설정
        private void button21_Click(object sender, EventArgs e)
        {
            long acc = Convert.ToInt64(numericUpDown1.Value);
            long dec = Convert.ToInt64(numericUpDown2.Value);
            long maxVel = Convert.ToInt64(numericUpDown3.Value);
            long vel = Convert.ToInt64(numericUpDown4.Value);

            EtherCAT_M.Axis1_UD_Config_Update(acc, dec, maxVel, vel);  // 상/하축
            EtherCAT_M.Axis2_LR_Config_Update(acc, dec, maxVel, vel);  // 좌/우축

            MessageBox.Show("파라미터 적용 완료");
        }

        // ══════════════ 100ms 주기 : UI 갱신 + 자동 시퀀스 ══════════════

        // [100ms 심장박동] 연결 후 0.1초마다 자동 호출된다.
        // 하는 일: ① 센서/축 값 읽어 화면 라벨 갱신 → ② 자동 시퀀스 1스텝 실행
        //          → ③ 상태에 따라 버튼 켜고 끄기. 이 함수가 화면과 자동운전을 계속 굴린다.
        private void timer1_Tick(object sender, EventArgs e)
        {
            // 점멸 플래그: 5틱(500ms)마다 토글 (알람 배너/State 배지/미믹 점멸용)
            // (0.5초마다 깜빡이게 하려고 참/거짓을 뒤집는 스위치)
            if (++blinkCounter >= 5) { blinkCounter = 0; blinkFlag = !blinkFlag; }

            // ── 입력 상태 (IOMap 사용) ──
            bool isP000_SW1 = io.Input(IOMap.Input.PW1);
            bool isP001_SW2 = io.Input(IOMap.Input.PW2);
            bool isP002_SelectSW = io.Input(IOMap.Input.SelectSW);
            bool isP003_EMG = io.Input(IOMap.Input.EMG);
            bool isP005_MainPress = io.Input(IOMap.Input.MainPressure);

            // Chamber_A 도어 상태
            label8.Text = io.Input(IOMap.Input.ChamberA_DoorUp) ? "True" : "False";
            label9.Text = io.Input(IOMap.Input.ChamberA_DoorDown) ? "True" : "False";

            // Chamber_B 도어 상태
            label10.Text = io.Input(IOMap.Input.ChamberB_DoorUp) ? "True" : "False";
            label11.Text = io.Input(IOMap.Input.ChamberB_DoorDown) ? "True" : "False";

            // Chamber_C 도어 상태
            label12.Text = io.Input(IOMap.Input.ChamberC_DoorUp) ? "True" : "False";
            label13.Text = io.Input(IOMap.Input.ChamberC_DoorDown) ? "True" : "False";

            // 이송 실린더 상태 (P013: 전진, P012: 후진)
            label14.Text = robot.IsForward() ? "True" : "False";
            label15.Text = robot.IsBackward() ? "True" : "False";

            // 스위치 / 압력
            label27.Text = isP000_SW1 ? "ON" : "OFF";
            label28.Text = isP001_SW2 ? "ON" : "OFF";
            label29.Text = isP002_SelectSW ? "ON" : "OFF";
            // 신호 OFF = 눌림 이므로 표시를 반전
            label30.Text = io.Input(IOMap.Input.EMG) ? "정상" : "비상정지!";
            label31.Text = isP005_MainPress ? "ON" : "OFF";

            // ── 상/하축 상태 ──
            label33.Text = motion.UDPosition.ToString();
            label35.Text = EtherCAT_M.Axis1_Status("PP_M") ? "ON" : "OFF";
            label37.Text = EtherCAT_M.Axis1_Status("PP_D") ? "완료" : "이동중";
            label39.Text = EtherCAT_M.Axis1_Status("HOME_M") ? "ON" : "OFF";
            label41.Text = EtherCAT_M.Axis1_Status("HOME_D") ? "완료" : "미완료";

            // ── 좌/우축 상태 ──
            label43.Text = motion.LRPosition.ToString();
            label45.Text = EtherCAT_M.Axis2_Status("PP_M") ? "ON" : "OFF";
            label47.Text = EtherCAT_M.Axis2_Status("PP_D") ? "완료" : "이동중";
            label49.Text = EtherCAT_M.Axis2_Status("HOME_M") ? "ON" : "OFF";
            label51.Text = EtherCAT_M.Axis2_Status("HOME_D") ? "완료" : "미완료";

            // ── 상단 우측 장비 상태 UI ──           
            lblEquipState.Text = equipment.CurrentState.ToString();
            lblAutoStep.Text = "Job : " + autoSequence.CurrentJobText;
            lblSlot.Text = "Step : " + autoSequence.Step.ToString();
            lblChamber.Text = $"A:{chamber.ChamberA.ProcState}  B:{chamber.ChamberB.ProcState}  C:{chamber.ChamberC.ProcState}";
            lblAlarm.Text = alarm.HasAlarm ? alarm.AlarmMessage : "-";
            lblAlarm.ForeColor = alarm.HasAlarm ? Color.Red : Color.Black;

            // 장비 상태에 따라 상태 라벨 색상
            switch (equipment.CurrentState)
            {
                case EquipmentState.Running:
                case EquipmentState.Auto:
                    lblEquipState.ForeColor = Color.Green;
                    break;
                case EquipmentState.Alarm:
                case EquipmentState.Emergency:
                    lblEquipState.ForeColor = Color.Red;
                    break;
                case EquipmentState.Complete:
                    lblEquipState.ForeColor = Color.Blue;
                    break;
                default:
                    lblEquipState.ForeColor = Color.Black;
                    break;
            }

            // ── 알람 배너 (수정 1) ──
            UpdateAlarmBanner();

            // ── 미믹 다이어그램 / 슬롯 인디케이터 갱신 (수정 3: 캐시 후 Invalidate) ──
            UpdateMimicCache();
            mimicPanel.Invalidate();
            UpdateSlotIndicators();

            // ── 자동 시퀀스 1스텝 실행 ──
            autoSequence.Run();

            // ── 상태 기반 버튼 활성/비활성 (수정 2, 마지막에 호출) ──
            UpdateControlEnable();
        }

        // ══════════════ 자동운전 시작 / 정지 / 알람 리셋 ══════════════

        // [자동공정 시작 버튼] 알람이 떠 있으면 못 시작하게 막고, 아니면 자동운전 시작.
        private void btnAutoStart_Click(object sender, EventArgs e)
        {
            if (alarm.HasAlarm)   // 알람이 남아 있으면
            {
                MessageBox.Show("알람이 해제되지 않았습니다.\n\n" +
                                "알람 리셋 후 다시 시작하세요.");
                return;           // 시작하지 않고 나감
            }
            autoSequence.Start();
        }

        // [자동공정 정지 버튼] 자동운전만 멈춘다(서보 힘은 유지).
        private void btnAutoStop_Click(object sender, EventArgs e)
        {
            autoSequence.Stop();
        }

        // [알람 리셋 버튼] 알람을 지우고 대기(Idle) 상태로 되돌린다.
        private void btnAlarmReset_Click(object sender, EventArgs e)
        {
            autoSequence.Reset();   // 알람 해제 + Idle 복귀
        }

        
    }
}

