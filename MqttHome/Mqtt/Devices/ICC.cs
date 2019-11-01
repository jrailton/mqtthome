using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MQTTnet;

namespace MqttHome.Mqtt.Devices
{

    public class ICC : MqttSensorDevice<ICCSensorData>
    {
        public ICC(MqttHomeController controller, string id) : base(controller, id)
        {
        }

        public override List<string> SensorTopics => new List<string>{ "Inverter/AllValues", "Inverter/AllValues2", "Pylontech/Cycles" , "Pylontech/Watts", "Pylontech/Temperature", "Pylontech/Remaining_AH", "Pylontech/TimeRemaining", "Pylontech/AH_Use", "Pylontech/AH_Remaining_Till_20SOC" };
        public override string StateTopic => null;

        public override MqttDeviceType DeviceType { get; set; } = MqttDeviceType.ICC;
        public override MqttDeviceClass DeviceClass { get; set; } = MqttDeviceClass.Sensor;

        public override void ParseStatePayload(MqttApplicationMessage message)
        {
            throw new NotImplementedException();
        }

        // loadwatts, x, pvwatts, loadwatts, loadpcnt, invertertemp, x, batteryvolts, batterysoc, batteryamps, inverterfreq, batterywatts, x, x
        //Inverter/AllValues 158 0 1343 158.00 3 46.0 0 50.30 95.00 20.38 50.0 0.0 1025 0 0
        // x, falsesoc, pvamps, x, pvvolts, x, x, invertername, icc uptime, invertervoltage, mode
        //Inverter/AllValues2 0.00 62.00 21.00 0.00 223.10 0.00 1 Axpert5kvaSingle 142:43:56 229.90 B
        //Inverter/Totals 1.79 0 2.71 0.85 0.22 6266 1341 6266 1341
        //Inverter/Name Axpert5kvaSingle
        //Inverter/LoadWatts 158
        //Inverter/InverterVolts 230
        //Inverter/InverterFreq 50.0
        //Inverter/LoadPercentage 3
        //Inverter/BatteryVolts 50.40
        //Inverter/BatteryAmps 20.38
        //Inverter/Selected 2.00
        //Inverter/BatteryWatts 1024.9
        //Inverter/BatterySOC 62.0
        //Inverter/Temperature 46.0
        //Inverter/InverterMode Battery(Solar)
        //Inverter/Off_Grid_Time 00 days 02:10:59
        //Inverter/RPI_Temperature 59.1
        //Inverter/GridWatts 0
        //Inverter/GridVoltage 0
        //Inverter/GridFreq 0.0
        //Inverter/PvWattsTotal 1343
        //Inverter/MPPT1_Watts 1343
        //Inverter/MPPT2_Watts 0
        //Inverter/MPPT3_Watts 0
        //Inverter/MPPT1_Volts 223
        //Inverter/MPPT2_Volts 0
        //Inverter/MPPT3_Volts 0
        //Inverter/MPPT1_Amps 21.0
        //Inverter/MPPT2_Amps 0.0
        //Inverter/MPPT3_Amps 0.0
        //Inverter/GridKwUse 1.79
        //Inverter/BatteryKwUse 0.65
        //Inverter/BatteryKw_Charge 0.38
        //Inverter/TotalKwUse 2.71
        //Inverter/SolarKwUse 0.85
        //Inverter/MaxLoadDay 6266
        //Inverter/MaxPvDay 1341
        //Inverter/Solar_Efficiency 0.216
        //Inverter/ICC_Runtime 142:43:56
        //Inverter/ICC_Version 2.99
        //Pylontech/Cycles 6
        //Pylontech/Amps 20.38
        //Pylontech/Volts 50.30
        //Pylontech/Watts 1025
        //Pylontech/Temperature 25.00
        //Pylontech/Remaining_AH 212.44
        //Pylontech/SOC 95
        //Pylontech/TimeRemaining  ---
        //Pylontech/AH_Use 9.56
        //Pylontech/Selected 1
        //Pylontech/AH_Remaining_Till_20SOC 168.04
        //Inverter/Month_Grid_Total 40.44
        //Inverter/Month_Total_Use 58.72
        //Inverter/Month_PV_Total 31.99
        //Inverter/Month_Cost_Saving 2.19
        //Inverter/Month_TotalCost 4.85
        //BMV/DepthOfLastDischarge 0.00
        //BMV/AH_Consumed 0.00
        //BMV/Watts 1025
        //BMV/Amps 20.38
        //BMV/Voltage 50.30
        //BMV/MidpointVoltage 0.00
        //BMV/Midpoint 0.0
        //BMV/MidpointDeviation 0.00
        //BMV/ChargeEnergy 0.00
        //BMV/DischargeEnergy 0.00
        //BMV/Efficiency 0.00
        //BMV/DepthOfDeepestDischarge 0.00
        //BMV/DepthOfAverageDischarge 0.00
        //BMV/NumberOfChargeCycles 0
        //BMV/NumberOfFullDischarges 0
        //BMV/TimeSinceLastFullCharge 00 days 00:00:00
        //BMV/SOC 95.00
        //BMV/TimeToGo 0
        //BMV/TimeTo100 0
    }
}
