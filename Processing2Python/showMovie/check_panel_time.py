import datetime

START_TIME_HOUR = 18
MINUTES_PAST_HOUR = 50
END_TIME_HOUR = 23 #11pm

def good_time_to_play():
    now = datetime.datetime.now()

#    if now.day == 4:
#        return False
    
    if now.hour >= START_TIME_HOUR and now.hour < END_TIME_HOUR:
        return True
    else:
        return False
