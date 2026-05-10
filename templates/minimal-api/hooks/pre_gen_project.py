import re
import sys

PROJECT_NAME = "{{ cookiecutter.project_name }}"

# Regex for kebab-case (lowercase, numbers, and hyphens only)
KEBAB_REGEX = r"^[a-z0-9]+(-[a-z0-9]+)*$"

if not re.match(KEBAB_REGEX, PROJECT_NAME):
    print(f"ERROR: '{PROJECT_NAME}' is not a valid kebab-case name.")
    print("Please use lowercase letters, numbers, and hyphens (e.g., 'my-cool-project').")
    
    # Exit with a non-zero status to stop cookiecutter from generating files
    sys.exit(1)
