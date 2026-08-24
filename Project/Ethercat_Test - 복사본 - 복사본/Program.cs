using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  프로그램이 켜지면 '가장 먼저' 실행되는 시작점(Main)이 여기 있다.
    //  순서: ① 화면 기본 설정 → ② 로그인 창을 먼저 띄운다
    //        → ③ 로그인 성공(OK)해야 메인 화면(Form1)을 연다. 취소하면 그냥 종료.
    // ─────────────────────────────────────────────────────────────
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]   // Windows 화면(UI) 프로그램에 필요한 표준 설정(한 스레드에서 UI 처리)
        static void Main()
        {
            Application.EnableVisualStyles();                       // 요즘 윈도우 버튼/모양 스타일 켜기
            Application.SetCompatibleTextRenderingDefault(false);   // 글자 렌더링 방식 표준 설정

            // 로그인 성공해야 Form1 진입. 취소 시 종료.
            // using(...) : 창을 다 쓰고 나면 자동으로 정리(메모리 반납)해 주는 문법.
            // ShowDialog() : 이 창을 닫을 때까지 다음 줄로 넘어가지 않고 기다림(모달 창).
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK) return;  // OK 아니면(취소) 프로그램 끝
            }

            Application.Run(new Form1());   // 메인 화면을 띄우고 프로그램을 계속 돌린다
        }
    }
}
