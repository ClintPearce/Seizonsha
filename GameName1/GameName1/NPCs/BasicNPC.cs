﻿using GameName1.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameName1.NPCs
{
    class BasicNPC : GameEntity, AI
    {
        private int count = 0;

        public BasicNPC(Seizonsha game, Texture2D sprite, int x, int y, int width, int height)
            : base(game, sprite, x, y, width, height, Static.DAMAGE_TYPE_ENEMY, 20)
        {

        }

        public void AI()
        {
            //just sits there
        }

        public override void collide(GameEntity entity)
        {
            //Static.Debug("NPC collision with entity");
        }

        public override void collideWithWall()
        {
        }

        public override void OnSpawn()
        {

        }

        public override void Update()
        {
            
        }

        protected override void OnDie()
        {
            count++;
        }
    }
}
