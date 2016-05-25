import datetime

START_TIME_HOUR = 8
END_TIME_HOUR = 22


def good_time_to_play():
    now = datetime.datetime.now()
    if now.hour >= START_TIME_HOUR and now.hour < 22:
        return True
    else:
        return False
