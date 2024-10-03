import logging

class HttpError(Exception):
    """Exception class for HTTP errors."""
    def __init__(self, status: int, message: str):
        super().__init__(message)
        self.status = status

def clean_up_parameter(name: str, value: str) -> str:
    """Cleans up common issues with parameters."""
    val = value.lower()
    
    if "trey" in val or "research" in val:
        new_val = val.replace("trey", "").replace("research", "").strip()
        logging.warning(f"   ❗ Plugin name detected in the {name} parameter '{val}'; replacing with '{new_val}'.")
        val = new_val
    
    if val == "<user_name>":
        logging.warning(f"   ❗ Invalid name '{val}'; replacing with 'avery'.")
        val = "avery"
    
    if name == "role" and val == "consultant":
        logging.warning(f"   ❗ Invalid role name '{val}'; replacing with ''.")
        val = ""
    
    if val == "null":
        logging.warning(f"   ❗ Invalid value '{val}'; replacing with ''.")
        val = ""
    
    return val