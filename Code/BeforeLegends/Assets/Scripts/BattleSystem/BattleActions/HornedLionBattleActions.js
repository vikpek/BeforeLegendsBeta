﻿
//function executeAction(battle : BattleController){
//	switch(battle.enemyAction){ // always remember to add new actions BELOW and in BattleController.js
//		case Action.ATTACK :
//			attack(battle);
//			break;
//		case Action.FINALATTACK :
//			attackOpponentFinal(battle);
//			break;
//		case Action.HEAL :
//			healSelfOpponent(battle);
//			break;	
//	}
//}

//function attack(battle : BattleController){ // this is the basic attack
//	battle.playerData.hitPoints -= battle.enemyData.calcDamage(battle.playerData, 1)[0]; 
//	battle.animateEnemy(Anims.ATTACK); 
//	battle.animatePlayer(Anims.HURT);
//	AudioMaster.instance.audioSource.PlayOneShot(battle.enemyWorldObject.GetComponent(MapObjectCarrier).audioObject.attack);
//}

//function attackOpponentFinal(battle : BattleController){ // this is a very powerful attack, deals 4x basic damage
//	battle.enemyData.actionPoints--;
//	battle.playerData.hitPoints -= battle.enemyData.calcDamage(battle.playerData, 4)[0];
//	battle.animateEnemy(Anims.SPATTACK); 
//	battle.animatePlayer(Anims.HURT);
//	AudioMaster.instance.audioSource.PlayOneShot(battle.enemyWorldObject.GetComponent(MapObjectCarrier).audioObject.spattack); 
//}

//function healSelfOpponent(battle : BattleController){ // this power allows the Lion to heal itself
//	battle.enemyParticles.heal.Play();
//	battle.enemyData.actionPoints--;
//	battle.enemyData.hitPoints += battle.enemyData.maxHitPoints*0.05;
//	if (battle.enemyData.hitPoints > battle.enemyData.maxHitPoints){
//		battle.enemyData.hitPoints = battle.enemyData.maxHitPoints;
//	}
//}


