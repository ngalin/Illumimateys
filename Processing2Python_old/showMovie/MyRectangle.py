class MyRectangle(object):

    def __init__(self, top_corner, width, height):
        self.x = top_corner[0]
        self.y = top_corner[1]
        self.width = width
        self.height = height

    def get_bottom_right(self):
        d = self.x + self.width
        t = self.y + self.height
        return d, t
