using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherCAT_Test.Common
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  장비의 전기 신호(입력/출력)에 "몇 번 선인지" 번호를 붙여둔 번호표(주소록).
    //  IOManager 는 이 번호로 신호를 읽고(Input) 켜고 끈다(Output).
    //  코드에서 io.Input(3) 처럼 숫자를 직접 쓰면 무슨 신호인지 알 수 없으므로,
    //  IOMap.Input.EMG 처럼 이름으로 부르려고 번호에 이름을 붙였다.
    //  Input  = 장비 → 프로그램 (센서/스위치가 알려주는 값, 읽기 전용)
    //  Output = 프로그램 → 장비 (램프/실린더/진공을 켜고 끄는 명령)
    // ─────────────────────────────────────────────────────────────
    public static class IOMap
    {
        // 입력 신호 번호표 (프로그램이 "읽는" 신호들)
        public static class Input
        {
            public const int PW1 = 0;
            public const int PW2 = 1;
            public const int SelectSW = 2;
            public const int EMG = 3;
            public const int MainPressure = 5;

            public const int ChamberA_DoorUp = 6;
            public const int ChamberA_DoorDown = 7;

            public const int ChamberB_DoorUp = 8;
            public const int ChamberB_DoorDown = 9;

            public const int ChamberC_DoorUp = 10;
            public const int ChamberC_DoorDown = 11;

            public const int RobotBackward = 12;
            public const int RobotForward = 13;
            public const int VacuumSensor = 14;
        }

        // 출력 신호 번호표 (프로그램이 "켜고 끄는" 신호들)
        public static class Output
        {
            public const int TowerRed = 0;
            public const int TowerYellow = 1;
            public const int TowerGreen = 2;

            public const int ChamberALamp = 3;
            public const int ChamberA_DoorUp = 4;
            public const int ChamberA_DoorDown = 5;

            public const int ChamberBLamp = 6;
            public const int ChamberB_DoorUp = 7;
            public const int ChamberB_DoorDown = 8;

            public const int ChamberCLamp = 9;
            public const int ChamberC_DoorUp = 10;
            public const int ChamberC_DoorDown = 11;

            public const int RobotForward = 12;
            public const int RobotBackward = 13;

            public const int VacuumOn = 14;
            public const int VacuumBlow = 15;
        }
    }
}
