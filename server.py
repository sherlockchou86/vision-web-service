
'''
vision server, handle vision API requests
'''

from http.server import HTTPServer, SimpleHTTPRequestHandler
from PIL import Image

import urllib
import vision
import traceback
import io
import uuid
import time
import json
import os
import cgi

# server config
SERVER_IP = ""
SERVER_PORT = 8080

# API define
DETECT_ONLINE_API = "detect/online"
DETECT_LOCAL_API = "detect/local"

CLASSIFICATION_ONLINE_API = "classification/online"
CLASSIFICATION_LOCAL_API = "classification/local"

# UA for download online image
UA = {'User-Agent':'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36'}

# cache folder
CACHE_FOLDER = "image_cache"

class VisionRequestHandler(SimpleHTTPRequestHandler):
    def do_POST(self):
        '''dispatch API requests'''

        p = self.path.strip("/")

        if p == DETECT_ONLINE_API:
            self.detect_online()
        elif p == DETECT_LOCAL_API:
            self.detect_local()
        elif p == CLASSIFICATION_ONLINE_API:
            self.classification_online()
        elif p == CLASSIFICATION_LOCAL_API:
            self.classification_local()
        else:
            self.send_error(404)
    
    def detect_online(self):
        '''detect online image API'''
        key_value = self.rfile.read(int(self.headers["content-length"])).decode()

        name = key_value.split("=")[0]
        url = key_value.split("=")[1]

        # only handle the right parameter
        if name == "online_image_url":
            try:
                # download the image from url
                url = urllib.parse.unquote(url)
                req = urllib.request.Request(url, headers=UA)
                data = urllib.request.urlopen(req, timeout=5).read()
                data = io.BytesIO(data)

                # detect with vision module
                image = Image.open(data)
                detect_results = vision.detect_image(image)

                # send the json result to client
                self.send_json(detect_results)

                print("detect online image API -> " + url)
            except:
                print(traceback.print_exc())
                self.send_error(500, "detect failed for this image!")
        else:
            print("post parameter invalid!")
            self.send_error(500, "parameter invalid!")
    
    def detect_local(self):
        '''detect local image API'''
        
        try:
            data = self.rfile.read(int(self.headers["content-length"]))
            data = io.BytesIO(data)

            # need parse the base post info, and pass to cgi.FieldStorage, which can help us to parse the whole post body(with file data)
            content_type = self.headers["content-type"]
            index = content_type.find("boundary=")  # make sure multipart/form-data, we need image data from post body
            if index < 0:
                self.send_error(500, "invalid parameters!")
            else:
                fs = cgi.FieldStorage(fp=data,environ={"REQUEST_METHOD":"POST", "CONTENT_TYPE":content_type, "CONTENT_LENGTH":self.headers["content-length"]}, headers=self.headers, keep_blank_values=True)
                
                image_data = io.BytesIO(fs["local_image"].value)  # get the file data, 'local_image' is the element's name in html

                # detect with vision module
                image = Image.open(image_data)
                detect_results = vision.detect_image(image)

                # send the json result to client
                self.send_json(detect_results)

                print("detect local image API -> ")
        except:
            print(traceback.print_exc())
            self.send_error(500, "detect failed for this image!")
    
    def classification_online(self):
        '''classification for online image API'''
        pass
    
    def classification_local(self):
        '''classification for local image API'''
        pass

    def send_json(self, detect_results):
        '''send json response to client'''
        '''the format of detect_results:  (image, boxes, scores, classes)'''

        image = detect_results[0]
        boxes = detect_results[1]
        scores = detect_results[2]
        classes = detect_results[3]

        # save the image result to cache 
        cache_name = CACHE_FOLDER + "/" + str(uuid.uuid1()) + ".jpg"
        image.save(cache_name)

        # response object
        res = {}
        
        res["image"] = "/" + cache_name
        res["time"] = time.time()
        res["results"] = []

        l = len(boxes)

        if l > 0:
            for i in range(l):
                result = {}
                result["box"] = [float(value) for value in list(boxes[i])]
                result["score"] = float(scores[i])
                result["class"] = classes[i]

                res["results"].append(result)
        
        print(res)
        json_str = json.dumps(res, indent=4, sort_keys=True)

        self.send(json_str.encode(), content_type="application/json")

    def send(self, content, code=200, content_type="text/html"):
        '''send raw data to client'''
        self.send_response(code)

        self.send_header("content-type", content_type)
        self.send_header("content-length", len(content))
        self.end_headers()

        self.wfile.write(content)
    
    def send_error(self, code, error=""):
        '''send error to client'''
        content = error
        if code == 404:
            content = "<h2>404 Not Found!</h2>"
        
        content = content.encode()
        self.send(content, code)

if __name__ == "__main__":
    '''start vision server'''

    # create cache folder
    if not os.path.exists(CACHE_FOLDER):
        os.makedirs(CACHE_FOLDER)

    server_address = (SERVER_IP, SERVER_PORT)
    server = HTTPServer(server_address, VisionRequestHandler)

    print("start computer vision web server...")
    server.serve_forever()