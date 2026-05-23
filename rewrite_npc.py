import sys

with open('SNAP/Assets/_Game/Scripts/NPC/NPCController.cs', 'r') as f:
    lines = f.readlines()

new_lines = []
skip = False

for i, line in enumerate(lines):
    if "private IEnumerator DelayedWanderStart" in line:
        skip = True
        
    if "private IEnumerator ReactRoutine()" in line:
        skip = False # Keep ReactRoutine
        
    if "private IEnumerator SocialProximityScanRoutine()" in line:
        skip = True
        
    if "// ─── External Events ──────────────────────────────────────────────" in line:
        skip = False

    # Also clean up StartCoroutine(_wanderCoroutine) inside OnPhaseChanged and others
    if "_wanderCoroutine" in line:
        continue # Remove any line doing stuff with _wanderCoroutine
        
    if "_socializeCoroutine" in line:
        continue
        
    if "private Coroutine _wanderCoroutine;" in line or "private Coroutine _socializeCoroutine;" in line:
        continue

    # Add HandleMovement to Update
    if "UpdateAnimation();" in line and "private void Update()" in "".join(lines[max(0, i-15):i]):
        new_lines.append("            HandleMovement();\n")

    # Clean up Awake ActionPlanner
    if "gameObject.AddComponent<UtilityAI.SearchAction>();" in line:
        new_lines.append(line)
        new_lines.append("            gameObject.AddComponent<UtilityAI.TravelAction>();\n")
        new_lines.append("            gameObject.AddComponent<UtilityAI.ChillAloneAction>();\n")
        continue
        
    # Clean up HandleMovement
    if "if (currentState == NPCState.Wandering ||" in line and "HandleMovement()" in "".join(lines[max(0, i-10):i]):
        new_lines.append("            if (currentState == NPCState.WalkingToBoard ||\n")
        continue
        
    if "currentState == NPCState.WalkingToBoard ||" in line and "HandleMovement()" in "".join(lines[max(0, i-15):i]) and not "if" in line:
        # It's inside the if statement
        continue
        
    if "float speed = currentState == NPCState.WalkingToBoard || currentState == NPCState.Fleeing ? runSpeed : moveSpeed;" in line:
        new_lines.append("                float speed = currentState == NPCState.WalkingToBoard ? runSpeed : moveSpeed;\n")
        continue

    # Remove wandering conditions in HandleMovement
    if "else if (currentState == NPCState.Wandering && dist < 1.0f)" in line:
        skip = True
        continue
    if "else if (currentState == NPCState.Fleeing && dist < 1.5f)" in line and skip:
        continue
    if "else if (currentState == NPCState.Sitting && dist < 0.5f)" in line and skip:
        skip = False
        # we still want sitting
        new_lines.append(line)
        continue

    if not skip:
        new_lines.append(line)

with open('SNAP/Assets/_Game/Scripts/NPC/NPCController.cs', 'w') as f:
    f.writelines(new_lines)
