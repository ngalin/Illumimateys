import datetime

START_TIME_HOUR = 8
END_TIME_HOUR = 22


def check_time():
    now = datetime.datetime.now()
    # print now.hour

    if now.hour >= START_TIME_HOUR and now.hour < 22:
        if now.minute != 22:  # debug
            return True
    else:
        return False
