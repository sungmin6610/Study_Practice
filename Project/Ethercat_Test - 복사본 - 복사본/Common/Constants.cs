using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherCAT_Test.Common
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  프로그램 곳곳에서 쓰는 "고정된 숫자값(상수)"을 한 곳에 모아둔 창고.
    //  static class = 객체(new)를 만들지 않고 Constants.TimerInterval 처럼 바로 꺼내 씀.
    //  const  = 프로그램이 도는 동안 절대 바뀌지 않는 값. (숫자를 코드에 흩뿌리지 않고
    //           이름을 붙여두면, 나중에 값 하나만 여기서 고치면 전체에 반영된다.)
    //  단위는 대부분 ms(밀리초) = 1000분의 1초.
    // ─────────────────────────────────────────────────────────────
    public static class Constants
    {
        // 자동운전/화면 갱신 타이머 주기. 100ms 마다 한 번씩 동작 = 1초에 10번.
        public const int TimerInterval = 100;

        // 챔버 기본 공정 시간(3초). (실제 값은 레시피에서 읽어오는 경우가 많음)
        public const int ChamberProcessTime = 3000;

        // 로봇(이송 실린더)이 움직이는 데 걸리는 대략 시간(1초).
        public const int RobotMoveTime = 1000;

        // 챔버 도어가 열리고 닫히는 데 걸리는 대략 시간(0.5초).
        public const int DoorMoveTime = 500;

        // 배기 대기 시간 (ms) — 웨이퍼를 내려놓을 때 바람을 부는 시간(1초).
        public const int BlowTimeMs = 1000;
    }
}
