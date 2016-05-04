import numpy as np
import cv2

gamma = 1.7
gamma_table = []


def initialise_gamma_table():
    for i in range(0, 256):
        gamma_table.append(int(np.power(i / 255.0, gamma) * 255.0 + 0.5))


# convert an integer from 0 to 100 to a float percentage
# from 0.0 to 1.0.  Special cases for 1/3, 1/6, 1/7, etc
# are handled automatically to fix integer rounding.
def percentage_float(percent):
    if percent == 33:
        return 1.0 / 3.0
    if percent == 17:
        return 1.0 / 6.0
    if percent == 14:
        return 1.0 / 7.0
    if percent == 13:
        return 1.0 / 8.0
    if percent == 11:
        return 1.0 / 9.0
    if percent == 9:
        return 1.0 / 11.0
    if percent == 8:
        return 1.0 / 12.0
    return np.double(percent / 100.0)


# scale a number by a percentage, from 0 to 100
def percentage(num, percent):
    mult = percentage_float(percent)
    return int(num * mult)


# scale a number by the inverse of a percentage, from 0 to 100
def percentage_inverse(num, percent):
    div = percentage_float(percent)
    return int(num / div)


def convert_rgb_2_bgr(color):
    red = (color & 0xFF0000) >> 16
    green = (color & 0x00FF00) >> 8
    blue = (color & 0x0000FF)

    red = gamma_table[red]
    green = gamma_table[green]
    blue = gamma_table[blue]

    return (green << 16) | (red << 8) | blue


# image2data converts an image to OctoWS2811's raw data format.
# The number of vertical pixels in the image must be a multiple
# of 8.  The data array must be the proper size for the image.
def image_to_data(image, layout):
    byte_array = []
    byte_array.insert(0, 0)
    byte_array.insert(1, 0)
    byte_array.insert(2, 0)  # reserved values
    offset = 3
    height, width, depth = image.shape
    print image.shape
    lines_per_pin = width / 8
    pixel = []

    for y in range(0, lines_per_pin):
        # if (y & 1) == (layout ? 0 : 1): # even numbered columns are left to right
        if (y & 1) == layout:
            ans = 0
        else:
            ans = 1

        if ans:
            xbegin = 0
            xend = height  # xend = image.width
            xinc = 1
        else:  # odd numbered rows are right to left
            xbegin = height - 1  # image.width - 1
            xend = -1
            xinc = -1

        for x in range(xbegin, xend, xinc):
            for i in range(0, 8):  # fetch 8 pixels from the image, 1 for each pin
                tmp = np.copy(image[x, (y + lines_per_pin * i), :])
                tmp &= 0xF0F0F0
                tmp >>= 4
                pixel.append(tmp)

            # convert 8 pixels to 24 bytes
            for i in range(0, 8):
                byte_array.insert(offset, pixel[i][0])
                byte_array.insert(offset + 1, pixel[i][1])
                byte_array.insert(offset + 2, pixel[i][2])
                offset += 3
                # mask = 0x800000
                # num_right_shifts = 6
                # for i in range(0, num_right_shifts):
                #     byte = 0
                #     for byte_count in range(0, 8):
                #         if pixel[i] & mask != 0:
                #             byte |= (1 << i)
                #
                #     byte_array[offset] = byte
                #     offset += 1
                #     mask >>= 1
    print len(byte_array)
    return byte_array

# because one of the panels is different from others, we need to compensate by inserting dummy columns
def add_dummy_columns(image, idx_dummy_columns):
    r, g, b = cv2.split(image)
    height, width = r.shape

    for i in range(0, len(idx_dummy_columns)):
        r = np.insert(r, idx_dummy_columns[i], np.zeros(height), axis=1)
        g = np.insert(g, idx_dummy_columns[i], np.zeros(height), axis=1)
        b = np.insert(b, idx_dummy_columns[i], np.zeros(height), axis=1)

    return cv2.merge((b, g, r))  # merging as bgr so as not to call 'convert_rgb_2_bgr'


def resize(frame, width, height, extra_columns_idxs):
    res = cv2.resize(frame, (width, height))
    # check that res size is 180x120
    if extra_columns_idxs is not None:
        res = add_dummy_columns(res, extra_columns_idxs)
    print res.shape  # now image size should be 184x120

    return res
