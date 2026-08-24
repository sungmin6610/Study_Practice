using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherCAT_Test.Common;

namespace EtherCAT_Test.Process
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  웨이퍼(반도체 원판) 한 장의 정보를 담는 "데이터 그릇".
    //  이 장이 몇 번 슬롯인지, 지금 어디에 있는지, 어떤 공정 중인지 등을 기억한다.
    //  { get; set; } 는 값을 읽고(get) 쓸(set) 수 있는 '속성(프로퍼티)' 문법이다.
    // ─────────────────────────────────────────────────────────────
    public class Wafer
    {
        // 슬롯 번호
        public int Slot { get; set; }

        // 현재 웨이퍼 위치
        public WaferLocation Location { get; set; }

        // 현재 공정 상태
        public WaferState State { get; set; }

        // 현재 진행 중인 공정
        public ProcessType CurrentProcess { get; set; }

        // 공정 완료 여부
        public bool ProcessCompleted { get; set; }

        // 생성자: new Wafer(3) 처럼 웨이퍼를 만들 때 처음 상태를 정해준다.
        // (생성자 = 객체가 태어나는 순간 딱 한 번 실행되는 초기화 함수)
        public Wafer(int slot)
        {
            Slot = slot;

            // 처음에는 FOUP_A(출발 선반)에 있음
            Location = WaferLocation.FOUPA;

            // 아직 공정 전 (대기)
            State = WaferState.Ready;

            CurrentProcess = ProcessType.PRCoating;

            ProcessCompleted = false;
        }
    }
}
