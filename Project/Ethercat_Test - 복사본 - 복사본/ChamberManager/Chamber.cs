using System;
using EtherCAT_Test.Common;
using EtherCAT_Test.IO;
using EtherCAT_Test.Process;

namespace EtherCAT_Test.Chamber
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  챔버(공정 방) 한 칸을 표현. 문(도어) 열고닫기, 램프 켜고끄기, 문 상태 확인을 담당.
    //  핵심 아이디어: A·B·C 챔버는 동작이 똑같고 '연결된 IO 번호'만 다르다.
    //  그래서 코드를 3벌 만들지 않고, 이 클래스 하나에 IO 번호를 '생성 시 주입'받아
    //  A/B/C 를 같은 코드로 처리한다. (주입 = 만들 때 필요한 값을 밖에서 넣어줌)
    //  솔레노이드(SOL) = 전기로 밀고 당기는 공압 밸브. '복동'은 여는 쪽/닫는 쪽이 따로 있음.
    // ─────────────────────────────────────────────────────────────

    // 챔버 1개의 도어(복동 솔레노이드) / 램프 / 상태를 담당.
    // IO 주소는 생성 시 주입받아 A/B/C 를 동일 코드로 처리한다.

    public class Chamber
    {
        private readonly IOManager io;
        private readonly RecipeManager recipe;   // 공정시간을 레시피에서 읽기 위함
        private readonly int defaultTimeMs;      // 레시피에 스텝이 없을 때 fallback

        public ChamberType Type { get; }
        public ProcessType Process { get; }
        public long PositionX { get; }          // 좌/우 서보 위치

        //  공정 시간: 하드코딩/생성자 주입값 대신 RecipeManager.CurrentRecipe 에서 읽는다.
        //  (AutoSequence 의 Cham_ProcessWait 호출부는 그대로 Cham.ProcessTimeMs 사용 → 무변경)
        public int ProcessTimeMs => recipe != null
            ? recipe.GetProcessTimeMs(Type, defaultTimeMs)
            : defaultTimeMs;

        // IO 매핑
        private readonly int outDoorUp;
        private readonly int outDoorDown;
        private readonly int outLamp;
        private readonly int inDoorUp;
        private readonly int inDoorDown;

        public bool HasWafer { get; set; }
        public bool Busy { get; set; }

        public Chamber(IOManager ioManager,
                       ChamberType type, ProcessType process,
                       long positionX, int processTimeMs,
                       int outDoorUp, int outDoorDown, int outLamp,
                       int inDoorUp, int inDoorDown,
                       RecipeManager recipeManager)
        {
            io = ioManager;
            Type = type;
            Process = process;
            PositionX = positionX;
            defaultTimeMs = processTimeMs;
            recipe = recipeManager;
            this.outDoorUp = outDoorUp;
            this.outDoorDown = outDoorDown;
            this.outLamp = outLamp;
            this.inDoorUp = inDoorUp;
            this.inDoorDown = inDoorDown;
        }

        //   이 장비는 Door Down = '열림', Door Up = '닫힘' 이다.
        //   따라서 도어 열기 = 하강(Down) SOL 구동, 도어 닫기 = 상승(Up) SOL 구동.
        //   복동 SOL 이므로 반대편은 반드시 OFF.
        public void OpenDoor()   // 열림 = 하강 구동
        {
            io.Output(outDoorDown, true);
            io.Output(outDoorUp, false);
        }

        public void CloseDoor()  // 닫힘 = 상승 구동
        {
            io.Output(outDoorUp, true);
            io.Output(outDoorDown, false);
        }

        //   열림/닫힘은 반드시 양쪽 센서로 판정 (한쪽만 보면 오독 시 블레이드가
        //   문 닫힌 상태에서 전진해 버린다).
        //   열림 = 하강 센서 ON && 상승 센서 OFF
        //   닫힘 = 상승 센서 ON && 하강 센서 OFF
        public bool IsDoorOpen => io.Input(inDoorDown) && !io.Input(inDoorUp);
        public bool IsDoorClosed => io.Input(inDoorUp) && !io.Input(inDoorDown);

        public void LampOn() => io.Output(outLamp, true);
        public void LampOff() => io.Output(outLamp, false);
    }
}
