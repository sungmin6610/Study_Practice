using System.Windows.Forms;
using EtherCAT_Test.Auth;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  수정 2: 상태 기반 버튼 활성/비활성 (partial class Form1).
    //  timer1_Tick 마지막 + 연결/해제 시점에 UpdateControlEnable() 호출.
    //  값이 바뀔 때만 Enabled 대입(깜빡임/성능). Manual 계열은 GroupBox 단위.
    //
    //  [초보자용 설명]
    //  Enabled = false 이면 버튼이 회색으로 눌리지 않는다.
    //  이 파일은 "지금 상황에 눌러도 되는 버튼만 켜두는" 안전장치다.
    //  판단 기준 두 가지를 AND(둘 다 만족)로 본다:
    //   ① 상황(연결됨? 자동운전 중? 알람 중?)  ② 권한(이 계정이 Engineer 이상인가?)
    //  예) 미연결이면 연결 버튼 빼고 전부 끔 / 자동운전 중이면 수동조작 다 끔.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private bool isConnected;   // button1/button2 에서 세팅

        private void SetEnabled(Control c, bool want)
        {
            if (c != null && c.Enabled != want) c.Enabled = want;
        }

        private void UpdateControlEnable()
        {
            bool connected = isConnected;
            bool autoRunning = autoSequence.AutoRun;
            bool hasAlarm = alarm.HasAlarm;

            // ── 권한 조건 (상태 조건과 AND) ──
            var u = UserManager.Instance.CurrentUser;
            bool roleEng = u != null && u.HasPermission(UserRole.Engineer);        // 수동/레시피/파라미터
            bool roleAdm = u != null && u.HasPermission(UserRole.Administrator);   // 계정관리

            // 관리자 탭은 연결/운전 상태와 무관, 권한으로만
            SetAdminEnabled(roleAdm);
            UpdatePermTooltips(roleEng);

            // Connection 버튼
            SetEnabled(button1, !connected);   // ON : 미연결일 때만
            SetEnabled(button2, connected);    // OFF: 연결 시

            // 1) 미연결 → Connection ON 외 전부 비활성
            if (!connected)
            {
                SetManualEnabled(false);
                SetRecipeEnabled(false);
                SetEnabled(btnAutoStart, false);
                SetEnabled(btnAutoStop, false);
                SetEnabled(btnAlarmReset, false);
                SetEnabled(btnBannerReset, false);
                return;
            }

            // 2) 자동운전 중 → Manual/Recipe 전부 비활성, 정지/OFF/알람리셋만 활성
            if (autoRunning)
            {
                SetManualEnabled(false);
                SetRecipeEnabled(false);
                SetEnabled(btnAutoStart, false);
                SetEnabled(btnAutoStop, true);
                SetEnabled(btnAlarmReset, true);
                SetEnabled(btnBannerReset, true);
                return;
            }

            // 3) 연결 + 비운전 : 상태로는 활성이나 권한(Engineer+)으로 게이팅
            SetManualEnabled(roleEng);
            SetRecipeEnabled(roleEng);
            SetEnabled(btnAutoStop, false);          // 비운전 시 정지 비활성
            SetEnabled(btnAlarmReset, true);
            SetEnabled(btnBannerReset, true);
            SetEnabled(btnAutoStart, !hasAlarm);     // 자동시작: 모든 역할 허용(알람 시 제외)
        }

        // 권한 부족 툴팁 (Engineer 미만이면 Manual 그룹에 "권한 부족" 표시)
        private void UpdatePermTooltips(bool roleEng)
        {
            if (permTip == null) return;
            string t = roleEng ? "" : "권한 부족 (Engineer 이상 필요)";
            SetPermTip(groupBox1, t); SetPermTip(groupBox2, t); SetPermTip(groupBox3, t);
            SetPermTip(groupBox4, t); SetPermTip(groupBox5, t); SetPermTip(groupBox6, t);
            SetPermTip(groupBox7, t); SetPermTip(groupBox8, t); SetPermTip(groupBox9, t);
            SetPermTip(groupBox10, t);
        }

        private void SetPermTip(Control c, string t)
        {
            if (c != null) permTip.SetToolTip(c, t);
        }

        // Manual 계열 GroupBox 단위 제어 (chamber/tower/robot/servo/param/pos/jog)
        private void SetManualEnabled(bool en)
        {
            SetEnabled(groupBox1, en);   // PMC-A 수동
            SetEnabled(groupBox2, en);   // PMC-B 수동
            SetEnabled(groupBox3, en);   // PMC-C 수동
            SetEnabled(groupBox4, en);   // Lamp Tower 테스트
            SetEnabled(groupBox5, en);   // 로봇 실린더
            SetEnabled(groupBox6, en);   // 진공/파기
            SetEnabled(groupBox7, en);   // 서보 ON/OFF·원점복귀
            SetEnabled(groupBox8, en);   // 파라미터
            SetEnabled(groupBox9, en);   // 절대 위치 이동
            SetEnabled(groupBox10, en);  // 조그
        }

        private void SetRecipeEnabled(bool en)
        {
            SetEnabled(lstRecipes, en);
            SetEnabled(nudRcpA, en);
            SetEnabled(nudRcpB, en);
            SetEnabled(nudRcpC, en);
            SetEnabled(btnRecipeApply, en);
            SetEnabled(btnRecipeSave, en);
            SetEnabled(btnRecipeNew, en);
        }
    }
}
