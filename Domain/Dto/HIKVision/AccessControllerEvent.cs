using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.HIKVision
{
    public class AccessControllerEvent
    {
        public string deviceName { get; set; }

        public int majorEventType { get; set; }

        public int subEventType { get; set; }

        public string netUser { get; set; }

        public string remoteHostAddr { get; set; }

        public string cardNo { get; set; }

        public int cardType { get; set; }

        public int whiteListNo { get; set; }

        public int reportChannel { get; set; }

        public int cardReaderKind { get; set; }

        public int cardReaderNo { get; set; }

        public int doorNo { get; set; }

        public int verifyNo { get; set; }

        public int alarmInNo { get; set; }

        public int alarmOutNo { get; set; }

        public int caseSensorNo { get; set; }

        public int RS485No { get; set; }

        public int multiCardGroupNo { get; set; }

        public int accessChannel { get; set; }

        public int deviceNo { get; set; }

        public int distractControlNo { get; set; }

        public string employeeNoString { get; set; }

        public int localControllerID { get; set; }

        public int InternetAccess { get; set; }

        public int type { get; set; }

        public string MACAddr { get; set; }

        public int swipeCardType { get; set; }

        public int serialNo { get; set; }

        public int channelControllerID { get; set; }

        public int channelControllerLampID { get; set; }

        public int channelControllerIRAdaptorID { get; set; }

        public int channelControllerIREmitterID { get; set; }

        public string userType { get; set; }

        public string currentVerifyMode { get; set; }

        public string currentEvent { get; set; }

        public int frontSerialNo { get; set; }

        public int picturesNumber { get; set; }

        public bool remoteCheck { get; set; }



        public float currTemperature { get; set; }
        public string isAbnomalTemperature { get; set; }
        public string mask { get; set; }

        public string name { get; set; }

    }
}
