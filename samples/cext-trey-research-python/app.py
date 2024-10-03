import os
import subprocess
import shutil
import sys
from db_setup.azure_table_setup import main as setup_azure_tables
from config import Settings

CONFIG = Settings()

# Setup Azure Tables
def setup_tables():
    connection_string = CONFIG.STORAGE_ACCOUNT_CONNECTION_STRING
    setup_azure_tables(connection_string, reset=True)  # Adjust `reset` as needed

# Run Azure Functions
def run_azure_functions():
    # Check if 'func' is in PATH
    func_path = shutil.which("func")
    
    if func_path:
        # Change directory to where your Azure Functions app is located
        os.chdir('functions')  # Adjust 'functions' if needed to match your directory structure

        try:
            # Run the Azure Functions host using the subprocess module to call 'func start'
            subprocess.run([func_path, "start"], shell=True, check=True)
        except subprocess.CalledProcessError as e:
            print(f"Error occurred while running Azure Functions: {e}")
            sys.exit(1)
    else:
        print("Azure Functions Core Tools (func) not found in your system PATH.")
        print("Please install it or ensure it's available in the PATH.")
        sys.exit(1)

if __name__ == "__main__":
    # Step 1: Setup Azure Tables
    setup_tables()

    # Step 2: Start Azure Functions
    run_azure_functions()