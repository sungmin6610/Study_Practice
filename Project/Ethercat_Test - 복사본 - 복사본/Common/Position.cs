using System;

namespace EtherCAT_Test.Common
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  로봇 서보축이 이동해야 할 "목표 위치 좌표값"을 모아둔 곳.
    //  이런 값을 현장에서 실제로 맞춰 넣는 작업을 '티칭(teaching)' 이라고 부른다.
    //  숫자(예: 302380)는 서보 모터의 엔코더 눈금(펄스) 값이라고 생각하면 된다.
    //
    //  구조가 두 덩어리로 나뉘는 이유:
    //   - PositionConfig : 파일(positions.json)에 저장/복원하기 위한 "데이터 그릇".
    //   - Position       : 프로그램이 실제로 꺼내 쓰는 "읽기 전용 상수 모음".
    //                       프로그램 시작 시 파일을 읽어 채우고, 파일이 없으면
    //                       기본값(Default)으로 채운 뒤 파일을 새로 만들어 둔다.
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// 티칭 위치값 외부화 컨테이너 (Config/positions.json 직렬화 대상).
    /// FOUP B 상/하값은 FOUP A 와 동일하므로 별도 저장하지 않는다.
    /// </summary>
    public class PositionConfig
    {
        // 인덱스 0 은 미사용([slot] 직접 접근용)
        public long[] FOUPA_UP { get; set; }
        public long[] FOUPA_DOWN { get; set; }

        public long FOUPA_X { get; set; }
        public long FOUPB_X { get; set; }

        public long CHAMBER_A_X { get; set; }
        public long CHAMBER_B_X { get; set; }
        public long CHAMBER_C_X { get; set; }

        public long CHAMBER_UP { get; set; }
        public long CHAMBER_DOWN { get; set; }

        /// <summary>현재 티칭값(파일이 없을 때 자동 생성용 기본값).</summary>
        public static PositionConfig Default()
        {
            return new PositionConfig
            {
                FOUPA_UP = new long[] { 0, 302380, 982378, 1627604, 2332102, 3018457 },
                FOUPA_DOWN = new long[] { 0, 30000, 700000, 1350000, 2050000, 2750000 },
                FOUPA_X = 14140,
                FOUPB_X = -394293,
                CHAMBER_A_X = -59064,
                CHAMBER_B_X = -190823,
                CHAMBER_C_X = -322000,
                CHAMBER_UP = 1156931,
                CHAMBER_DOWN = 806931,
            };
        }
    }

    /// <summary>
    /// 티칭 위치 상수 (호출부 접근방식 불변: Position.FOUPA_UP[slot], Position.FOUPA_X 등).
    /// 내부 값은 Config/positions.json 에서 로드하며, 파일이 없으면 기본값으로 생성한다.
    /// </summary>
    public static class Position
    {
        // ---------- FOUP A (상/하 축, 슬롯 1~5) ----------
        public static readonly long[] FOUPA_UP;
        public static readonly long[] FOUPA_DOWN;   // 안착 위치

        // FOUP B 는 상/하 값이 FOUP A 와 동일 (좌/우 위치만 다름)
        public static readonly long[] FOUPB_UP;
        public static readonly long[] FOUPB_DOWN;

        // ---------- 좌/우 축 ----------
        public static readonly long FOUPA_X;
        public static readonly long FOUPB_X;

        public static readonly long CHAMBER_A_X;
        public static readonly long CHAMBER_B_X;
        public static readonly long CHAMBER_C_X;

        // 챔버 인덱스(0=A, 1=B, 2=C)로 접근할 수 있는 배열
        public static readonly long[] CHAMBER_X;

        // ---------- 챔버 상/하 (3개 챔버 공통) ----------
        public static readonly long CHAMBER_UP;     // 이동(상승) 위치
        public static readonly long CHAMBER_DOWN;   // 안착 위치

        static Position()
        {
            var path = JsonUtil.Combine("Config", "positions.json");

            PositionConfig cfg;
            if (!JsonUtil.TryLoad<PositionConfig>(path, out cfg)
                || cfg == null || cfg.FOUPA_UP == null || cfg.FOUPA_DOWN == null)
            {
                cfg = PositionConfig.Default();
                JsonUtil.TrySave(path, cfg);   // 없으면 현재값으로 자동 생성
            }

            FOUPA_UP = cfg.FOUPA_UP;
            FOUPA_DOWN = cfg.FOUPA_DOWN;
            FOUPB_UP = FOUPA_UP;
            FOUPB_DOWN = FOUPA_DOWN;

            FOUPA_X = cfg.FOUPA_X;
            FOUPB_X = cfg.FOUPB_X;

            CHAMBER_A_X = cfg.CHAMBER_A_X;
            CHAMBER_B_X = cfg.CHAMBER_B_X;
            CHAMBER_C_X = cfg.CHAMBER_C_X;
            CHAMBER_X = new long[] { CHAMBER_A_X, CHAMBER_B_X, CHAMBER_C_X };

            CHAMBER_UP = cfg.CHAMBER_UP;
            CHAMBER_DOWN = cfg.CHAMBER_DOWN;
        }
    }
}
