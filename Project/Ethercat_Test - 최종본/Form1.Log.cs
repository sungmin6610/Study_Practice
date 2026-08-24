using System;
using EtherCAT_Test.Equipment;
using EtherCAT_Test.Logging;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  로깅/이벤트 훅 배선 (partial class Form1).
    //  AutoSequence 의 이벤트(Action)를 구독하여 Form 측에서 로깅/ Lot 을 처리.
    //  Phase 3 에서 Handle* 본문에 Lot 생성/종료 로직을 추가한다.
    //
    //  [초보자용 설명 — '이벤트 구독(훅)'이란?]
    //  AutoSequence 는 "자동공정 시작/완료/알람정지" 같은 일이 생기면
    //  OnAutoStart 같은 신호(이벤트)를 '방송'한다. 여기서 += 로 그 방송을
    //  '구독(신청)'해 두면, 그 일이 생길 때 Handle... 함수가 자동으로 불린다.
    //  덕분에 AutoSequence 는 로그/화면을 몰라도 되고(자기 일만), 기록은 Form 이 맡는다.
    //  (신문 구독과 같다: 신문사는 발행만, 구독자는 오면 읽는다.)
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private void InitLoggingHooks()
        {
            LogManager.Instance.StepProvider = () => autoSequence.Step.ToString();
            autoSequence.OnAutoStart += HandleAutoStart;
            autoSequence.OnAutoComplete += HandleAutoComplete;
            autoSequence.OnAutoAbort += HandleAutoAbort;

            // E10 집계: 상태 전이 훅 → E10StateTracker 누적
            equipment.OnStateChanged += (prev, next) =>
                E10StateTracker.Instance.OnStateChange(prev, next);
        }

        private void HandleAutoStart()
        {
            LogManager.Instance.LogEvent("INFO", "자동공정 시작");
            StartLot();          // Phase 3: Lot 생성 + CEID 1001
        }

        private void HandleAutoComplete()
        {
            LogManager.Instance.LogEvent("INFO", "자동공정 완료");
            EndLot(false);       // Phase 3: Lot 완료 + CEID 1002
        }

        private void HandleAutoAbort(string message)
        {
            LogManager.Instance.LogEvent("STATE", "자동공정 알람정지: " + message);
            EndLot(true);        // Phase 3: Lot 중단 + CEID 1002
        }
    }
}
