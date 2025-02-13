#!/usr/bin/python3
#!/usr/bin/bash

import argparse
import sys
import os
import subprocess
import time
import pathlib


def run():
    
    print("Start the server? y/N? ")
    user_input = str(input())
    if user_input.lower() == "y":
        print("Server starting...")
        time.sleep(10)
        print("Running commands: 'dotnet build'\n 'dotnet run'")
        subprocess.Popen("dotnet build")
        time.sleep(10)
        subprocess.Popen("dotnet run")

def main():
    pass

if __name__ =="__main__":
    run()
