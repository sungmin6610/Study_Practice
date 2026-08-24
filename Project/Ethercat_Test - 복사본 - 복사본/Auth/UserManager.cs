using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EtherCAT_Test.Common;
using EtherCAT_Test.Logging;

namespace EtherCAT_Test.Auth
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  로그인/계정 관리를 담당하는 싱글턴(UserManager.Instance 하나만 존재).
    //   - 계정 목록을 Config/users.json 파일로 저장/복원
    //   - 로그인 확인(TryLogin), 로그인 5회 실패 시 30초 잠금(무차별 대입 방지)
    //   - 비밀번호 변경, 관리자용 계정 추가/삭제/권한변경/비번초기화
    //  비밀번호는 절대 그대로 저장하지 않고 salt+SHA-256 해시로만 저장한다(User.cs 참고).
    //  처음 실행 시 계정이 없으면 기본 관리자(admin/admin, 최초 로그인 시 변경 강제)를 만든다.
    // ─────────────────────────────────────────────────────────────

    // 로그인 결과 3가지: 성공 / 아이디·비번 틀림 / 계정 잠김
    public enum AuthResult { Success, BadCredentials, Locked }

    /// <summary>
    /// 계정 저장(Config/users.json)/인증/권한/잠금 관리 (싱글턴).
    /// 비밀번호는 계정별 salt + SHA-256 해시로만 저장. 로그는 LogManager(LEVEL=AUTH).
    /// </summary>
    public class UserManager
    {
        private static readonly UserManager _instance = new UserManager();
        public static UserManager Instance => _instance;

        private const int MaxFails = 5;
        private const int LockSeconds = 30;

        private readonly string _path;
        private readonly object _lock = new object();
        private List<User> _users = new List<User>();
        private readonly Dictionary<string, int> _fails =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _lockUntil =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        /// <summary>로그인된 사용자. 로그아웃 시 null(최소 권한).</summary>
        public User CurrentUser { get; private set; }

        public IReadOnlyList<User> Users { get { lock (_lock) return _users.ToList(); } }

        private UserManager()
        {
            _path = JsonUtil.Combine("Config", "users.json");
            Load();
        }

        private void Load()
        {
            List<User> list;
            if (JsonUtil.TryLoad<List<User>>(_path, out list) && list != null && list.Count > 0)
                _users = list;
            else
            {
                _users = new List<User> { CreateDefaultAdmin() };
                Save();   // 기본 관리자(admin/"admin", MustChangePassword=true) 자동 생성
            }
        }

        private void Save() => JsonUtil.TrySave(_path, _users);

        private static User CreateDefaultAdmin()
        {
            string salt = NewSalt();
            return new User
            {
                Username = "admin",
                Salt = salt,
                PasswordHash = Hash("admin", salt),
                Role = UserRole.Administrator,
                MustChangePassword = true
            };
        }

        // ─────────── 해시 (SHA-256 + salt) ───────────
        private static string NewSalt()
        {
            var b = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(b);
            return Convert.ToBase64String(b);
        }

        // 비밀번호를 되돌릴 수 없게 뒤섞는 함수.
        // (salt + 비밀번호) 를 이어 붙인 뒤 SHA-256 으로 계산해 Base64 글자로 만든다.
        // 같은 입력이면 항상 같은 결과가 나오므로, 로그인 때 이 결과끼리 비교해 확인한다.
        public static string Hash(string password, string saltBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);
            var pw = Encoding.UTF8.GetBytes(password ?? "");
            var buf = new byte[salt.Length + pw.Length];
            Buffer.BlockCopy(salt, 0, buf, 0, salt.Length);
            Buffer.BlockCopy(pw, 0, buf, salt.Length, pw.Length);
            using (var sha = SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(buf));
        }

        private User Find(string username)
            => _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        // ─────────── 인증/잠금 ───────────
        public bool IsLocked(string username, out int remainSec)
        {
            remainSec = 0;
            username = (username ?? "").Trim();
            if (_lockUntil.TryGetValue(username, out var until))
            {
                if (DateTime.Now < until)
                {
                    remainSec = (int)Math.Ceiling((until - DateTime.Now).TotalSeconds);
                    return true;
                }
                _lockUntil.Remove(username);
                _fails[username] = 0;
            }
            return false;
        }

        // 로그인 시도. 결과(AuthResult)를 돌려주고, 성공 시 out 으로 그 계정을 넘겨준다.
        // (out = 함수가 결과를 여러 개 돌려줄 때 쓰는, 밖의 변수에 채워주는 방식)
        // 흐름: 잠겼나? → 아이디 찾고 비번 해시 비교 → 맞으면 성공, 틀리면 실패횟수 +1,
        //       5회 넘으면 30초 잠금.
        public AuthResult TryLogin(string username, string password, out User user)
        {
            user = null;
            username = (username ?? "").Trim();   // 앞뒤 공백 제거, null 이면 빈 문자열로
            lock (_lock)
            {
                if (IsLocked(username, out int rem))
                {
                    LogManager.Instance.LogEvent("AUTH", $"로그인 잠금({rem}s 남음) - {username}");
                    return AuthResult.Locked;
                }

                var u = Find(username);
                if (u != null && Hash(password, u.Salt) == u.PasswordHash)
                {
                    _fails[username] = 0;
                    CurrentUser = u;
                    user = u;
                    LogManager.Instance.LogEvent("AUTH", $"로그인 성공 - {username} ({u.Role})");
                    return AuthResult.Success;
                }

                int c = (_fails.TryGetValue(username, out var cc) ? cc : 0) + 1;
                _fails[username] = c;
                if (c >= MaxFails)
                {
                    _lockUntil[username] = DateTime.Now.AddSeconds(LockSeconds);
                    LogManager.Instance.LogEvent("AUTH", $"로그인 {MaxFails}회 실패 → {LockSeconds}s 잠금 - {username}");
                }
                else
                    LogManager.Instance.LogEvent("AUTH", $"로그인 실패({c}/{MaxFails}) - {username}");
                return AuthResult.BadCredentials;
            }
        }

        public void Logout()
        {
            if (CurrentUser != null)
                LogManager.Instance.LogEvent("AUTH", $"로그아웃 - {CurrentUser.Username}");
            CurrentUser = null;
        }

        // ─────────── 비밀번호 변경(본인/관리자) ───────────
        public bool ChangePassword(string username, string newPassword)
        {
            lock (_lock)
            {
                var u = Find(username);
                if (u == null) return false;
                u.Salt = NewSalt();
                u.PasswordHash = Hash(newPassword, u.Salt);
                u.MustChangePassword = false;
                Save();
                LogManager.Instance.LogEvent("AUTH", $"비밀번호 변경 - {username} (by {Actor()})");
                return true;
            }
        }

        // ─────────── 관리자 전용 CRUD ───────────
        public bool AddUser(string username, string password, UserRole role)
        {
            lock (_lock)
            {
                username = (username ?? "").Trim();
                if (username.Length == 0 || Find(username) != null) return false;
                string salt = NewSalt();
                _users.Add(new User
                {
                    Username = username,
                    Salt = salt,
                    PasswordHash = Hash(password, salt),
                    Role = role,
                    MustChangePassword = true
                });
                Save();
                LogManager.Instance.LogEvent("AUTH", $"계정 추가 - {username} ({role}) (by {Actor()})");
                return true;
            }
        }

        public bool DeleteUser(string username)
        {
            lock (_lock)
            {
                var u = Find(username);
                if (u == null) return false;
                if (CurrentUser != null &&
                    string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
                    return false;   // 본인 삭제 금지
                if (u.Role == UserRole.Administrator && AdminCount() <= 1)
                    return false;   // 마지막 관리자 보호
                _users.Remove(u);
                Save();
                LogManager.Instance.LogEvent("AUTH", $"계정 삭제 - {username} (by {Actor()})");
                return true;
            }
        }

        public bool ChangeRole(string username, UserRole role)
        {
            lock (_lock)
            {
                var u = Find(username);
                if (u == null) return false;
                if (u.Role == UserRole.Administrator && role != UserRole.Administrator && AdminCount() <= 1)
                    return false;   // 마지막 관리자 강등 금지
                u.Role = role;
                Save();
                LogManager.Instance.LogEvent("AUTH", $"역할 변경 - {username} → {role} (by {Actor()})");
                return true;
            }
        }

        public bool ResetPassword(string username, string newPassword)
        {
            lock (_lock)
            {
                var u = Find(username);
                if (u == null) return false;
                u.Salt = NewSalt();
                u.PasswordHash = Hash(newPassword, u.Salt);
                u.MustChangePassword = true;
                Save();
                LogManager.Instance.LogEvent("AUTH", $"비밀번호 초기화 - {username} (by {Actor()})");
                return true;
            }
        }

        private int AdminCount() => _users.Count(x => x.Role == UserRole.Administrator);
        private string Actor() => CurrentUser?.Username ?? "system";
    }
}
