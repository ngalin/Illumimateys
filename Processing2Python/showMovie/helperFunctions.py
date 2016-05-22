import time

import cv2

import numpy as np
from bitarray import bitarray


def make_gamma_table(g):
    return [int(((i / 255.0) ** g) * 255.0 + 0.5) for i in range(256)]

def reverse_bits(x):
    result = 0
    for i in xrange(8):
        if (x >> i) & 1: result |= 1 << (8 - 1 - i)
    return result

led_gamma = 2.8#1.7
led_gamma_table = make_gamma_table(led_gamma) #[int(((i / 255.0) ** led_gamma) * 255.0 + 0.5) for i in range(256)]
# Reduce LED intensity by discarding least significant bits
led_gamma_table = [c >> 3 for c in led_gamma_table]

led_table_np = np.array(led_gamma_table, dtype=np.uint8)
# led_table_np_reversed = np.array([reverse_bits(i) for i in led_gamma_table], dtype=np.uint8)
bitswap_table = np.array([reverse_bits(i) for i in range(256)], dtype=np.uint8)


def bgr2grb(b, g, r):
    blue = led_gamma_table[b]
    green = led_gamma_table[g]
    red = led_gamma_table[r]
    return (green << 16) | (red << 8) | blue

# image2data converts an image to OctoWS2811's raw data format.
# The number of vertical pixels in the image must be a multiple
# of 8.  The data array must be the proper size for the image.
def image_to_data_original(image, strip_layout_direction):
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

mean_dur = 0.0105

def image_to_data_fast(image, strip_layout_direction):
    global mean_dur
    tstart = time.time()
    height, width, depth = image.shape
    image = cv2.flip(image,1)

    teensy_pins = 8
    rows_per_pin = width / teensy_pins

    fwd_range = tuple(range(0, height, 1))
    bwd_range = tuple(range(height-1, -1, -1))

    # The image is in BGR, roll the channel axis to get GRB
    image = np.roll(image, -1, axis=2)

    bit_chunks = []

    for y in range(0, rows_per_pin):
        # Even strips are indexed forward, odd strips backwards.
        if (y % 2) != strip_layout_direction:
            rng = fwd_range
        else:
            rng = bwd_range
        for x in rng:
            # Collect one pixel per teensy pin, 8 x 3 bytes (g, r, b)
            t1 = time.time()

            # fetch teensy_pins pixels from the image, 1 for each strip
            pixel_arr = image[x, y:y+rows_per_pin*teensy_pins:rows_per_pin, :]

            # Look up gamma-corrected LED values
            pixel_arr = led_table_np[pixel_arr]
            pixel_bits_np = np.unpackbits(pixel_arr)
            t2 = time.time()

            # Serialise pixels to 3 bytes per pin, 1 bit at a time.
            # The most significant bit for each pin goes first.
            # This relies on teensy_pins <= 8 so it fits in a byte.
            bit_chunks.append(pixel_bits_np.reshape((8, 24)).T)

            t3 = time.time()
            # print (t2-t1)*1000000, (t3-t2)*1000000

    all_bits_np = np.zeros((24,), dtype=np.bool)
    all_bits_np = np.append(all_bits_np, bit_chunks)

    packed = np.packbits(all_bits_np)
    packed = bitswap_table[packed]
    bytearr = bytearray(packed.tobytes())

    # tend = time.time()
    # duration = (tend-tstart)
    # mean_dur = (mean_dur * 0.98) + (duration * 0.02)
    # print mean_dur*1000, duration*1000

    # assert len(bytearr) == 11523 # not true for the last teensy
    return bytearr



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
