#!usr/bin/bash

# this needs to run with sudo permissions

sudo apt-get install -y dotnet-runtime-8.0

# Ok

echo "Running Python crypto module"

python3 CreateKeyPair.py 

python3 ShellScript.py

echo "Successfully ran the setup scripts"

