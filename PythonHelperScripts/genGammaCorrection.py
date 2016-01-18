#!/usr/local/bin/python

#Date created: 18-JAN-2016
#Author: NG
#code pilfered from LadyAda!
#https://learn.adafruit.com/led-tricks-gamma-correction/the-longer-fix
#Purpose: This table remaps linear input values (the numbers we’d like to use; e.g. 127 = half brightness) to nonlinear gamma-corrected output values (numbers producing the desired effect on the LED; e.g. 36 = half brightness).
#Great news, if we want an LED to appear at 'half' power, instead of writing '127' to the LED, we can write: table[127], which is 37! W00t for power saving :)

import numpy as np
import sys #get print without newline

gamma = 2.8
max_in = 255
max_out = 255

table = []
tmp = 0
for i in range(0,max_in):
    if i > 0: 
        sys.stdout.write(',')
    if np.mod(i,15) == 0:
        sys.stdout.write('\n ')
    tmp = np.uint8(np.round(pow(np.float32(i)/np.float(max_in),gamma) * max_out + 0.5))
    table.append(tmp)
    sys.stdout.write(str(tmp))
