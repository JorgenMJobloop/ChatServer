#!/usr/bin/python3
#!/usr/bin/bash

import argparse
import sys
import os
import subprocess
import time
import pathlib

def get_standard_output():
    with open("../../Internal/expected_output.txt", "r") as f:
        print(f.read()) # verify that this is the correct output, and pass it to the run function


def run():
    get_stdout = get_standard_output()
    expected_output = subprocess.Popen(["dotnet", "--version"]) # expected output from stdout
    if(expected_output != get_stdout):
       print("Installing depencies..")
       subprocess.Popen(["chmod+=x", "dotnet.sh"])
       subprocess.Popen(["./dotnet.sh", ">> success.txt"]) # must run as root
    else:
        print("Server starting...")
        time.sleep(10)
        print("Running command: 'dotnet watch --quiet run'")
        subprocess.Popen("dotnet watch --quiet run")
    
    time.sleep(10)
    print("Finished checking depencies, running application..")

def main():
    pass

if __name__ =="__main__":
    run()
