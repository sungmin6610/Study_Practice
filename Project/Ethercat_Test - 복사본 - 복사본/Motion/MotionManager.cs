using System;
using IEG3268_Dll;

namespace EtherCAT_Test.Motion
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  서보 모터 2축을 제어. UD = 상/하축(Up-Down), LR = 좌/우축(Left-Right).
    //   - ServoON/OFF : 모터에 힘을 주기/풀기
    //   - Home       : 기준점(원점) 찾기. 좌표의 '0' 을 정하는 작업
    //   - Move       : 지정한 좌표로 이동
    //   - Position   : 지금 위치 읽기,  ...HomeDone / ...MoveDone : 다 됐는지 확인
    //  서보란? 명령한 위치로 정밀하게 움직이고 멈추는 똑똑한 모터.
    // ─────────────────────────────────────────────────────────────
    public class MotionManager
    {
        private readonly IEG3268 _ethercat;

        // 마지막으로 지령한 목표 위치를 기억 → 완료 판정에 사용
        // (이동 명령을 내린 목표값을 저장해 두고, 실제 위치가 거기 도달했는지 비교하기 위함)
        private long _lastUDTarget;
        private long _lastLRTarget;

        public MotionManager(IEG3268 ethercat)
        {
            _ethercat = ethercat;
        }

        // 서보 켜기: 두 축 모두 모터에 힘을 준다(이제 움직일 수 있음).
        public void ServoON()
        {
            _ethercat.Axis1_ON();
            _ethercat.Axis2_ON();
        }

        // 주의: 자동운전(서보 제어) 중에는 절대 호출하지 말 것 (규칙 1)
        public void ServoOFF()
        {
            _ethercat.Axis1_OFF();
            _ethercat.Axis2_OFF();
        }

        // 원점(기준점) 복귀. => 는 한 줄짜리 함수를 짧게 쓰는 문법.
        public void HomeUD() => _ethercat.Axis1_UD_Homming();   // 상/하축 원점
        public void HomeLR() => _ethercat.Axis2_LR_Homming();   // 좌/우축 원점

        // 상/하축을 pos 위치로 이동: ① 목표값 기억 → ② 목표 좌표 알려주고 → ③ 출발 명령
        public void MoveUD(long pos)
        {
            _lastUDTarget = pos;
            _ethercat.Axis1_UD_POS_Update(pos);
            _ethercat.Axis1_UD_Move_Send();
        }

        // 좌/우축을 pos 위치로 이동 (상/하축과 같은 방식)
        public void MoveLR(long pos)
        {
            _lastLRTarget = pos;
            _ethercat.Axis2_LR_POS_Update(pos);
            _ethercat.Axis2_LR_Move_Send();
        }

        // Axis*_is_PosData() 가 문자열을 반환하므로 TryParse 로 안전 변환
        public long UDPosition
        {
            get
            {
                long v;
                return long.TryParse(Convert.ToString(_ethercat.Axis1_is_PosData()), out v) ? v : 0;
            }
        }

        public long LRPosition
        {
            get
            {
                long v;
                return long.TryParse(Convert.ToString(_ethercat.Axis2_is_PosData()), out v) ? v : 0;
            }
        }

        public bool UDHomeDone => _ethercat.Axis1_Status("HOME_D");
        public bool LRHomeDone => _ethercat.Axis2_Status("HOME_D");

        // 현재 위치가 target 근처(허용오차 tolerance 이내)인지 확인.
        // Math.Abs = 절댓값(부호 무시한 크기). 즉 목표와의 거리 차가 100 이하이면 '도착'으로 봄.
        // (서보는 딱 떨어지는 값에 100% 멈추지 않으므로 약간의 오차를 허용한다)
        public bool IsUDPosition(long target, long tolerance = 100)
            => Math.Abs(UDPosition - target) <= tolerance;

        public bool IsLRPosition(long target, long tolerance = 100)
            => Math.Abs(LRPosition - target) <= tolerance;

        //   완료 판정 강화:
        //   PP_D 플래그는 직전 이동의 완료 상태가 남아 있는 첫 스캔에
        //   그대로 true 일 수 있으므로, 반드시 "목표 위치 도달"과 AND 로 판정.
        public bool UDMoveDone
            => _ethercat.Axis1_Status("PP_D") && IsUDPosition(_lastUDTarget);

        public bool LRMoveDone
            => _ethercat.Axis2_Status("PP_D") && IsLRPosition(_lastLRTarget);
    }
}


