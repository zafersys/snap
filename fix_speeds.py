import os
import glob

# Path to the UtilityAI actions
path = 'SNAP/Assets/_Game/Scripts/NPC/UtilityAI/*.cs'

for file_path in glob.glob(path):
    with open(file_path, 'r') as f:
        lines = f.readlines()
        
    modified = False
    new_lines = []
    
    for i, line in enumerate(lines):
        new_lines.append(line)
        if "_agent.SetDestination" in line:
            # Check if the next line already sets speed
            if i + 1 < len(lines) and "_agent.speed" not in lines[i+1]:
                # We need to inject speed setting. Use the same indentation.
                indent = line[:len(line) - len(line.lstrip())]
                new_lines.append(indent + "_agent.speed = Controller.moveSpeed;\n")
                modified = True
                
    if modified:
        with open(file_path, 'w') as f:
            f.writelines(new_lines)
        print(f"Updated {file_path}")
