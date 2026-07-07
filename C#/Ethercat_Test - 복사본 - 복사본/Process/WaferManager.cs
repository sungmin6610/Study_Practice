using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherCAT_Test.Process
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  웨이퍼 5장(Wafer)을 리스트로 들고 관리하는 "관리자".
    //  Wafer 는 낱장 하나, WaferManager 는 그 낱장들을 모아둔 통이라고 보면 된다.
    // ─────────────────────────────────────────────────────────────
    public class WaferManager
    {
        // 웨이퍼 5장이 담기는 목록(List). get 만 있어 밖에서 통 자체를 바꿀 수는 없다.
        public List<Wafer> Wafers { get; }

        // 생성자: 만들 때 슬롯 1~5번 웨이퍼를 미리 5장 채워 넣는다.
        public WaferManager()
        {
            Wafers = new List<Wafer>();

            for (int i = 1; i <= 5; i++)   // 1,2,3,4,5 반복
            {
                Wafers.Add(new Wafer(i));  // i번 슬롯 웨이퍼를 만들어 목록에 추가
            }
        }

        // 슬롯 번호로 해당 웨이퍼 한 장을 찾아 돌려준다. (없으면 null)
        // Find(w => w.Slot == slot) = "Slot 이 slot 인 첫 번째 원소를 찾아라"
        public Wafer GetWafer(int slot)
        {
            return Wafers.Find(w => w.Slot == slot);
        }
    }
}
