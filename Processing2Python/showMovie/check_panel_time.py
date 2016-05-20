import datetime

START_TIME_HOUR = 8
END_TIME_HOUR = 22


def good_time_to_play():
    now = datetime.datetime.now()
    if now.hour >= START_TIME_HOUR and now.hour < 22:
        if now.minute != 22:  # debug
            return True
    else:
        return False
