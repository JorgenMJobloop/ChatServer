#!/usr/bin/bash
echo "Running the setup script.."

sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0

read -rsn1 -p "Press any key to continue..."

sleep 5 # sleep for 5 seconds

sudo mv CreateKeyPair.py /ChatServer/

sudo mv ShellScript.py /chatServer/

echo "Python scripts moved to current working directory"

sleep 5

read -rsn1 -p "Press any key to continue..."

echo "Generating a new keypair"

python3 CreateKeyPair.py

echo "Running the ShellScript python module"

python3 ShellScript.py

