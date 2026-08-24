using System;
using System.Collections.Generic;
using EtherCAT_Test.Common;
using EtherCAT_Test.IO;
using EtherCAT_Test.Process;

namespace EtherCAT_Test.Chamber
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  챔버 A/B/C 3개를 한꺼번에 만들어 들고 있는 관리자.
    //  각 챔버에 "어떤 공정인지 / 좌우 위치 / 어떤 IO 번호를 쓸지"를 정해 생성한다.
    //  Chambers[0]=A, [1]=B, [2]=C 처럼 번호(인덱스)로도 접근할 수 있어,
    //  자동 시퀀스가 챔버 3개를 반복문으로 똑같이 처리하기 편하다.
    // ─────────────────────────────────────────────────────────────
    public class ChamberManager
    {
        // 이름으로 접근 (챔버 A / B / C)
        public Chamber ChamberA { get; }
        public Chamber ChamberB { get; }
        public Chamber ChamberC { get; }

        /// <summary>인덱스(0=A, 1=B, 2=C)로 접근 — 시퀀스에서 반복 처리용</summary>
        public IReadOnlyList<Chamber> Chambers { get; }

        public ChamberManager(IOManager io, RecipeManager recipe)
        {
            // 마지막 인자(8000/12000/8000)는 레시피에 해당 스텝이 없을 때의 fallback 기본값.
            // 실제 공정시간은 RecipeManager.CurrentRecipe 에서 읽는다(기본 SCA: 3000/5000/3000).
            ChamberA = new Chamber(io, ChamberType.A, ProcessType.PRCoating,
                Position.CHAMBER_A_X, 8000,
                IOMap.Output.ChamberA_DoorUp, IOMap.Output.ChamberA_DoorDown, IOMap.Output.ChamberALamp,
                IOMap.Input.ChamberA_DoorUp, IOMap.Input.ChamberA_DoorDown, recipe);

            ChamberB = new Chamber(io, ChamberType.B, ProcessType.Exposure,
                Position.CHAMBER_B_X, 12000,
                IOMap.Output.ChamberB_DoorUp, IOMap.Output.ChamberB_DoorDown, IOMap.Output.ChamberBLamp,
                IOMap.Input.ChamberB_DoorUp, IOMap.Input.ChamberB_DoorDown, recipe);

            ChamberC = new Chamber(io, ChamberType.C, ProcessType.Develop,
                Position.CHAMBER_C_X, 8000,
                IOMap.Output.ChamberC_DoorUp, IOMap.Output.ChamberC_DoorDown, IOMap.Output.ChamberCLamp,
                IOMap.Input.ChamberC_DoorUp, IOMap.Input.ChamberC_DoorDown, recipe);

            Chambers = new List<Chamber> { ChamberA, ChamberB, ChamberC };
        }
    }
}
