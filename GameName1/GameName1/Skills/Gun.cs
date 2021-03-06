﻿using GameName1.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameName1.Skills
{
	class Gun : Equipable
	{

		private int recharge_time;
		private int recharged;
		private int damage;
		private Vector2 bulletSpeed; 

		public Gun(int damage, int recharge_time, Vector2 bulletSpeed)
		{
			this.recharge_time = recharge_time;
			this.recharged = recharge_time;
			this.damage = damage;

			this.bulletSpeed = bulletSpeed; 
		}

		public void OnUnequip(Seizonsha game, GameEntity entity)
		{

		}

		public void OnEquip(Seizonsha game, GameEntity entity)
		{

		}

		public void Use(Seizonsha game, GameEntity entity)
		{
			// entity is whatever is using this 

//			if (entity.isFrozen())
//			{
//				return;
//			}
			recharged = 0;
			// entity.Freeze(recharge_time);
			int damageType = Static.DAMAGE_TYPE_NO_DAMAGE;
			if (entity.getTargetType() == Static.TARGET_TYPE_FRIENDLY){
				damageType = Static.DAMAGE_TYPE_FRIENDLY;
			}
			if (entity.getTargetType() == Static.TARGET_TYPE_ENEMY){
				damageType = Static.DAMAGE_TYPE_ENEMY;
			}
			Rectangle slashBounds = new Rectangle(	entity.getCenterX(), 
													entity.getCenterY(), 
													Static.PLAYER_WIDTH/2, 
													Static.PLAYER_HEIGHT/2);

			//Rectangle bulletBounds = new Rectangle(

			game.Spawn(new Bullet(game, game.getTestSprite(slashBounds, Color.Red), slashBounds, damage, damageType, 1, bulletSpeed, entity.alexDirection));

			// game sprite bounds amount dmgAmount dmgType duration bulletSpeed
		}

		public bool Available(Seizonsha game, GameEntity entity)
		{
			return recharged >= recharge_time;
		}

		public string getDescription()
		{
			return "A GUN";
		}

		public string getName()
		{
			return "Gun";
		}


		public void Update(Seizonsha game, GameEntity entity)
		{
			if (recharged < recharge_time)
			{
				recharged++;
			}
		}
	}
}
