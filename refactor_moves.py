import os
import glob

# Script to refactor Action scripts to use MoveAction base class and simplify execution loops

actions_path = 'SNAP/Assets/_Game/Scripts/NPC/UtilityAI/*.cs'
for file_path in glob.glob(actions_path):
    if "MoveAction.cs" in file_path or "NPCActionPlanner.cs" in file_path or "NPCNeeds.cs" in file_path or "NPCAction.cs" in file_path:
        continue

    with open(file_path, 'r') as f:
        content = f.read()

    # Change inheritance
    content = content.replace("public class " + os.path.basename(file_path).split('.')[0] + " : NPCAction", 
                              "public class " + os.path.basename(file_path).split('.')[0] + " : MoveAction")

    # Remove redundant NavMeshAgent and Interrupt() since MoveAction has them
    # Wait, some have targetNPC or something, but _agent and _isExecuting and Interrupt are in MoveAction
    if "private NavMeshAgent _agent;" in content:
        content = content.replace("private NavMeshAgent _agent;\n", "")
    if "private bool _isExecuting = false;" in content:
        content = content.replace("private bool _isExecuting = false;\n", "")
        
    # We will just write specialized replacement logic for the main actions
    
    # TravelAction
    if "TravelAction.cs" in file_path:
        content = content.replace("float utility = BaseUtility + Random.Range(0f, 15f);", "float utility = BaseUtility + 5f; // Random removed to prevent oscillation")
        
        # Replace the while loop with MoveToTarget
        loop_start = content.find("if (_agent.isOnNavMesh")
        loop_end = content.find("if (!_isExecuting) yield break;", loop_start)
        if loop_start != -1 and loop_end != -1:
            loop_end += len("if (!_isExecuting) yield break;\n")
            replacement = """
            yield return MoveToTarget(outsideTarget, NPCState.Traveling, 1.2f, 8f);
            if (!_isExecuting) yield break;
            
            Controller.TriggerReaction("[TRAVEL]", new Color(0.4f, 0.8f, 0.4f));
            if (JournalManager.Instance != null && Random.value < 0.4f)
            {
                JournalManager.Instance.AddObservation($"[{Controller.NpcName}] [TRAVEL] -> [OUTSIDE-VILLAGE] \\o/", new Color(0.4f, 0.8f, 0.4f));
            }
"""
            content = content[:loop_start] + replacement + content[loop_end:]

    # WanderAction
    if "WanderAction.cs" in file_path:
        loop_start = content.find("NavMeshHit hit;")
        loop_end = content.find("// Satisfy boredom by arriving at a new location", loop_start)
        if loop_start != -1 and loop_end != -1:
            replacement = """
                yield return MoveToTarget(randomDirection, NPCState.Wandering, 1.2f, 5f);
                if (!_isExecuting) yield break;
                
"""
            content = content[:loop_start] + replacement + content[loop_end:]

    # GoHomeAction
    if "GoHomeAction.cs" in file_path:
        loop_start = content.find("if (_agent.isOnNavMesh")
        loop_end = content.find("// Arrived home", loop_start)
        if loop_start != -1 and loop_end != -1:
            replacement = """
            yield return MoveToTarget(Controller.homePosition, NPCState.WalkingHome, 1.2f, 8f);
            if (!_isExecuting) yield break;
            
"""
            content = content[:loop_start] + replacement + content[loop_end:]

    # ChillAloneAction
    if "ChillAloneAction.cs" in file_path:
        loop_start = content.find("NavMeshHit hit;")
        loop_end = content.find("if (!_isExecuting) yield break;", loop_start)
        if loop_start != -1 and loop_end != -1:
            loop_end += len("if (!_isExecuting) yield break;\n")
            replacement = """
            yield return MoveToTarget(scenicSpot, NPCState.Wandering, 0.8f, 5f);
            if (!_isExecuting) yield break;
"""
            content = content[:loop_start] + replacement + content[loop_end:]

    # ReadNewsAction
    if "ReadNewsAction.cs" in file_path:
        loop_start = content.find("if (_agent.isOnNavMesh")
        loop_end = content.find("// Arrived at board", loop_start)
        if loop_start != -1 and loop_end != -1:
            replacement = """
            yield return MoveToTarget(Controller.boardPosition.position, NPCState.WalkingToBoard, 1.2f, 5f);
            if (!_isExecuting) yield break;
            
"""
            content = content[:loop_start] + replacement + content[loop_end:]

    # Remove Interrupt entirely since it's inherited
    interrupt_idx = content.find("public override void Interrupt()")
    if interrupt_idx != -1:
        # find the end of the method
        end_idx = content.find("}", content.find("}", interrupt_idx) + 1) + 1
        content = content[:interrupt_idx] + content[end_idx:]

    with open(file_path, 'w') as f:
        f.write(content)
        
print("Refactored MoveActions")
