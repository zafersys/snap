import sys

with open('SNAP/Assets/_Game/Scripts/NPC/NPCController.cs', 'r') as f:
    lines = f.readlines()

new_lines = []
skip = False
i = 0
while i < len(lines):
    line = lines[i]

    # Add components in Awake
    if "gameObject.AddComponent<UtilityAI.SearchAction>();" in line:
        new_lines.append(line)
        new_lines.append("            gameObject.AddComponent<UtilityAI.TravelAction>();\n")
        new_lines.append("            gameObject.AddComponent<UtilityAI.ChillAloneAction>();\n")
        new_lines.append("            gameObject.AddComponent<UtilityAI.ReadNewsAction>();\n")
        new_lines.append("            gameObject.AddComponent<UtilityAI.GoHomeAction>();\n")
        new_lines.append("            gameObject.AddComponent<UtilityAI.FleePlayerAction>();\n")
        i += 1
        # Skip duplicate TravelAction/ChillAloneAction if they exist
        while i < len(lines) and ("gameObject.AddComponent<UtilityAI.TravelAction>();" in lines[i] or "gameObject.AddComponent<UtilityAI.ChillAloneAction>();" in lines[i]):
            i += 1
        continue

    # Remove HandleMovement from Update
    if "HandleMovement();" in line:
        i += 1
        continue

    # Remove HandleMovement completely
    if "private void HandleMovement()" in line:
        # Skip until the next // ─── State Machine ───
        while i < len(lines) and "// ─── State Machine" not in lines[i]:
            i += 1
        continue

    # Replace OnPhaseChanged completely
    if "public void OnPhaseChanged(DayPhase newPhase)" in line:
        new_lines.append("        public void OnPhaseChanged(DayPhase newPhase)\n")
        new_lines.append("        {\n")
        new_lines.append("            switch (newPhase)\n")
        new_lines.append("            {\n")
        new_lines.append("                case DayPhase.Morning:\n")
        new_lines.append("                    _hasReadTodayNews = false;\n")
        new_lines.append("                    if (_occupiedBench != null)\n")
        new_lines.append("                    {\n")
        new_lines.append("                        _occupiedBench.Vacate();\n")
        new_lines.append("                        _occupiedBench = null;\n")
        new_lines.append("                    }\n")
        new_lines.append("                    if (_pendingNews != null && boardPosition != null)\n")
        new_lines.append("                    {\n")
        new_lines.append("                        _needs.HasPendingNews = true;\n")
        new_lines.append("                        if (_actionPlanner != null) _actionPlanner.ForceReevaluate();\n")
        new_lines.append("                    }\n")
        new_lines.append("                    _needs.IsNightTime = false;\n")
        new_lines.append("                    break;\n")
        new_lines.append("\n")
        new_lines.append("                case DayPhase.Midday:\n")
        new_lines.append("                    // Observational dynamics (pantomime / gossip) could go here\n")
        new_lines.append("                    // but movement is handled purely by Utility AI.\n")
        new_lines.append("                    break;\n")
        new_lines.append("\n")
        new_lines.append("                case DayPhase.Afternoon:\n")
        new_lines.append("                    // Finding a bench is now a Utility Action (TODO: SitAction)\n")
        new_lines.append("                    break;\n")
        new_lines.append("\n")
        new_lines.append("                case DayPhase.Night:\n")
        new_lines.append("                    _needs.IsNightTime = true;\n")
        new_lines.append("                    if (_actionPlanner != null) _actionPlanner.ForceReevaluate();\n")
        new_lines.append("                    break;\n")
        new_lines.append("            }\n")
        new_lines.append("        }\n")

        # Skip until the end of the original OnPhaseChanged and GoToBoardDelayed
        while i < len(lines) and "public void ReceiveNews" not in lines[i]:
            i += 1
        continue

    # Update PlayerProximityCheckRoutine Hostile logic
    if "Vector3 dirAway = (transform.position - player.transform.position).normalized;" in line:
        new_lines.append("                        _needs.IsPlayerHostile = true;\n")
        new_lines.append("                        _needs.IsPlayerNearby = true;\n")
        new_lines.append("                        if (_actionPlanner != null) _actionPlanner.ForceReevaluate();\n")
        
        # skip lines until we hit the "}" of the else if block
        while i < len(lines) and "}" not in lines[i].strip():
            i += 1
        new_lines.append("                    }\n") # Add back the closing brace
        i += 1
        continue

    new_lines.append(line)
    i += 1

with open('SNAP/Assets/_Game/Scripts/NPC/NPCController.cs', 'w') as f:
    f.writelines(new_lines)
