using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MqttHome.Devices.Serial.Axpert;
using MqttHome.Devices.Serial.Pylontech;

namespace MqttHomeWeb.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            var commander = new AxpertCommand("", 115200, null);
            //pwrsys
            //@
            // Power System Information
            // -------------------------------- -
            // System is idle
            // Total Num: 3
            // Present Num              : 3
            // Sleep Num                : 0
            // System Volt              : 52988    mV
            // System Curr: 0        mA
            //System RC: 221556   mAH
            //System FCC: 221556   mAH
            //System SOC: 100 %
            //System SOH: 100 %
            //Highest voltage: 3542     mV
            //Average voltage: 3532     mV
            //Lowest voltage: 3516     mV
            //Highest temperature: 26000    mC
            //Average temperature: 25666    mC
            //Lowest temperature: 25000    mC
            //Recommend chg voltage    : 53250    mV
            //Recommend dsg voltage    : 47000    mV
            //Recommend chg current    : 22200    mA
            //Recommend dsg current    : -111000  mA
            //Command completed successfully
            //$$

            //info
            //@
            //Device address      : 1
            //Manufacturer: Pylon
            //Device name: US3000A
            //Board version: PHANTOMSAV10R03
            //Main Soft version   : B65.6
            //Soft version       : V1.3
            //Boot version       : V1.4
            //Comm version        : V2.0
            //Release Date        : 18 - 09 - 12
            //Barcode: PPTAH02139605095

            //Specification       : 48V / 74AH
            //Cell Number: 15
            //Max Dischg Curr: -100000mA
            //Max Charge Curr     : 102000mA
            //EPONPort rate: 1200
            //Console Port rate: 115200
            //Command completed successfully
            //$$

            return View();
        }
    }
}