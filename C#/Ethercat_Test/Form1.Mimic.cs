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
        private readonly int[] mElapsed = new int[3];   // 챔버별 공정 경과(ms)
        private bool mFwd, mVac;
        private readonly bool[] mSlotA = new bool[6];  // [1..5] FOUP A 에 웨이퍼 유무
        private readonly bool[] mSlotB = new bool[6];  // [1..5] FOUP B 에 웨이퍼 유무

        private static readonly Color CNone = Color.Silver;
        private static readonly Color CWait = Color.RoyalBlue;
        private static readonly Color CDone = Color.FromArgb(39, 174, 96);
        private static readonly Color CBusy = Color.Orange;
        private static readonly Color CBg = Color.FromArgb(236, 240, 244);
        private const long StationTol = 40000;   // 스테이션 정렬 허용 밴드(LR)

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
                    mElapsed[i] = chs[i].ElapsedMs;
                }
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

            // ── 이송 로봇 : 가로 LR(구간별 정확 매핑) + 세로 UD, 챔버는 세로 진입 ──
            int railY = stTop + stH + 26;   // 레일을 챔버에 가깝게 → 블레이드 짧게
            if (railY > h - 40) railY = h - 40;
            using (var rail = new Pen(Color.DimGray, 3f))
                g.DrawLine(rail, marginX, railY, w - marginX, railY);

            // 스테이션별 LR 값 ↔ 컬럼 중심 X : 구간별 선형보간으로 정확 정렬
            double[] stLR = { Position.FOUPA_X, Position.CHAMBER_A_X, Position.CHAMBER_B_X, Position.CHAMBER_C_X, Position.FOUPB_X };
            int[] stCX = new int[cols];
            for (int i = 0; i < cols; i++) stCX[i] = colX[i] + colW / 2;
            int rx = MapRobotX(mLR, stLR, stCX);
            int near = NearestStation(mLR, stLR);
            bool atStation = Math.Abs(mLR - stLR[near]) <= StationTol;   // 허용밴드 안일 때만 '해당 스테이션'
            bool isChamber = near >= 1 && near <= 3;
            if (atStation) rx = stCX[near];   // 스테이션에 있으면 컬럼 중심 스냅

            // 현재 스테이션 하이라이트 마커
            using (var hp = new Pen(Color.RoyalBlue, 3f))
                g.DrawLine(hp, stCX[near], railY - 3, stCX[near], railY + 3);

            // 헤드 미세 상하(레일 위)
            double uf = mUD / 3100000.0;
            if (uf < 0) uf = 0; if (uf > 1) uf = 1;
            int headY = railY - (int)(uf * 20);
            int hw = 60, hh = 30, wr = 44;   // wr = 로봇 웨이퍼 지름(크게)
            int headTopY = headY - hh / 2;

            using (var udrail = new Pen(Color.Gray, 2f))
                g.DrawLine(udrail, rx, railY + 6, rx, railY - 26);

            // 블레이드 : 스테이션에 전진 상태면 세로로 박스 안으로 삽입, 아니면 후진(레일 대기)
            if (mFwd && atStation)
            {
                int tipY;
                if (isChamber)
                {
                    // 챔버 : UD(CHAMBER_UP~DOWN)로 삽입 깊이. 안착(dep=1)에서 챔버 '중앙'까지만 짧게.
                    double dep = (Position.CHAMBER_UP - (double)mUD) / (double)(Position.CHAMBER_UP - Position.CHAMBER_DOWN);
                    if (dep < 0) dep = 0; if (dep > 1) dep = 1;
                    int chShallow = stTop + stH - 22;   // 상승(dep0): 입구 근처
                    int chCenter = stTop + stH / 2;     // 안착(dep1): 챔버 중앙
                    tipY = (int)(chShallow + dep * (chCenter - chShallow));
                }
                else
                {
                    // FOUP : UD 로 가장 가까운 슬롯에 정렬 + 강조
                    int slot = NearestSlot(mUD);
                    int top = stTop + 22, areaH = stH - 28, slotH = areaH / 5, si = 5 - slot;
                    tipY = top + si * slotH + slotH / 2;
                    var sr = new Rectangle(colX[near] + 6, top + si * slotH + 2, colW - 12, slotH - 4);
                    using (var sp = new Pen(Color.OrangeRed, 3f)) g.DrawRectangle(sp, sr);
                }

                int bladeH = Math.Max(6, headTopY - tipY);
                g.FillRectangle(Brushes.DimGray, new Rectangle(rx - 7, tipY, 14, bladeH));   // 세로 블레이드
                if (mVac)   // 웨이퍼 (블레이드 끝 = 박스 안)
                {
                    g.FillEllipse(Brushes.RoyalBlue, rx - wr / 2, tipY - wr / 2, wr, wr);
                    g.DrawEllipse(Pens.MidnightBlue, rx - wr / 2, tipY - wr / 2, wr, wr);
                }
            }
            else if (mVac)   // 후진/이동 중 : 진공 웨이퍼는 헤드 위에
            {
                g.FillEllipse(Brushes.RoyalBlue, rx - wr / 2, headTopY - wr - 2, wr, wr);
                g.DrawEllipse(Pens.MidnightBlue, rx - wr / 2, headTopY - wr - 2, wr, wr);
            }

            // 헤드 (블레이드 위에 덮어 연결부 자연스럽게)
            var head = new Rectangle(rx - hw / 2, headY - hh / 2, hw, hh);
            g.FillRectangle(Brushes.SteelBlue, head);
            g.DrawRectangle(Pens.Black, head);

            // 현재 구간 텍스트
            string[] stName = { "FOUP A", "PMC-A", "PMC-B", "PMC-C", "FOUP B" };
            string atTxt = atStation ? ("@ " + stName[near]) : "(이동중)";
            using (var sf = new Font("맑은 고딕", 9f, FontStyle.Bold))
                TextRenderer.DrawText(g, "ROBOT " + atTxt, sf,
                    new Rectangle(rx - 70, railY + 8, 140, 16), Color.MidnightBlue,
                    TextFormatFlags.HorizontalCenter);

            // ── 색 범례 ──
            DrawLegend(g, marginX, h - 24);
        }

        // LR 값(좌→우로 감소)을 스테이션 컬럼 중심에 구간별 선형보간
        private static int MapRobotX(long lr, double[] stLR, int[] stCX)
        {
            if (lr >= stLR[0]) return stCX[0];
            for (int i = 0; i < stLR.Length - 1; i++)
            {
                if (lr <= stLR[i] && lr >= stLR[i + 1])
                {
                    double denom = stLR[i] - stLR[i + 1];
                    double t = denom != 0 ? (stLR[i] - lr) / denom : 0;
                    return (int)(stCX[i] + t * (stCX[i + 1] - stCX[i]));
                }
            }
            return stCX[stLR.Length - 1];
        }

        private static int NearestStation(long lr, double[] stLR)
        {
            int best = 0; double bd = double.MaxValue;
            for (int i = 0; i < stLR.Length; i++)
            {
                double d = Math.Abs(stLR[i] - lr);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        // UD 위치에 가장 가까운 FOUP 슬롯(1~5) — 상승/안착 값 중 최소 거리
        private static int NearestSlot(long ud)
        {
            int best = 1; long bd = long.MaxValue;
            for (int s = 1; s <= 5; s++)
            {
                long d = Math.Min(Math.Abs(Position.FOUPA_UP[s] - ud), Math.Abs(Position.FOUPA_DOWN[s] - ud));
                if (d < bd) { bd = d; best = s; }
            }
            return best;
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

            // 웨이퍼 (더 크게)
            if (mHasWafer[idx])
            {
                int wd = 58, wht = 30;
                g.FillEllipse(Brushes.RoyalBlue, x + wdt / 2 - wd / 2, y + hgt / 2 - wht / 2, wd, wht);
                g.DrawEllipse(Pens.MidnightBlue, x + wdt / 2 - wd / 2, y + hgt / 2 - wht / 2, wd, wht);
            }

            // 진행률 텍스트
            if (mBusy[idx])
            {
                double el = mElapsed[idx] / 1000.0;
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
