using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EtherCAT_Test.Common;

namespace EtherCAT_Test.Process
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  '레시피' = 요리 레시피처럼 "각 챔버에서 몇 초씩 처리할지" 적어둔 설정표.
    //  세 종류의 클래스가 나온다:
    //   - RecipeStep    : 챔버 하나의 공정 1줄 (예: A챔버 PR도포 3000ms)
    //   - Recipe        : 그런 줄(Step)들을 A→B→C 로 묶은 레시피 한 장
    //   - RecipeManager : 레시피 파일들을 읽고/저장하고/현재 레시피를 고르는 관리자
    // ─────────────────────────────────────────────────────────────

    /// <summary>레시피 1스텝(챔버 1개 공정).</summary>
    public class RecipeStep
    {
        public ChamberType ChamberType { get; set; }
        public string ProcessName { get; set; }
        public int ProcessTimeMs { get; set; }
    }

    /// <summary>포토공정 레시피 (A→B→C 3스텝).</summary>
    public class Recipe
    {
        public string RecipeId { get; set; }
        public string Description { get; set; }
        public List<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
    }

    /// <summary>
    /// Recipes 폴더의 *.json 을 로드/저장/목록조회.
    /// 기본 레시피(SCA)가 없으면 자동 생성. 파일 I/O 실패 시 기본값 사용.
    /// </summary>
    public class RecipeManager
    {
        private readonly string _dir;   // BaseDirectory/Recipes

        public List<Recipe> Recipes { get; private set; } = new List<Recipe>();
        public Recipe CurrentRecipe { get; set; }

        // 생성자: 프로그램 시작 때 Recipes 폴더의 레시피들을 모두 읽는다.
        // 하나도 없으면 기본 레시피(SCA)를 만들어 넣고, 첫 번째를 '현재 레시피'로 삼는다.
        public RecipeManager()
        {
            _dir = JsonUtil.Combine("Recipes");
            LoadAll();

            if (Recipes.Count == 0)
            {
                var def = CreateDefault();
                Recipes.Add(def);
                Save(def);          // 실패해도 메모리상 기본값으로 계속 동작
            }

            CurrentRecipe = Recipes[0];
        }

        /// <summary>기본 레시피 SCA: PR도포 3000 / 노광 5000 / 현상 3000 ms.</summary>
        public static Recipe CreateDefault()
        {
            return new Recipe
            {
                RecipeId = "SCA",
                Description = "표준 포토공정 레시피",
                Steps = new List<RecipeStep>
                {
                    new RecipeStep { ChamberType = ChamberType.A, ProcessName = "PR 도포", ProcessTimeMs = 3000 },
                    new RecipeStep { ChamberType = ChamberType.B, ProcessName = "노광",    ProcessTimeMs = 5000 },
                    new RecipeStep { ChamberType = ChamberType.C, ProcessName = "현상",    ProcessTimeMs = 3000 },
                }
            };
        }

        public void LoadAll()
        {
            Recipes = new List<Recipe>();
            try
            {
                if (!Directory.Exists(_dir)) return;
                foreach (var f in Directory.GetFiles(_dir, "*.json"))
                {
                    if (JsonUtil.TryLoad<Recipe>(f, out var r)
                        && r != null && !string.IsNullOrEmpty(r.RecipeId) && r.Steps != null)
                    {
                        Recipes.Add(r);
                    }
                }
            }
            catch { /* 로드 실패는 무시하고 기본값 경로로 진행 */ }
        }

        public bool Save(Recipe r)
        {
            if (r == null || string.IsNullOrEmpty(r.RecipeId)) return false;
            return JsonUtil.TrySave(Path.Combine(_dir, r.RecipeId + ".json"), r);
        }

        public Recipe GetById(string id)
            => Recipes.FirstOrDefault(x => x.RecipeId == id);

        /// <summary>메모리 목록에 추가 또는 동일 ID 교체.</summary>
        public void AddOrReplace(Recipe r)
        {
            if (r == null || string.IsNullOrEmpty(r.RecipeId)) return;
            Recipes.RemoveAll(x => x.RecipeId == r.RecipeId);
            Recipes.Add(r);
        }

        /// <summary>현재 레시피에서 챔버 타입의 공정시간(ms). 없으면 fallback.</summary>
        // 예: A챔버 시간을 물으면 현재 레시피에서 A스텝을 찾아 그 시간을 준다.
        // 그런 스텝이 없거나 값이 0 이하이면, 대신 fallback(예비 기본값)을 돌려준다.
        // '?.' 는 앞이 null(없음)이면 그냥 null 로 넘어가 오류를 막아주는 안전장치.
        public int GetProcessTimeMs(ChamberType type, int fallback)
        {
            var step = CurrentRecipe?.Steps?.FirstOrDefault(s => s.ChamberType == type);
            return (step != null && step.ProcessTimeMs > 0) ? step.ProcessTimeMs : fallback;
        }
    }
}
