
import vision
from http.server import BaseHTTPRequestHandler, HTTPServer

SERVER_IP = ""
SERVER_PORT = 8083

class VisionRequestHandler(BaseHTTPRequestHandler):
    '''vision request handler'''
    
    image_files = ["jpg", "jpeg", "png", "ico", "bmp"]
    html_files = ["html", "htm"]
    css_files = ["css"]
    js_files = ["js"]

    static_root = "web-app"

    def do_GET(self):
        '''dispatch GET request'''

        if self.path == "":
            self.file_action(self.static_root + "/index.html")
        else:
            p = self.path.split(".")
            p = p[len(p) - 1].lower()
            
            if p in self.image_files:
                self.file_action(self.static_root + self.path, "image/" + p)
            elif p in self.html_files:
                self.file_action(self.static_root + self.path, "text/html")
            elif p in self.css_files:
                self.file_action(self.static_root + self.path, "text/css")
            elif p in self.js_files:
                self.file_action(self.static_root + self.path, "application/javascript")
            else:
                self.send_error(404)
    
    def do_POST(self):
        '''dispatch POST request'''
        p = self.path.strip("/")

        if p == "classification":
            self.classification_action()
        elif p == "detect":
            self.detect_action()
        else:
            self.send_error(404)
    
    def file_action(self, file, content_type):
        '''static file by GET method'''
        try:
            with open(file, "rb") as f:
                content = f.read()
                self.send(content, content_type=content_type)
        except:
            self.send_error(404)
    
    def classification_action(self):
        '''image classification API by POST method'''
        pass
    
    def detect_action(self):
        '''image detect API by POST method'''
        requets_data = self.rfile.read()
        print(requets_data)
    
    def send(self, content, code=200, content_type="text/html"):
        '''send raw data to client'''
        self.send_response(code)

        self.send_header('Content-Type', content_type)
        self.send_header('Content-Length', len(content))
        self.end_headers()

        self.wfile.write(content)
    
    def send_error(self, code):
        '''send error to client'''
        content = ""
        if code == 404:
            content = "<h2>404 Not Found!</h2>"
        elif code == 500:
            content = "<h2>500 Server Error!</h2>"
        else:
            content = "<h2>" + str(code) + " Unknown Error!</h2>"
        
        content = content.encode()

        self.send(content, code)


if __name__ == "__main__":
    server_address = (SERVER_IP, SERVER_PORT)
    server = HTTPServer(server_address, VisionRequestHandler)

    server.serve_forever()
