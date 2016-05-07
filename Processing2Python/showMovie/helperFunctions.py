import numpy as np
import cv2

gamma = 2.8#1.7
gamma_table = [int(((i / 255.0) ** gamma) * 255.0 + 0.5) for i in range(256)]

def bgr2grb(blue, green, red):
    blue = gamma_table[blue]
    green = gamma_table[green]
    red = gamma_table[red]
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
                pixels[i] = bgr2grb(pixel_channels[0], pixel_channels[1], pixel_channels[2])

                # Reduce intensity by discarding least significant bits
                pixels[i] &= 0xf8f8f8
                pixels[i] >>= 3

            # serialise 8 pixels to 24 bytes
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
        b = np.insert(b, idx_dummy_columns[i], np.zeros(height), axis=1)
        g = np.insert(g, idx_dummy_columns[i], np.zeros(height), axis=1)
        r = np.insert(r, idx_dummy_columns[i], np.zeros(height), axis=1)
    return cv2.merge((b, g, r))

def resize(frame, width, height, extra_columns_idxs):
    res = cv2.resize(frame, (width, height))
    # check that res size is 180x120
    if extra_columns_idxs is not None:
        res = add_dummy_columns(res, extra_columns_idxs)
    # print res.shape  # now image size should be 184x120
    return res
