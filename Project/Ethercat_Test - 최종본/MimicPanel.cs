using System.Windows.Forms;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  장비 모습을 그림으로 보여주는 '미믹(mimic) 그림판' 역할의 패널.
    //  ': Panel' = 윈도우의 기본 패널을 물려받아(상속) 그림 최적화만 켠 것.
    //  더블버퍼링 = 그림을 화면에 바로 그리지 않고 뒤에서 완성한 뒤 한 번에 보여주는 기법.
    //  이렇게 하면 화면이 깜빡이지 않는다. 실제로 무엇을 그리는지는 Form1.Mimic.cs 에 있다.
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// 장비 미믹(mimic) 다이어그램용 더블버퍼 패널.
    /// 실제 드로잉은 Form1.UI.cs 의 mimicPanel_Paint 에서 수행한다(매니저 접근 목적).
    /// </summary>
    public class MimicPanel : Panel
    {
        public MimicPanel()
        {
            this.DoubleBuffered = true;    // 깜빡임 방지(뒤에서 그려 한 번에 표시)
            this.ResizeRedraw = true;      // 크기 바뀌면 다시 그리기
            SetStyle(ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint, true);
        }
    }
}
