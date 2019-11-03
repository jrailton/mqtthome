$i = 1

while(1){
	write-host "loop number $i"
	& "C:\Program Files\mosquitto\mosquitto_pub.exe" -h 10.0.0.59 -t "R/7c386655e76b/system/0/Serial" -n
	sleep 30
	$i++ 
 }