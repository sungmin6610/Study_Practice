using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherCAT_Test.Logging;

namespace EtherCAT_Test.Alarm
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  "지금 알람이 떠 있는가?"와 "무슨 알람인가?"를 기억하는 아주 단순한 게시판.
    //  SetAlarm 으로 알람을 켜면 로그도 함께 남기고, Clear 로 알람을 끈다.
    //  화면(Form1)은 이 값을 보고 빨간 배너를 띄우거나 지운다.
    // ─────────────────────────────────────────────────────────────
    public class AlarmManager
    {
        public bool HasAlarm { get; private set; }      // 알람 떠 있음? (true/false)

        public string AlarmMessage { get; private set; } // 알람 내용 글

        // 알람 발생: 상태를 켜고 메시지를 저장한 뒤, 기록(로그/이벤트)도 남긴다.
        public void SetAlarm(string message)
        {
            HasAlarm = true;
            AlarmMessage = message;
            LogManager.Instance.LogAlarm(message);                       // ALARM 로그(+발생 스텝)
            LogManager.Instance.EventReport(3001, "AlarmSet", message);  // E30 이벤트
        }

        // 알람 해제: 껐고, 원래 켜져 있었던 경우에만 '해제됨' 이벤트를 남긴다.
        public void Clear()
        {
            bool was = HasAlarm;   // 지우기 전에 원래 켜져 있었는지 기억
            HasAlarm = false;
            AlarmMessage = "";
            if (was) LogManager.Instance.EventReport(3002, "AlarmClear", "");  // 알람 해제 이벤트
        }
    }
}
