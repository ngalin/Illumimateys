import numpy as np
import cv2
import binascii

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

def int2bytes(i):
    hex_string = '%x' % i
    n = len(hex_string)
    return binascii.unhexlify(hex_string.zfill(n + (n & 1)))

def convert_rgb_2_grb(color):
    red = (color & 0xFF0000) >> 16
    green = (color & 0x00FF00) >> 8
    blue = (color & 0x0000FF)

    return grb(red, green, blue)

def grb(red, green, blue):
    red = gamma_table[red]
    green = gamma_table[green]
    blue = gamma_table[blue]
    return (green << 16) | (red << 8) | blue



# image2data converts an image to OctoWS2811's raw data format.
# The number of vertical pixels in the image must be a multiple
# of 8.  The data array must be the proper size for the image.
def image_to_data(image, strip_layout_direction):
    byte_array = bytearray(11523)
    offset = 3
    height, width, depth = image.shape
    image = cv2.flip(image,1)

    lines_per_pin = width / 8

    for y in range(0, lines_per_pin):
        # Even strips are indexed forward, odd strips backwards.
        if y % 2 == strip_layout_direction:
            forward = 0
        else:
            forward = 1
        if forward:
            xbegin = 0
            xend = height  # xend = image.width
            xinc = 1
        else:  # odd numbered rows are right to left
            xbegin = height - 1  # image.width - 1
            xend = -1
            xinc = -1
        #print 'xbeing: ' + str(xbegin) + ' xend: ' + str(xend) + ' xinc: ' + str(xinc)
        for x in range(xbegin, xend, xinc):
            pixels = [0] * 8
            for i in range(0, 8):  # fetch 8 pixels from the image, 1 for each strip
                pixel_channels = np.copy(image[x, (y + lines_per_pin * i), :])
                pixels[i] = grb(pixel_channels[0], pixel_channels[1], pixel_channels[2])

                # Reduce intensity by discarding least significant 4 bits
                pixels[i] &= 0xf0f0f0
                pixels[i] >>= 4

            # convert 8 pixels to 24 bytes
            mask = 0x800000
            while mask != 0:
                b = 0
                for i in range(0, 8):
                    if pixels[i] & mask != 0:
                        b |= (1 << i)
                byte_array[offset] = b
                offset += 1
                mask >>= 1

    return byte_array


# because one of the panels is different from others, we need to compensate by inserting dummy columns
def add_dummy_columns(image, idx_dummy_columns):
    b, g, r = cv2.split(image)
    height, width = r.shape

    for i in range(0, len(idx_dummy_columns)):
        r = np.insert(r, idx_dummy_columns[i], np.zeros(height), axis=1)
        g = np.insert(g, idx_dummy_columns[i], np.zeros(height), axis=1)
        b = np.insert(b, idx_dummy_columns[i], np.zeros(height), axis=1)
    return cv2.merge((b, g, r))

def resize(frame, width, height, extra_columns_idxs):
    res = cv2.resize(frame, (width, height))
    # check that res size is 180x120
    if extra_columns_idxs is not None:
        res = add_dummy_columns(res, extra_columns_idxs)
    # print res.shape  # now image size should be 184x120
    return res
