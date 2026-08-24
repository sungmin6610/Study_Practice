using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherCAT_Test.Common;
using IEG3268_Dll;

namespace EtherCAT_Test.IO
{
    // ─────────────────────────────────────────────────────────────
    //  [이 클래스가 하는 일]
    //  디지털 입출력(ON/OFF 신호)을 읽고 쓰는 가장 밑단 도우미.
    //  실제 하드웨어 통신은 외부 DLL(IEG3268)이 하고, 이 클래스는 그 앞에 붙은
    //  '얇은 창구'다. 다른 코드는 IEG3268 을 직접 부르지 않고 이 창구만 쓰면 되므로,
    //  나중에 통신 방식이 바뀌어도 여기만 고치면 된다.
    //  bool = 참(true)/거짓(false), 즉 신호가 켜짐/꺼짐 두 가지뿐인 값.
    // ─────────────────────────────────────────────────────────────
    public class IOManager
    {
        // readonly: 생성자에서 한 번 정해지면 그 뒤로는 바꿀 수 없는 통신 객체.
        private readonly IEG3268 _ethercat;

        // 생성자: 사용할 EtherCAT 통신 객체를 밖에서 받아 보관한다.
        public IOManager(IEG3268 ethercat)
        {
            _ethercat = ethercat;
        }

        // index 번 입력 신호가 켜져 있으면 true, 아니면 false 를 돌려준다. (읽기)
        public bool Input(int index)
        {
            return _ethercat.Digital_Input(index);
        }

        // index 번 출력 신호를 value(true=켜기 / false=끄기) 로 만든다. (쓰기)
        public void Output(int index, bool value)
        {
            _ethercat.Digital_Output(index, value);
        }
    }
}
