using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;

public class LiveInterpreter : MonoBehaviour{
	private robotD robot;
	private robotD robot2;

	[SerializeField]
	private bool secureMode = false; // Toggle in Unity Inspector
	private int numRobots = 1;

	private bool play;

	// command-following variable
	private int currentLineNum;
	private float nextCommandTime;
	List<string> function;
	List<int> functionStarts;
	List<bool> isFunc;
	List<int> lineNumbers;
	List<int> loopsLeft;

  [SerializeField]
  private int threeStarsLine = 1;

  [SerializeField]
  private int twoStarsLine = 2;

  [SerializeField]
  private int oneStarLine = 3;

	// statistic trackers
	private int operationCount;
	private int lineCount;

	// to visualize code walkthrough
	[SerializeField]
	private ScriptText scriptText = null;

	// command/fileRead variables
	private string[] script;
	private bool[] line;

	void Start(){
    // stats
    operationCount = 0;
    lineCount = 0;

    // get reference to robot script
    robot = GameObject.Find("robot").GetComponent<robotD>();

    if(numRobots == 2){
        robot2 = GameObject.Find("robot2").GetComponent<robotD>();
    }

    // initial text file creation
    StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/script.txt", false);
    writer.Write("");
    writer.Close();

    // initial text file read
    readFile();

    reset();
}
  
 // called on play button press
public void start(){
    string[] script = readFile();
    
    // VALIDATE BEFORE RUNNING
    if(!validateScript(script)){
        // Validation failed - don't run the code
        return;
    }
    
    // Validation passed - proceed with execution
    operationCount = 0;
    scriptText.setRunningLineNum(currentLineNum);
    
    play = true;
    saveScriptToFile(script);
}

	public void reset(){
		play = false;

		currentLineNum = -1;
		nextCommandTime = Time.time;

		isFunc = new List<bool>();
		lineNumbers = new List<int>();
		loopsLeft = new List<int>();

		function = new List<string>();
		functionStarts = new List<int>();

    scriptText.setRunningLineNum(-1);
	}

	// Validates the script before execution
private bool validateScript(string[] script){
    Stack<int> bracketStack = new Stack<int>();
    bool hasValidCommands = false;
    
    for(int i = 0; i < script.Length; i++){
        string line = StringMethods.killWhiteSpace(script[i]);
        
        // Skip empty lines
        if(line == ""){
            continue;
        }
        
        // Check for opening brackets
        if(line.Length > 0 && line[line.Length - 1] == '{'){
            bracketStack.Push(i);
            hasValidCommands = true;
        }
        // Check for closing brackets
        else if(line == "}"){
            if(bracketStack.Count == 0){
                Debug.LogError("ERROR: Closing bracket '}' on line " + (i+1) + " has no matching opening bracket!");
                ShowErrorMessage("Syntax Error: Unmatched closing bracket '}' on line " + (i+1));
                return false;
            }
            bracketStack.Pop();
        }
        // Check for valid commands
        else if(line == "moveForward();" || line == "turnRight();" || line == "turnLeft();" || 
                line == "pickUp();" || line == "putDown();" || line == "throwItem();"){
            hasValidCommands = true;
        }
        // Check for function calls
        else if(line.Length >= 7 && line.Substring(0, 4) == "func" && line.Substring(line.Length - 3) == "();"){
            hasValidCommands = true;
        }
        // Check for do loops
        else if(line.Length >= 2 && line.Substring(0, 2) == "do"){
            hasValidCommands = true;
        }
        // Check for for loops
        else if(line.Length >= 3 && line.Substring(0, 3) == "for"){
            hasValidCommands = true;
        }
        // Unknown/invalid command
        else{
            Debug.LogError("ERROR: Unrecognized command on line " + (i+1) + ": " + line);
            ShowErrorMessage("Syntax Error: Invalid command on line " + (i+1) + ": '" + line + "'");
            return false;
        }
    }
    
    // Check for unmatched opening brackets
    if(bracketStack.Count > 0){
        int unmatchedLine = bracketStack.Pop();
        Debug.LogError("ERROR: Opening bracket '{' on line " + (unmatchedLine+1) + " has no matching closing bracket!");
        ShowErrorMessage("Syntax Error: Unmatched opening bracket '{' on line " + (unmatchedLine+1));
        return false;
    }
    
    // Check if script has any commands at all
    if(!hasValidCommands){
        Debug.LogError("ERROR: Script is empty or has no valid commands!");
        ShowErrorMessage("Error: Your program needs at least one command!");
        return false;
    }
    
    return true;
}

// Display error message to user (you'll need to create a UI element for this)
// Display error message to user
private void ShowErrorMessage(string message){
    Debug.Log("VALIDATION ERROR: " + message);
    
    // Show the popup if it exists
    if(ErrorPopupManager.Instance != null){
        ErrorPopupManager.Instance.ShowError(message);
    }
}

	void FixedUpdate(){
		if(play){
			runCode(script);
		}
	}


	public void stop(){
		play = false;
    scriptText.setRunningLineNum(-1);
	}

	// reads "script.txt" and puts each line into String array 'script'
	// - this method should be rewritten eventually
	// - possibly rewrite with List type for 'script'
	public string[] readFile(){
		lineCount = 0;
		int counter = 0;
		string curLine;

		System.IO.StreamReader file;
		file = new System.IO.StreamReader(Application.persistentDataPath + "/script.txt");
		while((curLine = file.ReadLine()) != null){
			if(curLine != "")
				lineCount++;
    	counter++;
		}
		file.Close();
		script = new string[counter];

		counter = 0;
		file = new System.IO.StreamReader(Application.persistentDataPath + "/script.txt");
		while((curLine = file.ReadLine()) != null){
    	script[counter] = curLine;
    	counter++;
		}
    //Debug.Log("here");
		file.Close();

		return script;
  }

	public void onCodeChange(){
    string[] script = readFile();
    saveScriptToFile(script);
	}

	void saveScriptToFile(string[] script){
    // copy script to level script, to save for next time.
    Scene scene = SceneManager.GetActiveScene();
    System.IO.StreamWriter writeFile;
    writeFile = new System.IO.StreamWriter(Application.persistentDataPath + "/" + scene.name + "Script.txt");
    foreach (string line in script)
    {
      //Debug.Log("writing file: " + line);
      writeFile.WriteLine(line);
    }
    writeFile.Flush();
    writeFile.Close();
    //File.Copy(Application.persistentDataPath + "/script.txt", Application.persistentDataPath + "/" + scene.name + "Script.txt");
  }

	void runCode(string[] code){
		if(currentLineNum == -1){
			//currentLineNum = 0;
    	scriptText.setRunningLineNum(0);
		}

		if(Time.time >= (nextCommandTime - 0.05f)){
			robot.neutralAnim();
			if(numRobots == 2){
				robot2.neutralAnim();
			}
		}

		if(currentLineNum < script.Length && Time.time >= nextCommandTime){
			advanceCode();
		}else if(currentLineNum >= script.Length && Time.time >= nextCommandTime){
			currentLineNum = -1;
      
			stop();
		}
	}

	void advanceCode(){
		if(currentLineNum == script.Length - 1){
			stop();
      scriptText.setRunningLineNum(-1);
			return;
		}
	
		nextCommandTime = Time.time + 0.5f;
		//Debug.Log("currentLineNum = " + currentLineNum + ", script.Length = " + script.Length);
		//Debug.Log("line " + currentLineNum + ": " + StringMethods.killWhiteSpace(script[currentLineNum]));
		if(currentLineNum < script.Length - 1){
			currentLineNum++;
			//Debug.Log("run line " + currentLineNum);
			runLine(StringMethods.killWhiteSpace(script[currentLineNum]));
		}
		if(currentLineNum < script.Length)
			scriptText.setRunningLineNum(currentLineNum);
	}

	void runLine(string command){
	if(command == ""){
		advanceCode();
		/*if(currentLineNum < script.Length - 1)
		scriptText.setRunningLineNum(currentLineNum + 1);
		nextCommandTime = Time.time;*/
	} else if(command == "moveForward();"){
		// SECURE MODE: Check if movement is safe
		if(secureMode && !robot.canMoveForward()){
			Debug.LogError("SECURE MODE VIOLATION: Cannot move forward - obstacle or edge detected!");
			ShowErrorMessage("Secure Mode: Blocked unsafe move forward!");
			stop();
			return;
		}
		
		operationCount++;
		robot.moveForward();
		///////////////////////
		if(numRobots == 2){
			robot2.moveForward();
		}
		//////////////////////
	} else if(command == "turnRight();"){
		operationCount++;
		robot.turnRight();
		///////////////////////
		if(numRobots == 2){
			robot2.turnRight();
		}
		//////////////////////
		nextCommandTime = Time.time + 0.4f;
	} else if(command == "turnLeft();"){
		operationCount++;
		robot.turnLeft();
		///////////////////////
		if(numRobots == 2){
			robot2.turnLeft();
		}
		//////////////////////
		nextCommandTime = Time.time + 0.4f;
	} else if(command == "pickUp();"){
		// SECURE MODE: Check if there's a package to pick up
		if(secureMode && !robot.canPickUp()){
			Debug.LogError("SECURE MODE VIOLATION: Cannot pick up - no package in range!");
			ShowErrorMessage("Secure Mode: No package to pick up!");
			stop();
			return;
		}
		
		operationCount++;
		//nextCommandTime = Time.time + 0.4f;
		robot.pickUp();
		///////////////////////
		if(numRobots == 2){
			robot2.pickUp();
		}
		//////////////////////
	} else if(command == "putDown();"){
		// SECURE MODE: Check if robot is holding something
		if(secureMode && !robot.isHoldingPackage()){
			Debug.LogError("SECURE MODE VIOLATION: Cannot put down - not holding anything!");
			ShowErrorMessage("Secure Mode: Not holding a package!");
			stop();
			return;
		}
		
		operationCount++;
		//nextCommandTime = Time.time + 0.4f;
		robot.putDown();
		///////////////////////
		if(numRobots == 2){
			robot2.putDown();
		}
		//////////////////////
	} else if(command == "throwItem();"){
		// SECURE MODE: Check if robot is holding something
		if(secureMode && !robot.isHoldingPackage()){
			Debug.LogError("SECURE MODE VIOLATION: Cannot throw - not holding anything!");
			ShowErrorMessage("Secure Mode: Not holding a package!");
			stop();
			return;
		}
		
		operationCount++;
		//nextCommandTime = Time.time + 0.4f;
		robot.throwItem();
		///////////////////////
		if(numRobots == 2){
			robot2.throwItem();
		}
		//////////////////////
	} else if(StringMethods.cutString(command, 0, 2) == "do"){
		isFunc.Add(false);
		lineNumbers.Add(currentLineNum);
		loopsLeft.Add((int)char.GetNumericValue(command[2]) - 1);
		nextCommandTime = Time.time + 0.25f;
	} else if(StringMethods.cutString(command, 0, 3) == "for"){
		isFunc.Add(false);
		lineNumbers.Add(currentLineNum);
		loopsLeft.Add((int)char.GetNumericValue(command[4]) - 1);
		nextCommandTime = Time.time + 0.25f;
	} else if(StringMethods.killWhiteSpace(command) == "}"){
		if(isFunc.Count > 0 && isFunc[isFunc.Count - 1]){ //PROBLEM HERE, possibly solved
			currentLineNum = lineNumbers[lineNumbers.Count-1];
			scriptText.setRunningLineNum(currentLineNum);
			
			isFunc.RemoveAt(isFunc.Count - 1);
			lineNumbers.RemoveAt(lineNumbers.Count - 1);
			loopsLeft.RemoveAt(loopsLeft.Count - 1);
		} else{
			if(loopsLeft[loopsLeft.Count - 1] > 0){
				loopsLeft[loopsLeft.Count - 1]--;
				currentLineNum = lineNumbers[lineNumbers.Count - 1];
				scriptText.setRunningLineNum(currentLineNum);
				
			} else {
				isFunc.RemoveAt(isFunc.Count - 1);
				lineNumbers.RemoveAt(lineNumbers.Count - 1);
				loopsLeft.RemoveAt(loopsLeft.Count - 1);
			}
		}
		nextCommandTime = Time.time + 0.25f;
	} else if(isFunctionCall(command)){
		isFunc.Add(true);
		lineNumbers.Add(currentLineNum);
		loopsLeft.Add(0);
		funcJump(StringMethods.funcName(command));
		nextCommandTime = Time.time + 0.25f;
	} else if(isFunctionDeclaration(command)){
		//Debug.Log("function declaration");
		function.Add(StringMethods.funcName(command));
		functionStarts.Add(currentLineNum);
		// this next line could be a problem. TODO: test it
		jumpToCloseBracket();

		scriptText.setRunningLineNum(currentLineNum);
		
		nextCommandTime = Time.time + 0.25f;
	} else{
		Debug.Log("Error: string not recognized.");
		nextCommandTime = Time.time;
	}
}

	// script line jumping
	/*void findForLoopStart(){ //Not currently in use
		while(StringMethods.cutString(StringMethods.killWhiteSpace(script[currentLineNum]), 0, 3) != "for"){
			currentLineNum--;
			textArea.RunningLine = currentLineNum;
		}
	}*/

	void jumpToCloseBracket(){
		int numOpenBrackets = 1;
		while(numOpenBrackets > 0){
			currentLineNum++;
			if(script[currentLineNum].Length > 0 && script[currentLineNum][script[currentLineNum].Length - 1] == '{'){
				numOpenBrackets++;
			}
			if(script[currentLineNum] == "}"){
				numOpenBrackets--;
			}
		}
	}

	void funcJump(string func){
		for(int i = 0; i < function.Count; i++){
			if(func == function[i]){
				currentLineNum = functionStarts[i];
				scriptText.setRunningLineNum(currentLineNum);
        
				return;
			}
		}
	}

	// boolean functions
	bool isFunctionCall(string line){
		for(int i = 0; i < function.Count; i++){
			if(StringMethods.funcName(line) == function[i]){
				return true;
			}
		}
		return false;
	}

	bool isFunctionDeclaration(string line){
		return line[line.Length - 1] == '{';
	}

	public String getStats(){
		return "You completed the level with:\n" +
       "<color=#00FFFF>" + lineCount + "</color> lines\n" +
       "<color=#00FFFF>" + operationCount +"</color> operations";
	}

	public int getNumStars(){
		if(lineCount <= threeStarsLine){
				return 3;
		} else if(lineCount <= twoStarsLine){
				return 2;
		} else if(lineCount <= oneStarLine){
				return 1;
		}
		return -1;
  }
}
