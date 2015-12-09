import serial
ser = serial.Serial(0)  # open first serial port
print ser.portstr       # check which port was really used
ser.write("hello")      # write a string
ser.close()    
