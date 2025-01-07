import re

# Path to the build.txt file
file_path = "build.txt"

# Read the file content
with open(file_path, "r") as file:
    content = file.readlines()

# Process each line to find and increment the version
for i, line in enumerate(content):
    match = re.match(r"version = (\d+)\.(\d+)\.(\d+)", line)
    if match:
        major, minor, patch = map(int, match.groups())
        patch += 1  # Increment the patch version
        content[i] = f"version = {major}.{minor}.{patch}\n"
        break

# Write the updated content back to the file
with open(file_path, "w") as file:
    file.writelines(content)
