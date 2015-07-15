﻿private var action : String; 
private var player : String; 
private var battle = true; 
private var displayGui = false;

function Start () { 
	while (battle) { 
		yield PlayerChoice(); 
		yield BattlePhase(); 
		yield TurnEnd();

     	yield EnemyChoice();
     	yield BattlePhase();
     	yield TurnEnd();
 	}
}

function PlayerChoice() { 
	print("human makes a decision"); 
	player = "human"; 
	displayGui = true;

 	while (displayGui)
    	yield;        
}

function BattlePhase() { 
	print("battle rages on"); 
	yield new WaitForSeconds(1); 
	print(player + " " + action); 
	yield new WaitForSeconds(1); 
}

function EnemyChoice() { 
	
	print("enemy makes a decision"); 
	GetComponent(BattleReactionsController).ReactionHornedLion();
	yield new WaitForSeconds(1); 
	player = "enemy"; 
	action = "attacks"; 
}

function TurnEnd() { 
	print("turn ends"); 
	yield new WaitForSeconds(1); 
}

function OnGUI() { 
	if (!displayGui){
//		GameObject.FindGameObjectWithTag("UserAttackUI").SetActive(false);
	}else{
//		GameObject.FindGameObjectWithTag("UserAttackUI").SetActive(true);
	}

// 	GUI.Box (Rect (10,10,100,90), "Battle Menu");
// 
//	 if (GUI.Button (Rect (20,40,80,20), "Attack"))
//	 {
//	     action = "attacks";
//	     displayGui = false;
//	 } 
//	 
//	 if (GUI.Button (Rect (20,70,80,20), "Defend"))
//	 {
//	     action = "defends";
//	     displayGui = false;
//	 }
}

function HideGUI()
{
	displayGui = false;
}