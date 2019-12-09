using System;
using System.Collections.Generic;
using System.Text;

namespace MqttHome.Devices.Serial.Pylontech
{

    public class PPGetManufacturerInfo : PylonPacket
    {
        public PPGetManufacturerInfo() : base()
        {
            CID1 = 0x46;
            CID2 = 0x51;
        }
    }

    public class PPManufacturerInfo : PylonPacket
    {
        public string DeviceName => Encoding.UTF8.GetString(INFO[0..10]).Trim();
        public byte[] SoftwareVersion => INFO[10..13];
        public string ManufacturerName => Encoding.UTF8.GetString(INFO[12..]).Trim();

        public override string ToString()
        {
            return $"{base.ToString()}DeviceName: {DeviceName}, SoftwareVersion {SoftwareVersion[0]}.{SoftwareVersion[1]}, ManufacturerName: {ManufacturerName}";
        }
    }


public class PPGetAnalogValue : PylonPacket
        {

            def __init__(self):
        super().__init__()
            self.info=bytearray(1)
            self.LENGTH=0x02
        self.CID1=0x46
        self.CID2=0x42
        

    @property
    def Command(self) :
            return self.info[0]

        @Command.setter


        def Command(self, value):

            self.info[0] = value




        def __str__(self, ** kwargs):
        return super().__str__(** kwargs)+(", Command: %s"%(self.Command))



public class PPAnalogValue : PylonPacket
            {

                def __init__(self):
        super().__init__()
            self.voltages=[]
                self.temperatures=[]

                @property
            def CellsCount(self) :
            return self.info[2]

        @property
        def CellVoltages(self):
        return self.voltages

    @property
        def TemperaturesCount(self):
        idx=(self.CellsCount*2)+3
        return self.info[idx]

    @property
        def Temperatures(self):
        return self.temperatures

    @property
        def TotalCurrent(self):
        return self.GetInt2Complement(-11)/10.0 

    @property
    def TotalVoltage(self) :
            return self.GetInt2(-9)/1000.0 

    @property
    def RemainingCapacity(self) :
            return self.GetInt2(-7)/1000.0 

    @property
    def Quantity(self) :
            return self.info[-5]

        @property
        def TotalCapacity(self):
        return self.GetInt2(-4)/1000.0  

    @property
    def Cycles(self) :
            return self.GetInt2(-2) 

    def PostParse(self):
        logging.debug("Post processing parsed data %s",self.info.hex())
        self.voltages=[]
                self.temperatures=[]
        for v in range(0, self.CellsCount) :
                cv=self.GetInt2(3+(2* v))/1000.0
            logging.debug("Voltage ",v,cv)
            self.voltages.append(cv)

        idx = (self.CellsCount * 2) + 3
        for t in range(0, self.TemperaturesCount) :
                tv=self.GetInt2Complement(idx+1+(2* t))
            tv=(tv-2731)/10.0
            logging.debug("Temperature %s: %s",v,tv)
            self.temperatures.append(tv)

    def __str__(self, ** kwargs) :
            ret=super().__str__(**kwargs)
            ret+=("\r\n>  CellsCount: %s, TemperaturesCount: %s"%(self.CellsCount, self.TemperaturesCount))
        ret+=("\r\n>  TotalCurrent: %.3f, TotalVoltage: %.3f, RemainingCapacity: %.3f, P: %.2f"%(self.TotalCurrent, self.TotalVoltage, self.RemainingCapacity, (self.TotalCurrent* self.TotalVoltage)))
        ret+=("\r\n>  Quantity: %s, TotalCapacity: %s, Cycles: %s"%(self.Quantity, self.TotalCapacity, self.Cycles))
        ret+=("\r\n>  CellVoltages: %s"%(self.CellVoltages))
        ret+=("\r\n>  Temperatures: %s"%(self.Temperatures))
        return ret

    public class PPGetSystemParameter : PylonPacket
                {
                    def __init__(self):
        super().__init__()
            self.CID1=0x46
        self.CID2=0x47

public class PPSystemParameter : PylonPacket
                    {
                        @property
                        def INFOFLAG(self) :
            return self.info[0]

        @property
        def UnitCellVoltage(self):
        return self.GetInt2(1)/1000.0

    @property
    def UnitCellLowVoltage(self) :
            return self.GetInt2(3)/1000.0

    @property
    def UnitCellUnderVoltage(self) :
            return self.GetInt2(5)/1000.0

    #TODO: Doplnit dalsi neuzitecne property
    def __str__(self, ** kwargs):
        return super().__str__(**kwargs)+("\r\n>  FLAG: %s, UnitCellVoltage: %s, UnitCellLowVoltage %s, UnitCellUnderVoltage: %s"%(bin(self.INFOFLAG), self.UnitCellVoltage,self.UnitCellLowVoltage,self.UnitCellUnderVoltage))


public class PPGetAlarmInformation : PylonPacket
                        {
                            def __init__(self):
        super().__init__()
            self.CID1=0x46
        self.CID2=0x44
        self.info=bytearray(1)
            self.LENGTH=0x02
 
    @property
    def Command(self) :
            return self.info[0]

        @Command.setter


        def Command(self, value):

            self.info[0] = value




        def __str__(self, ** kwargs):
        return super().__str__(** kwargs)+(", Command: %s"%(self.Command))


public class PPAlarmInformation : PylonPacket
                            {
                                pass


    class PPGetChargeManagementInformation : PylonPacket
                                {
                                    def __init__(self):
        super().__init__()
            self.CID1=0x46
        self.CID2=0x92
        self.info=bytearray(1)
            self.LENGTH=0x02
 
    @property
    def Command(self) :
            return self.info[0]

        @Command.setter


        def Command(self, value):

            self.info[0] = value




        def __str__(self, ** kwargs):
        return super().__str__(** kwargs)+(", Command: %s"%(self.Command))


class PPChargeManagementInformation(PylonPacket):

    @property
    def VoltageUpLimit(self) :
            return self.GetInt2(1)/1000.0

    @property
    def VoltageDownLimit(self) :
            return self.GetInt2(3)/1000.0

    @property
    def MaxChargeCurrent(self) :
            return self.GetInt2Complement(5)/1.0

    @property
    def MaxDischargeCurrent(self) :
            return self.GetInt2Complement(7)/1.0

    @property
    def Status(self) :
            return self.info[9]

        def __str__(self, ** kwargs) :
            print(self.info.hex())
            return super().__str__(**kwargs)+("\r\n>  VoltageUpLimit: %s, VoltageDownLimit: %s, MaxChargeCurrent: %s, MaxDischargeCurrent: %s, Status: %s"%(self.VoltageUpLimit, self.VoltageDownLimit, self.MaxChargeCurrent, self.MaxDischargeCurrent, self.Status))



class PPGetSeriesNumber(PylonPacket):
    def __init__(self):
        super().__init__()
            self.CID1=0x46
        self.CID2=0x93
        self.info=bytearray(1)
            self.LENGTH=0x02
 
    @property
    def Command(self) :
            return self.info[0]

        @Command.setter


        def Command(self, value):

            self.info[0] = value




        def __str__(self, ** kwargs):
        return super().__str__(** kwargs)+(", Command: %s"%(self.Command))


class PPSeriesNumber(PylonPacket):
    
    @property
    def SeriesNumber(self) :
            return self.info[1:].decode()



    def __str__(self, ** kwargs):
        return super().__str__(**kwargs)+("\r\n> Series Number: %s"%(self.SeriesNumber))

class PPTurnOff(PylonPacket):
    def __init__(self):
        super().__init__()
            self.CID1=0x46
        self.CID2=0x95
        self.info=bytearray(1)
            self.LENGTH=0x02
 
    @property
    def Command(self) :
            return self.info[0]

        @Command.setter


        def Command(self, value):

            self.info[0] = value



        def __str__(self, ** kwargs):
        return super().__str__(** kwargs)+(", Command: %s"%(self.Command))



class PPTurnOffReply(PylonPacket):
    pass
                                }
                            }
                        }
}
