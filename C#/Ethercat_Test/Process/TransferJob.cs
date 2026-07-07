using EtherCAT_Test.Common;

namespace EtherCAT_Test.Process
{
    // 수정 2: 로봇 이송 1건 = From 스테이션에서 웨이퍼를 픽업해 To 스테이션에 안착.
    // Slot 은 FOUP A 에서 꺼낸 슬롯번호(1~5)로, FOUP B 안착까지 동일하게 유지된다.
    public class TransferJob
    {
        public Station From;
        public Station To;
        public int Slot;

        public TransferJob(Station from, Station to, int slot)
        {
            From = from;
            To = to;
            Slot = slot;
        }

        public override string ToString() => $"{From}→{To} Slot{Slot}";
    }
}
