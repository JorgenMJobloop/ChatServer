echo "Running the setup script.."

sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0


sudo mv CreateKeyPair.py /ChatServer/
sudo mv ShellScript.py /chatServer/

echo "Setup complete.."
