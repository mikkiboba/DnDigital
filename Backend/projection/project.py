import dlnap
import time
import os
import shutil
import socket
import threading
from http.server import SimpleHTTPRequestHandler
from socketserver import TCPServer

PROJECTOR_IP = "192.168.1.13"
IMAGE_PATH = "/Users/mikki/Documents/dnd-cpippi/projection/imgsToProject/grid.png"
RANGE_IMAGE_PATH = "/Users/mikki/Documents/dnd-cpippi/projection/imgsToProject/gridRange.png"
ACTIVE_IMAGE_PATH = "/Users/mikki/Documents/dnd-cpippi/projection/imgsToProject/active_view.png"
HOST_PORT = 8000

def get_local_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect((PROJECTOR_IP, 1))
        ip = s.getsockname()[0]
    except Exception:
        ip = '127.0.0.1'
    finally:
        s.close()
    return ip

def update_active_image(source_path):
    try:
        if os.path.exists(source_path):
            shutil.copy2(source_path, ACTIVE_IMAGE_PATH)
            return True
    except Exception as e:
        print(f"Error copying file: {e}")
    return False

def serve_file(directory):
    os.chdir(directory)
    TCPServer.allow_reuse_address = True
    httpd = TCPServer(("", HOST_PORT), SimpleHTTPRequestHandler)
    thread = threading.Thread(target=httpd.serve_forever)
    thread.daemon = True
    thread.start()
    return httpd

WOOPER_FILE = "wooper.wooper"

def ensure_wooper_dir():
    directory = os.path.dirname(WOOPER_FILE)
    if directory:
        os.makedirs(directory, exist_ok=True)

def cast_image():
    ensure_wooper_dir()  
    file_dir = os.path.dirname(IMAGE_PATH)
    local_ip = get_local_ip()
    serve_file(file_dir)
    
    print(f"Connecting to {PROJECTOR_IP}...")
    devices = dlnap.discover(ip=PROJECTOR_IP, timeout=3)
    
    if not devices:
        print(f"Error: Could not communicate with projector at {PROJECTOR_IP}.")
        return

    device = devices[0]
    range_start_time = None
    is_showing_range = False

    update_active_image(IMAGE_PATH)
    
    initial_url = f"http://{local_ip}:{HOST_PORT}/active_view.png?t=start"
    device.set_current_media(url=initial_url)
    device.play()
    
    try:
        while True:
            range_exists = os.path.exists(RANGE_IMAGE_PATH)
            should_change = False

            if range_exists and not is_showing_range:
                print(">>> Range detected! Switching...")
                
                with open(WOOPER_FILE, 'w') as f:
                    f.write('wooper')
                
                update_active_image(RANGE_IMAGE_PATH)
                range_start_time = time.time()
                is_showing_range = True
                should_change = True

            elif is_showing_range:
                if not range_exists or (time.time() - range_start_time >= 5):
                    print(">>> Reverting to Grid...")
                    
                    if os.path.exists(RANGE_IMAGE_PATH):
                        try: os.remove(RANGE_IMAGE_PATH)
                        except: pass
                        
                    update_active_image(IMAGE_PATH)
                    is_showing_range = False
                    range_start_time = None
                    
                    timestamp_url = f"http://{local_ip}:{HOST_PORT}/active_view.png?t={int(time.time() * 1000)}"
                    try:
                        device.set_current_media(url=timestamp_url)
                        device.play()
                    except Exception as e:
                        print(f"Projector error: {e}")

                    print(">>> Waiting for projector to stabilize...")
                    time.sleep(1.5) 

                    if os.path.exists(WOOPER_FILE):
                        try: 
                            os.remove(WOOPER_FILE)
                            print(">>> Resume C++ detection.")
                        except: pass
                    
                    should_change = False

            if should_change:
                timestamp_url = f"http://{local_ip}:{HOST_PORT}/active_view.png?t={int(time.time() * 1000)}"
                try:
                    device.set_current_media(url=timestamp_url)
                    device.play()
                except Exception as e:
                    print(f"Projector communication error: {e}")

            time.sleep(0.5)

    except KeyboardInterrupt:
        if os.path.exists(WOOPER_FILE):
            os.remove(WOOPER_FILE)
        print("Closing...")

if __name__ == "__main__":
    cast_image()