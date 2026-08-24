using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EtherCAT_Test.Common;
using EtherCAT_Test.Process;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  Recipe 탭 (코드로 동적 생성 → Form1.Designer.cs 미변경).
    //  partial class Form1 이므로 private 매니저 필드(recipeManager, autoSequence)에 접근 가능.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private ListBox lstRecipes;
        private NumericUpDown nudRcpA, nudRcpB, nudRcpC;
        private Label lblRcpInfo;
        private Button btnRecipeApply, btnRecipeSave, btnRecipeNew;   // 수정 2: 활성/비활성 제어용

        private void InitRecipeTab()
        {
            var tab = new TabPage("Recipe") { BackColor = Color.White };

            // 좌측: 레시피 목록
            var gbList = new GroupBox
            {
                Text = "레시피 목록",
                Location = new Point(20, 20),
                Size = new Size(300, 430),
                Font = new Font("굴림", 10F, FontStyle.Bold)
            };
            lstRecipes = new ListBox
            {
                Location = new Point(15, 30),
                Size = new Size(270, 350),
                Font = new Font("굴림", 11F)
            };
            lstRecipes.SelectedIndexChanged += (s, e) => LoadSelectedRecipeToUi();
            gbList.Controls.Add(lstRecipes);

            // 우측: 스텝별 공정시간 편집
            var gbEdit = new GroupBox
            {
                Text = "스텝별 공정시간 (ms)",
                Location = new Point(340, 20),
                Size = new Size(440, 300),
                Font = new Font("굴림", 10F, FontStyle.Bold)
            };
            lblRcpInfo = new Label
            {
                Location = new Point(20, 30),
                Size = new Size(400, 44),
                Font = new Font("굴림", 10F),
                Text = "-"
            };
            nudRcpA = MakeTimeNud(210, 100);
            nudRcpB = MakeTimeNud(210, 150);
            nudRcpC = MakeTimeNud(210, 200);
            gbEdit.Controls.Add(lblRcpInfo);
            gbEdit.Controls.Add(new Label { Text = "A - PR 도포 :", Location = new Point(30, 103), AutoSize = true, Font = new Font("굴림", 10F) });
            gbEdit.Controls.Add(new Label { Text = "B - 노광 :", Location = new Point(30, 153), AutoSize = true, Font = new Font("굴림", 10F) });
            gbEdit.Controls.Add(new Label { Text = "C - 현상 :", Location = new Point(30, 203), AutoSize = true, Font = new Font("굴림", 10F) });
            gbEdit.Controls.Add(nudRcpA);
            gbEdit.Controls.Add(nudRcpB);
            gbEdit.Controls.Add(nudRcpC);

            // 버튼
            btnRecipeApply = new Button { Text = "선택 레시피 적용", Location = new Point(340, 340), Size = new Size(140, 44), Font = new Font("굴림", 10F, FontStyle.Bold) };
            btnRecipeSave = new Button { Text = "저장", Location = new Point(490, 340), Size = new Size(130, 44), Font = new Font("굴림", 10F) };
            btnRecipeNew = new Button { Text = "새로 만들기", Location = new Point(630, 340), Size = new Size(150, 44), Font = new Font("굴림", 10F) };
            btnRecipeApply.Click += (s, e) => ApplyRecipe();
            btnRecipeSave.Click += (s, e) => SaveRecipe();
            btnRecipeNew.Click += (s, e) => NewRecipe();

            tab.Controls.Add(gbList);
            tab.Controls.Add(gbEdit);
            tab.Controls.Add(btnRecipeApply);
            tab.Controls.Add(btnRecipeSave);
            tab.Controls.Add(btnRecipeNew);

            tabMain.TabPages.Add(tab);
            RefreshRecipeList();
        }

        private NumericUpDown MakeTimeNud(int x, int y)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(140, 27),
                Minimum = 0,
                Maximum = 600000,
                Increment = 100,
                Font = new Font("굴림", 11F)
            };
        }

        private bool BlockIfAutoRun()
        {
            if (autoSequence.AutoRun)
            {
                MessageBox.Show("자동운전 중에는 레시피를 변경/적용할 수 없습니다.\n\n자동공정을 정지한 후 시도하세요.");
                return true;
            }
            return false;
        }

        private void RefreshRecipeList()
        {
            lstRecipes.Items.Clear();
            foreach (var r in recipeManager.Recipes)
                lstRecipes.Items.Add(r.RecipeId);

            int idx = recipeManager.Recipes.FindIndex(
                r => r.RecipeId == recipeManager.CurrentRecipe?.RecipeId);
            if (idx >= 0) lstRecipes.SelectedIndex = idx;
            else if (lstRecipes.Items.Count > 0) lstRecipes.SelectedIndex = 0;
        }

        private Recipe SelectedRecipe()
        {
            if (lstRecipes.SelectedItem == null) return null;
            return recipeManager.GetById(lstRecipes.SelectedItem.ToString());
        }

        private void LoadSelectedRecipeToUi()
        {
            var r = SelectedRecipe();
            if (r == null) return;
            bool applied = r.RecipeId == recipeManager.CurrentRecipe?.RecipeId;
            lblRcpInfo.Text = $"ID : {r.RecipeId}\r\n{r.Description}" + (applied ? "     [ 적용중 ]" : "");
            nudRcpA.Value = ClampTime(StepTime(r, ChamberType.A));
            nudRcpB.Value = ClampTime(StepTime(r, ChamberType.B));
            nudRcpC.Value = ClampTime(StepTime(r, ChamberType.C));
        }

        private static int StepTime(Recipe r, ChamberType t)
        {
            var s = r.Steps?.FirstOrDefault(x => x.ChamberType == t);
            return s?.ProcessTimeMs ?? 0;
        }

        private decimal ClampTime(int ms)
        {
            if (ms < 0) ms = 0;
            if (ms > 600000) ms = 600000;
            return ms;
        }

        private void SetStepTime(Recipe r, ChamberType t, string name, int ms)
        {
            var s = r.Steps?.FirstOrDefault(x => x.ChamberType == t);
            if (s == null)
            {
                if (r.Steps == null) r.Steps = new System.Collections.Generic.List<RecipeStep>();
                s = new RecipeStep { ChamberType = t, ProcessName = name };
                r.Steps.Add(s);
            }
            s.ProcessTimeMs = ms;
        }

        private void ApplyRecipe()
        {
            if (BlockIfAutoRun()) return;
            var r = SelectedRecipe();
            if (r == null) return;
            recipeManager.CurrentRecipe = r;   // Chamber.ProcessTimeMs 가 실시간 반영
            LoadSelectedRecipeToUi();
            MessageBox.Show($"레시피 [{r.RecipeId}] 를 적용했습니다.");
        }

        private void SaveRecipe()
        {
            if (BlockIfAutoRun()) return;
            var r = SelectedRecipe();
            if (r == null) return;
            SetStepTime(r, ChamberType.A, "PR 도포", (int)nudRcpA.Value);
            SetStepTime(r, ChamberType.B, "노광", (int)nudRcpB.Value);
            SetStepTime(r, ChamberType.C, "현상", (int)nudRcpC.Value);
            bool ok = recipeManager.Save(r);
            LoadSelectedRecipeToUi();
            MessageBox.Show(ok ? $"레시피 [{r.RecipeId}] 저장 완료." : "레시피 저장 실패(파일 I/O).");
        }

        private void NewRecipe()
        {
            if (BlockIfAutoRun()) return;
            var id = "RCP" + DateTime.Now.ToString("HHmmss");
            var r = new Recipe
            {
                RecipeId = id,
                Description = "새 레시피",
                Steps = new System.Collections.Generic.List<RecipeStep>
                {
                    new RecipeStep { ChamberType = ChamberType.A, ProcessName = "PR 도포", ProcessTimeMs = (int)nudRcpA.Value },
                    new RecipeStep { ChamberType = ChamberType.B, ProcessName = "노광",    ProcessTimeMs = (int)nudRcpB.Value },
                    new RecipeStep { ChamberType = ChamberType.C, ProcessName = "현상",    ProcessTimeMs = (int)nudRcpC.Value },
                }
            };
            recipeManager.AddOrReplace(r);
            recipeManager.Save(r);
            RefreshRecipeList();
            lstRecipes.SelectedIndex = lstRecipes.Items.IndexOf(id);
            MessageBox.Show($"새 레시피 [{id}] 생성. (적용하려면 '선택 레시피 적용')");
        }
    }
}
