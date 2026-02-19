from PIL import Image, ImageDraw
from enum import Enum


class ColorGrid(Enum):
    RED     = (251, 77, 61)
    GREEN   = (158, 227, 125)
    BLUE    = (52, 89, 149)
    WHITE   = (255, 255, 255)


def create_grid_image(
        rows: int,
        cols: int,
        cell_size: int = 50,
        line_width: int = 1,
        save_path: str = "imgsToProject/grid.png",
        color_base: ColorGrid = ColorGrid.WHITE,
        color_range: ColorGrid = ColorGrid.RED,
        colored_cells: list[list[int]] = None
        ):

    if colored_cells is None:
        colored_cells = []

    width = cols * cell_size
    height = rows * cell_size

    img = Image.new('RGB', (width + line_width, height + line_width), color_base.value)
    draw = ImageDraw.Draw(img)

    offset = line_width // 2

    for col, row in colored_cells:
        flipped_col = (cols - 1) - col 
        
        x0 = flipped_col * cell_size + line_width
        y0 = row * cell_size + line_width
        x1 = (flipped_col + 1) * cell_size
        y1 = (row + 1) * cell_size

        draw.rectangle([x0, y0, x1, y1], fill=color_range.value)

    border_color = 'black'
    
    # Draw Vertical Lines
    for i in range(cols + 1):
        x = i * cell_size + offset
        draw.line([(x, 0), (x, height + line_width)], fill=border_color, width=line_width)

    # Draw Horizontal Lines
    for j in range(rows + 1):
        y = j * cell_size + offset
        draw.line([(0, y), (width + line_width, y)], fill=border_color, width=line_width)

    img.save(save_path)


if __name__ == "__main__":
    NUM_ROWS            = 6
    NUM_COLS            = 10
    CELL_PIXELS         = 15    # * dimensioni del quadrato cella
    BORDER_THICKNESS    = 1     # * thicc
    # ---------------------

    create_grid_image(
        NUM_ROWS, 
        NUM_COLS, 
        CELL_PIXELS, 
        BORDER_THICKNESS, 
        color_range=ColorGrid.BLUE, 
        colored_cells=[])
