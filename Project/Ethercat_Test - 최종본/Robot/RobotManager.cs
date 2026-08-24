using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherCAT_Test.Common;
using EtherCAT_Test.IO;

namespace EtherCAT_Test.Robot
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  웨이퍼를 집어 옮기는 이송장치(포크/블레이드) 제어.
    //   - 실린더 전진/후진 (Forward/Backward)
    //   - 진공 흡착 on/off (VacuumOn/Off) : 웨이퍼를 빨아 붙잡기
    //   - 배기(Blow) on/off : 바람을 불어 웨이퍼를 확실히 떼어놓기
    //   - 센서로 실제 상태 확인 (IsForward / IsBackward / IsVacuum)
    //  자기가 직접 하드웨어를 만지지 않고, 아래 IOManager 에게 신호 on/off 를 시킨다.
    // ─────────────────────────────────────────────────────────────
    public class RobotManager
    {
        // 신호를 켜고 끌 때 쓰는 입출력 창구(IOManager). 생성 시 받아 보관.
        private readonly IOManager io;

        public RobotManager(IOManager ioManager)
        {
            io = ioManager;
        }

        // 포크 전진: 전진 신호 켜고, 후진 신호는 꺼서 반대 명령이 겹치지 않게 한다.
        public void Forward()
        {
            io.Output(IOMap.Output.RobotForward, true);
            io.Output(IOMap.Output.RobotBackward, false);
        }

        // 포크 후진: 위와 반대 (후진 켜고 전진 끔)
        public void Backward()
        {
            io.Output(IOMap.Output.RobotForward, false);
            io.Output(IOMap.Output.RobotBackward, true);
        }

        public void VacuumOn()
        {
            io.Output(IOMap.Output.VacuumOn, true);   
        }

        public void VacuumOff()
        {
            io.Output(IOMap.Output.VacuumOn, false);  
        }

        public void BlowOn()
        {
            io.Output(IOMap.Output.VacuumBlow, true);
        }

        public void BlowOff()
        {
            io.Output(IOMap.Output.VacuumBlow, false);
        }

        // 아래 3개는 "명령"이 아니라 센서로 "실제 상태"를 확인하는 것.
        // 명령을 내려도 실제로 그렇게 됐는지는 센서로 확인해야 안전하다.

        // 포크가 실제로 전진 완료됐는가? (전진 센서 ON 이면 true)
        public bool IsForward()
        {
            return io.Input(IOMap.Input.RobotForward);
        }

        // 포크가 실제로 후진 완료됐는가?
        public bool IsBackward()
        {
            return io.Input(IOMap.Input.RobotBackward);
        }

        // 진공이 실제로 잡혔는가? (웨이퍼가 잘 흡착됐는지 확인)
        public bool IsVacuum()
        {
            return io.Input(IOMap.Input.VacuumSensor);
        }
    }
}
