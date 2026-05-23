import os
import glob

# 1. Fix NPCController to make EnterState public
npc_controller_path = 'SNAP/Assets/_Game/Scripts/NPC/NPCController.cs'
with open(npc_controller_path, 'r') as f:
    npc_lines = f.readlines()
    
for i, line in enumerate(npc_lines):
    if "private void EnterState(NPCState newState)" in line:
        npc_lines[i] = line.replace("private void", "public void")
    
    # Also fix UpdateAnimation to use the target agent speed!
    if "if (_agent != null && _agent.isOnNavMesh)" in line and "UpdateAnimation" in "".join(npc_lines[max(0, i-20):i]):
        # I want to revert my previous broken UpdateAnimation and make it use the speed variable based on state!
        pass

# Let's cleanly rewrite UpdateAnimation inside NPCController.cs
update_anim_start = -1
update_anim_end = -1
for i, line in enumerate(npc_lines):
    if "private void UpdateAnimation()" in line:
        update_anim_start = i
    if update_anim_start != -1 and "private void UpdateBob()" in line:
        update_anim_end = i
        break

if update_anim_start != -1 and update_anim_end != -1:
    new_anim = [
        "        private void UpdateAnimation()\n",
        "        {\n",
        "            if (_animator == null) return;\n",
        "            \n",
        "            bool isWalking = currentState == NPCState.Wandering ||\n",
        "                             currentState == NPCState.WalkingToBoard ||\n",
        "                             currentState == NPCState.WalkingHome ||\n",
        "                             currentState == NPCState.Fleeing ||\n",
        "                             currentState == NPCState.Socializing;\n",
        "            \n",
        "            float targetSpeed = 0f;\n",
        "            if (isWalking && _agent != null)\n",
        "            {\n",
        "                targetSpeed = _agent.speed;\n",
        "            }\n",
        "            \n",
        "            _animator.SetFloat(\"Speed\", targetSpeed);\n",
        "            _animator.SetInteger(\"Emotion\", (int)currentEmotion);\n",
        "        }\n\n"
    ]
    npc_lines = npc_lines[:update_anim_start] + new_anim + npc_lines[update_anim_end:]

with open(npc_controller_path, 'w') as f:
    f.writelines(npc_lines)


# 2. Fix all Action scripts to use EnterState
path = 'SNAP/Assets/_Game/Scripts/NPC/UtilityAI/*.cs'
for file_path in glob.glob(path):
    with open(file_path, 'r') as f:
        lines = f.readlines()
        
    modified = False
    new_lines = []
    
    for i, line in enumerate(lines):
        if "Controller.currentState =" in line:
            # e.g. Controller.currentState = NPCState.Wandering; -> Controller.EnterState(NPCState.Wandering);
            # extracting the state name
            parts = line.split("=")
            if len(parts) == 2:
                state = parts[1].strip().strip(';')
                indent = line[:len(line) - len(line.lstrip())]
                new_lines.append(f"{indent}Controller.EnterState({state});\n")
                modified = True
                continue
                
        if "Controller.ProcessReadNews();" in line:
            modified = True
            continue # delete this line because EnterState(NPCState.Reading) does it now
            
        if "Controller.transform.rotation = Quaternion.LookRotation(Controller.boardPosition.position - Controller.transform.position);" in line and "ReadNewsAction.cs" in file_path:
            modified = True
            continue # delete because EnterState handles it
            
        new_lines.append(line)
                
    if modified:
        with open(file_path, 'w') as f:
            f.writelines(new_lines)
        print(f"Updated {file_path}")
