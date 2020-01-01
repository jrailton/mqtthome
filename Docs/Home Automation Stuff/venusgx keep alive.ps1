Set-PSDebug -Trace 2
while(1)
{
    & .\mos158\mos158\mosquitto_pub.exe -h "10.0.0.59" -t "R/7c386655e76b/system/0/Serial" -n
   start-sleep -seconds 30
}