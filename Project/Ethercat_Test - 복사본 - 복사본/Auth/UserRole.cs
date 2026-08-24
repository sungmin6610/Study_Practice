namespace EtherCAT_Test.Auth
{
    // [이 파일] 사용자 권한 등급. 숫자가 클수록 더 높은 권한.
    // 그래서 "Role >= Engineer" 같은 비교로 '이 등급 이상인가?'를 쉽게 판단한다.
    //   Operator(0)      : 일반 조작자 — 기본 감시/자동시작 정도
    //   Engineer(1)      : 수동조작·레시피·파라미터 변경 가능
    //   Administrator(2) : 계정 관리까지 가능한 최고 권한
    /// <summary>권한 위계: Administrator > Engineer > Operator (숫자가 클수록 상위).</summary>
    public enum UserRole
    {
        Operator = 0,
        Engineer = 1,
        Administrator = 2
    }
}
