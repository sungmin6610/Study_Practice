using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using EtherCAT_Test.Common;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  수정 3: 미믹 패널 2축 + 도어/웨이퍼 (partial class Form1).
    //  값 읽기는 timer1_Tick(UpdateMimicCache)에서만, OnPaint 는 캐시만 사용.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        // ── 캐시 (timer1_Tick 에서 갱신) ──
        private long mLR, mUD;
        private readonly bool[] mDoorOpen = new bool[3];
        private readonly bool[] mDoorClosed = new bool[3];
        private readonly bool[] mBusy = new bool[3];
        private readonly bool[] mHasWafer = new bool[3];
        private readonly int[] mProcTime = new int[3];
        private int mElapsedMs;
        private bool mFwd, mVac;
        private readonly bool[] mSlotA = new bool[6];  // [1..5] FOUP A 에 웨이퍼 유무
        private readonly bool[] mSlotB = new bool[6];  // [1..5] FOUP B 에 웨이퍼 유무

        private static readonly Color CNone = Color.Silver;
        private static readonly Color CWait = Color.RoyalBlue;
        private static readonly Color CDone = Color.FromArgb(39, 174, 96);
        private static readonly Color CBusy = Color.Orange;
        private static readonly Color CBg = Color.FromArgb(236, 240, 244);

        // timer1_Tick 에서 호출 — 통신 값 읽어 캐시에 저장 (OnPaint 는 통신 호출 없음)
        private void UpdateMimicCache()
        {
            try
            {
                mLR = motion.LRPosition;
                mUD = motion.UDPosition;

                var chs = new[] { chamber.ChamberA, chamber.ChamberB, chamber.ChamberC };
                for (int i = 0; i < 3; i++)
                {
                    mDoorOpen[i] = chs[i].IsDoorOpen;
                    mDoorClosed[i] = chs[i].IsDoorClosed;
                    mBusy[i] = chs[i].Busy;
                    mHasWafer[i] = chs[i].HasWafer;
                    mProcTime[i] = chs[i].ProcessTimeMs;
                }
                mElapsedMs = autoSequence.CurrentProcessElapsedMs;
                mFwd = robot.IsForward();
                mVac = robot.IsVacuum();

                for (int s = 1; s <= 5; s++)
                {
                    var wf = waferManager.GetWafer(s);
                    mSlotA[s] = (wf != null && wf.Location == WaferLocation.FOUPA);
                    mSlotB[s] = (wf != null && wf.Location == WaferLocation.FOUPB);
                }
            }
            catch { /* 연결 전/일시 오류 무시 */ }
        }

        // OnPaint — 캐시만 사용
        private void mimicPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(CBg);

            int w = mimicPanel.ClientSize.Width;
            int h = mimicPanel.ClientSize.Height;
            if (w < 120 || h < 120) return;

            var center = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            using (var titleF = new Font("맑은 고딕", 9f, FontStyle.Bold))
                TextRenderer.DrawText(g, "Transfer Chamber & Load Ports", titleF,
                    new Rectangle(0, 4, w, 18), Color.DimGray, TextFormatFlags.HorizontalCenter);

            // ── 5개 스테이션 (왼쪽부터 FOUP_A, PMC-A, PMC-B, PMC-C, FOUP_B) ──
            int marginX = 18, gap = 14, cols = 5;
            int colW = (w - marginX * 2 - gap * (cols - 1)) / cols;
            int stTop = 42;
            int stH = Math.Max(130, (int)(h * 0.44));
            int[] colX = new int[cols];
            for (int i = 0; i < cols; i++) colX[i] = marginX + i * (colW + gap);

            DrawFoup(g, colX[0], stTop, colW, stH, "FOUP A", mSlotA, CWait, center);
            DrawFoup(g, colX[4], stTop, colW, stH, "FOUP B", mSlotB, CDone, center);

            string[] pmc = { "PMC-A  PR", "PMC-B  EXP", "PMC-C  DEV" };
            for (int i = 0; i < 3; i++)
                DrawChamber(g, colX[1 + i], stTop, colW, stH, pmc[i], i, center);

            // ── 이송 로봇 : 가로 LR + 세로 UD 2축 ──
            int railY = stTop + stH + 44;
            if (railY > h - 40) railY = h - 40;
            using (var rail = new Pen(Color.DimGray, 3f))
                g.DrawLine(rail, marginX, railY, w - marginX, railY);

            // 가로 위치 (FOUP_A=좌, FOUP_B=우)
            double lo = Position.FOUPB_X, hi = Position.FOUPA_X;
            double frac = (mLR - lo) / (hi - lo);
            if (frac < 0) frac = 0; if (frac > 1) frac = 1;
            int rx = marginX + (int)((1 - frac) * (w - marginX * 2));

            // 세로 UD 오프셋 (0~3.1M → 0~30px, 위로)
            double uf = mUD / 3100000.0;
            if (uf < 0) uf = 0; if (uf > 1) uf = 1;
            int headY = railY - (int)(uf * 30);

            // 세로 UD 레일 + 헤드
            using (var udrail = new Pen(Color.Gray, 2f))
                g.DrawLine(udrail, rx, railY + 4, rx, railY - 36);
            var head = new Rectangle(rx - 24, headY - 14, 48, 28);
            g.FillRectangle(Brushes.SteelBlue, head);
            g.DrawRectangle(Pens.Black, head);

            // 블레이드(전진 시 가로 막대 연장) + 진공 웨이퍼
            int bladeEndX = rx + 24;
            if (mFwd)
            {
                bladeEndX = rx + 60;
                g.FillRectangle(Brushes.DimGray, new Rectangle(rx + 24, headY - 5, 40, 10));
            }
            if (mVac)
                g.FillEllipse(Brushes.RoyalBlue, bladeEndX - 8, headY - 8, 16, 16);

            // ── 색 범례 ──
            DrawLegend(g, marginX, h - 24);
        }

        private void DrawFoup(Graphics g, int x, int y, int wdt, int hgt,
                              string name, bool[] slotState, Color onColor, TextFormatFlags center)
        {
            g.FillRectangle(Brushes.White, x, y, wdt, hgt);
            g.DrawRectangle(Pens.SteelBlue, x, y, wdt, hgt);
            TextRenderer.DrawText(g, name, this.Font, new Rectangle(x, y + 2, wdt, 18),
                Color.Black, TextFormatFlags.HorizontalCenter);

            int top = y + 22;
            int areaH = hgt - 28;
            int slotH = areaH / 5;
            for (int i = 0; i < 5; i++)
            {
                int slot = 5 - i;   // 위=5, 아래=1
                var r = new Rectangle(x + 6, top + i * slotH + 2, wdt - 12, slotH - 4);
                using (var b = new SolidBrush(slotState[slot] ? onColor : CNone))
                    g.FillRectangle(b, r);
                g.DrawRectangle(Pens.Gray, r);
                TextRenderer.DrawText(g, slot.ToString(), this.Font, r, Color.White, center);
            }
        }

        private void DrawChamber(Graphics g, int x, int y, int wdt, int hgt,
                                 string label, int idx, TextFormatFlags center)
        {
            var body = new Rectangle(x, y, wdt, hgt);

            // 공정 중이면 내부 주황 점멸
            Color fill = mBusy[idx] ? (blinkFlag ? CBusy : Color.NavajoWhite) : Color.White;
            using (var b = new SolidBrush(fill)) g.FillRectangle(b, body);
            g.DrawRectangle(Pens.DimGray, body);

            TextRenderer.DrawText(g, label, this.Font, new Rectangle(x, y + 2, wdt, 18),
                Color.Black, TextFormatFlags.HorizontalCenter);

            // 도어: 앞면 가로 막대 (열림=위 / 닫힘=아래 / 이동중=중간)
            int doorYUp = y + 24, doorYDn = y + 44;
            int doorY = mDoorOpen[idx] ? doorYUp : (mDoorClosed[idx] ? doorYDn : (doorYUp + doorYDn) / 2);
            Color doorC = mDoorOpen[idx] ? Color.LimeGreen : (mDoorClosed[idx] ? Color.Gray : Color.Orange);
            using (var db = new SolidBrush(doorC))
                g.FillRectangle(db, new Rectangle(x + 8, doorY, wdt - 16, 8));

            // 웨이퍼
            if (mHasWafer[idx])
                g.FillEllipse(Brushes.RoyalBlue, x + wdt / 2 - 12, y + hgt / 2 - 4, 24, 12);

            // 진행률 텍스트
            if (mBusy[idx])
            {
                double el = mElapsedMs / 1000.0;
                double tot = mProcTime[idx] / 1000.0;
                string t = $"{el:F1}/{tot:F1}s";
                using (var pf = new Font("맑은 고딕", 9f, FontStyle.Bold))
                    TextRenderer.DrawText(g, t, pf,
                        new Rectangle(x, y + hgt - 24, wdt, 18), Color.Black,
                        TextFormatFlags.HorizontalCenter);
            }
        }

        private void DrawLegend(Graphics g, int x, int y)
        {
            var items = new (Color c, string t)[]
            {
                (CNone, "없음"), (CWait, "대기(FOUP A)"), (CBusy, "공정중"), (CDone, "완료(FOUP B)")
            };
            int cx = x;
            foreach (var it in items)
            {
                g.FillRectangle(new SolidBrush(it.c), cx, y, 14, 14);
                g.DrawRectangle(Pens.Gray, cx, y, 14, 14);
                int tw = TextRenderer.MeasureText(it.t, this.Font).Width;
                TextRenderer.DrawText(g, it.t, this.Font, new Point(cx + 18, y), Color.DimGray);
                cx += 18 + tw + 18;
            }
        }
    }
}
