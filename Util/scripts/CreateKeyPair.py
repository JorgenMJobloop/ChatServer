import ssl
import subprocess
import pathlib

def create_new_pem():
    args = ["openssl genrsa -out keypair.pem 2048", "openssl rsa -in keypair.pem -pubout -out publickey.crt"]
    get_user_input = str(input("Run the openssl commands to generate a new key-pair. y/N? "))
    if get_user_input.lower() == "y":
        subprocess.Popen(args[0]) # generate private key
        subprocess.Popen(args[1]) # generate the public key
        print(f"Generated keys are located at: {pathlib.Path().cwd}")
        print("Listing all files and directories:\n")
        print(subprocess.Popen(["ls", "-l", "-a"])) # List out directories to verify that the certificates are created and stored in the correct path
    else:
        print("Not generating new key pairs..")    


if __name__ == "__main__":
    create_new_pem()