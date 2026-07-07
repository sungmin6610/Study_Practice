import webbrowser
import os

def showMap(map):
    map.save("map.html")
    filepath = os.getcwd()
    file_uri = 'file:///' + filepath + '/map.html'
    webbrowser.open_new_tab(file_uri)