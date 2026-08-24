namespace EtherCAT_Test.Auth
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  로그인 계정 한 명의 정보 그릇 (아이디, 권한 등).
    //  [보안 포인트] 비밀번호를 그대로(평문) 저장하지 않는다!
    //   - Salt(소금값): 계정마다 다른 무작위 값
    //   - PasswordHash: (Salt + 비밀번호)를 SHA-256 이라는 계산으로 뒤섞은 결과
    //  이렇게 하면 파일이 유출돼도 원래 비밀번호를 되돌리기 매우 어렵다.
    //  로그인 확인은 "입력값을 같은 방식으로 뒤섞어 저장된 값과 같은지" 비교하는 식.
    // ─────────────────────────────────────────────────────────────
    /// <summary>계정 1개. 비밀번호는 계정별 salt + SHA-256 해시로만 저장(평문 저장 금지).</summary>
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }   // Base64(SHA-256(salt + password))
        public string Salt { get; set; }           // Base64(16 bytes)
        public UserRole Role { get; set; }
        public bool MustChangePassword { get; set; }

        /// <summary>이 계정이 required 권한 이상을 보유하는지(위계 비교).</summary>
        public bool HasPermission(UserRole required) => Role >= required;
    }
}
