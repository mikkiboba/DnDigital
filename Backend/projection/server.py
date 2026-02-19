from flask import Flask, request, jsonify
import gridDrawer

ID_TURN: int = 2

# ROSSO ->      1
# VERDE ->      2
# BLU ->        3
# ARANCIONE ->  5

app = Flask(__name__)

@app.route('/highlight', methods=['POST'])
def get_range():
    data = request.get_json()

    if not data or 'coordinates' not in data or 'id' not in data:
        return jsonify({"status": "error", "message": "No coordinates or data found"}), 400

    coords = data['coordinates']
    id = data['id']
    highlighted_cells = []

    # * CASO MASTER
    if len(coords) == 1:
        x = coords[0].get('x')
        y = coords[0].get('y')
        if x < 0 or y < 0:
            global ID_TURN
            ID_TURN = id
            return jsonify({
            "status": "ID SET", 
            "received_count": len(coords)
            }), 200
    
    # * CASO PLAYER
    if id != ID_TURN:
        print(f"It's not your turn. Bruh. {id=}, {ID_TURN=}")
        return jsonify({
        "status": f"It's not your turn. Bruh. {id=}, {ID_TURN=}", 
        "received_count": len(coords)
        }), 403
    
    print(f"--- Received {len(coords)} coordinates to highlight ---")
    for point in coords:
        x = point.get('x')
        y = point.get('y')
        highlighted_cells.append([x,y])

    color: gridDrawer.ColorGrid = gridDrawer.ColorGrid.WHITE
    if id == 1:
        color = gridDrawer.ColorGrid.RED
    elif id == 2:
        color = gridDrawer.ColorGrid.GREEN
    elif id == 3:
        color = gridDrawer.ColorGrid.BLUE

    print(highlighted_cells)
    NUM_ROWS = 6
    NUM_COLS = 10
    CELL_PIXELS = 60  # * dimensioni del quadrato cella
    BORDER_THICKNESS = 5 # * thicc
    gridDrawer.create_grid_image(
        rows = NUM_ROWS,
        cols = NUM_COLS,
        cell_size = CELL_PIXELS,
        line_width = BORDER_THICKNESS,
        save_path = "imgsToProject/gridRange.png",
        color_range = color,
        colored_cells = highlighted_cells
    )
    return jsonify({
        "status": "success", 
        "received_count": len(coords)
    }), 200
if __name__ == '__main__':
    app.run(host='0.0.0.0', port=3487, debug=True)