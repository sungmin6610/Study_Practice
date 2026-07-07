using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace EtherCAT_Test.Common
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  프로그램 안의 데이터(레시피·계정·좌표 등)를 텍스트 파일(JSON)로 "저장"하고
    //  다시 "읽어오는" 도우미. JSON 은 { "이름": 값 } 형태의 사람이 읽을 수 있는 텍스트.
    //  - Serialize   : 프로그램 데이터 → 글자(JSON 텍스트)
    //  - Deserialize : 글자(JSON 텍스트) → 프로그램 데이터
    //  - TrySave / TryLoad : 저장/읽기를 시도하되, 실패해도 프로그램이 죽지 않게
    //                        try-catch 로 감싸 조용히 false 만 돌려준다(장비 제어 우선).
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// JSON 직렬화 유틸 (Newtonsoft 오프라인 미가용 → JavaScriptSerializer 사용).
    /// 모든 경로는 AppDomain.CurrentDomain.BaseDirectory 기준 상대 경로.
    /// 파일 I/O 는 전부 try-catch 로 감싸 실패해도 예외를 던지지 않는다.
    /// </summary>
    public static class JsonUtil
    {
        private static readonly JavaScriptSerializer Ser =
            new JavaScriptSerializer { MaxJsonLength = 32 * 1024 * 1024 };

        public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>BaseDirectory 기준 상대경로 결합.</summary>
        public static string Combine(params string[] parts)
        {
            string p = BaseDir;
            foreach (var s in parts) p = Path.Combine(p, s);
            return p;
        }

        public static string Serialize(object obj) => PrettyPrint(Ser.Serialize(obj));

        public static T Deserialize<T>(string json) => Ser.Deserialize<T>(json);

        public static bool TrySave(string fullPath, object obj)
        {
            try
            {
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(fullPath, PrettyPrint(Ser.Serialize(obj)), Encoding.UTF8);
                return true;
            }
            catch { return false; }
        }

        public static bool TryLoad<T>(string fullPath, out T result)
        {
            result = default(T);
            try
            {
                if (!File.Exists(fullPath)) return false;
                var json = File.ReadAllText(fullPath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) return false;
                result = Ser.Deserialize<T>(json);
                return result != null;
            }
            catch { return false; }
        }

        /// <summary>단일 라인 JSON 을 사람이 읽기 쉬운 들여쓰기 형태로 정리(파싱과 무관).</summary>
        public static string PrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            var sb = new StringBuilder();
            int indent = 0;
            bool inStr = false;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (inStr)
                {
                    sb.Append(c);
                    if (c == '\\' && i + 1 < json.Length) sb.Append(json[++i]);
                    else if (c == '"') inStr = false;
                    continue;
                }
                switch (c)
                {
                    case '"': inStr = true; sb.Append(c); break;
                    case '{':
                    case '[':
                        sb.Append(c).Append('\n').Append(new string(' ', ++indent * 2));
                        break;
                    case '}':
                    case ']':
                        sb.Append('\n').Append(new string(' ', --indent * 2)).Append(c);
                        break;
                    case ',':
                        sb.Append(c).Append('\n').Append(new string(' ', indent * 2));
                        break;
                    case ':':
                        sb.Append(": ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(c)) sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
