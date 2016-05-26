import datetime

START_TIME_HOUR = 17 #5pm
END_TIME_HOUR = 23 #11pm

MINUTES_PAST_HOUR = 30

def good_time_to_play():
    now = datetime.datetime.now()
    if now.hour >= START_TIME_HOUR and now.hour < END_TIME_HOUR:
        if now.minute > MINUTES_PAST_HOUR: #turn on at 5:30pm
            return True
    else:
        return False
