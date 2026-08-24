using System;

namespace EtherCAT_Test.Process
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  Lot(로트) = 웨이퍼 5장을 한 묶음으로 처리하는 "자동공정 1회분"을 뜻함.
    //  이 묶음의 번호(LotId), 시작/종료 시각, 상태를 기록한다.
    //  현장에서 "이번 로트 다 돌았어?" 할 때 그 로트가 바로 이것.
    // ─────────────────────────────────────────────────────────────

    // 로트의 진행 상태 (대기 / 진행중 / 완료 / 중단)
    public enum LotState { Ready, Running, Completed, Aborted }

    /// <summary>
    /// 1회 자동공정(웨이퍼 5매)을 나타내는 Lot.
    /// LotId 는 "LOT" + yyyyMMddHHmmss 로 자동 생성.
    /// </summary>
    public class Lot
    {
        public string LotId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public int WaferCount { get; set; }
        public LotState State { get; private set; }

        // 생성자: 로트를 시작할 때 번호와 시작시각을 자동으로 붙인다.
        // 예) 2026-07-07 13:05:22 에 시작하면 LotId = "LOT20260707130522"
        public Lot(int waferCount)
        {
            LotId = "LOT" + DateTime.Now.ToString("yyyyMMddHHmmss");
            StartTime = DateTime.Now;
            WaferCount = waferCount;
            State = LotState.Running;   // 만들자마자 '진행중'
        }

        // 정상 완료 처리: 상태를 완료로 바꾸고 끝난 시각 기록
        public void Complete()
        {
            State = LotState.Completed;
            EndTime = DateTime.Now;
        }

        // 중단 처리(알람 등으로 멈춤): 상태를 중단으로 바꾸고 끝난 시각 기록
        public void Abort()
        {
            State = LotState.Aborted;
            EndTime = DateTime.Now;
        }
    }
}
